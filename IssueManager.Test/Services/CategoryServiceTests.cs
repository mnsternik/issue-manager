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

public class CategoryServiceTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<CategoryService> _logger;


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

        SeedDatabase();
    }

    private void SeedDatabase()
    {
        _context.Database.EnsureDeleted();
        _context.Database.EnsureCreated();

        _context.Categories.AddRange(CategoryTestData.GetSampleCategories());
        _context.SaveChanges();
    }

    public void Dispose()
    {
        _context.Dispose();
    }

    [Fact]
    public async Task GetCategoriesAsync_WithSearch_ReturnsFilteredResults()
    {
        // Arrange
        var service = new CategoryService(_context, _mapper, _logger);
        var category = CategoryTestData.GetSampleCategories().First();
        int pageIndex = 1; 

        // Act
        var result = await service.GetCategoriesAsync(category.Name, pageIndex);

        // Assert
        Assert.Single(result.Categories);
        Assert.Equal(category.Name, result.Categories[0].Name);
    }

    [Fact]
    public async Task CreateCategoryAsync_DuplicateName_ThrowsException()
    {
        // Arrange
        var service = new CategoryService(_context, _mapper, _logger);
        var category = CategoryTestData.GetSampleCategories().First();
        var model = new CreateCategoryViewModel { Name = category.Name };

        // Act & Assert
        await Assert.ThrowsAsync<NameAlreadyExistsException>(() => service.CreateCategoryAsync(model));
    }

    [Fact]
    public async Task EditCategoryAsync_NewName_UpdatesSuccessfully()
    {
        // Arrange
        var service = new CategoryService(_context, _mapper, _logger);
        var category = await _context.Categories.FirstAsync(); 
        var newName = "Critical Bug";

        // Act
        category.Name = newName;
        await service.EditCategoryAsync(category);
        var updated = await _context.Categories.FindAsync(category.Id);

        // Assert
        Assert.Equal(newName, updated!.Name);
    }

    [Fact]
    public async Task DeleteCategoryAsync_ValidId_RemovesCategory()
    {
        // Arrange
        var service = new CategoryService(_context, _mapper, _logger);
        var category = CategoryTestData.GetSampleCategories().First();

        // Act
        await service.DeleteCategoryAsync(category.Id);
        var deleted = await _context.Categories.FindAsync(category.Id);

        // Assert
        Assert.Null(deleted);
    }
}