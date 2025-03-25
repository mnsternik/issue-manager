using AutoMapper;
using IssueManager.Data;
using IssueManager.Exceptions;
using IssueManager.Mapping;
using IssueManager.Models.ViewModels.Categories;
using IssueManager.Services.Categories;
using IssueManager.Test.TestData;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;

namespace IssueManager.Test.Services;

public class CategoryServiceTests
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<CategoryService> _logger;
    private readonly CategoryService _service;

    public CategoryServiceTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _context = new ApplicationDbContext(options);

        var config = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile(new MappingProfile());
        });
        _mapper = config.CreateMapper();

        _logger = Mock.Of<ILogger<CategoryService>>();

        _service = new CategoryService(_context, _mapper, _logger);

        SeedDatabase();
    }

    private void SeedDatabase()
    {
        _context.Database.EnsureDeleted();
        _context.Database.EnsureCreated();

        _context.Categories.AddRange(CategoryTestData.GetSampleCategories());
        _context.SaveChanges();
    }

    [Fact]
    public async Task GetCategoriesAsync_WithSearch_ReturnsFilteredResults()
    {
        // Arrange
        var category = await _context.Categories.FirstAsync();
        int pageIndex = 1; 

        // Act
        var result = await _service.GetCategoriesAsync(category.Name, pageIndex);

        // Assert
        Assert.Single(result.Categories);
        Assert.Equal(category.Name, result.Categories[0].Name);
    }

    [Fact]
    public async Task CreateCategoryAsync_SuccessfullyAddsNewCategory()
    {
        // Arrange
        int initialCategoryCount = await _context.Categories.CountAsync();
        var newCategoryModel = new CreateCategoryViewModel { Name = "New category" };

        // Act
        await _service.CreateCategoryAsync(newCategoryModel);

        // Assert
        var categories = await _context.Categories.ToListAsync();
        var newCategory = categories.FirstOrDefault(c => c.Name == newCategoryModel.Name);

        Assert.NotNull(newCategory);
        Assert.Equal(initialCategoryCount + 1, categories.Count);
        Assert.Equal(newCategoryModel.Name, newCategory?.Name);
    }

    [Fact]
    public async Task CreateCategoryAsync_DuplicateName_ThrowsException()
    {
        // Arrange
        var category = await _context.Categories.FirstAsync();
        var model = new CreateCategoryViewModel { Name = category.Name };

        // Act
        var exception = await Assert.ThrowsAsync<NameAlreadyExistsException>(() => _service.CreateCategoryAsync(model));

        // Assert
        Assert.Equal("Category with this name already exists", exception.Message);
    }

    [Fact]
    public async Task EditCategoryAsync_NewName_UpdatesSuccessfully()
    {
        // Arrange
        var category = await _context.Categories.FirstAsync(); 
        var newName = "Critical Bug";

        // Act
        category.Name = newName;
        await _service.EditCategoryAsync(category);
        var updated = await _context.Categories.FindAsync(category.Id);

        // Assert
        Assert.Equal(newName, updated!.Name);
    }

    [Fact]
    public async Task DeleteCategoryAsync_ValidId_RemovesCategory()
    {
        // Arrange
        var category = await _context.Categories.FirstAsync();

        // Act
        await _service.DeleteCategoryAsync(category.Id);
        var deleted = await _context.Categories.FindAsync(category.Id);

        // Assert
        Assert.Null(deleted);
    }
}