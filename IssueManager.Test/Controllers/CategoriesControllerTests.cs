using IssueManager.Controllers;
using IssueManager.Exceptions;
using IssueManager.Models;
using IssueManager.Models.ViewModels.Categories;
using IssueManager.Services.Categories;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace IssueManager.Tests.Controllers;

public class CategoriesControllerTests
{
    private readonly Mock<ICategoryService> _mockService;
    private readonly CategoriesController _controller;

    const int pageIndex = 1;

    public CategoriesControllerTests()
    {
        _mockService = new Mock<ICategoryService>();
        _controller = new CategoriesController(_mockService.Object);
        _controller.TempData = new TempDataDictionary(new DefaultHttpContext(), Mock.Of<ITempDataProvider>());
    }

    [Fact]
    public async Task Index_ReturnsViewWithCategories()
    {
        // Arrange
        var expectedModel = new CategoriesListViewModel();
        _mockService.Setup(s => s.GetCategoriesAsync(null, pageIndex)).ReturnsAsync(expectedModel);

        // Act
        var result = await _controller.Index(null, pageIndex);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal(expectedModel, viewResult.Model);
    }

    [Fact]
    public async Task Create_Post_ValidModel_RedirectsToIndex()
    {
        // Arrange
        var model = new CreateCategoryViewModel { Name = "Test" };

        // Act
        var result = await _controller.Create(model);

        // Assert
        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", redirectResult.ActionName);
        Assert.Equal("New category created successfully!", _controller.TempData["SuccessMessage"]);
    }

    [Fact]
    public async Task Create_Post_DuplicateName_ReturnsViewWithError()
    {
        // Arrange
        var model = new CreateCategoryViewModel { Name = "Duplicate" };
        _mockService.Setup(s => s.CreateCategoryAsync(model)).ThrowsAsync(new NameAlreadyExistsException("Name already exists"));

        // Act
        var result = await _controller.Create(model);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.True(_controller.ModelState.ErrorCount == 1);
        Assert.Equal("Name", _controller.ModelState.Keys.First());
    }

    [Fact]
    public async Task Edit_Post_ValidModel_RedirectsToIndex()
    {
        // Arrange
        var category = new Category { Id = 1, Name = "Updated" };

        // Act
        var result = await _controller.Edit(1, category);

        // Assert
        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", redirectResult.ActionName);
    }

    [Fact]
    public async Task Delete_ValidId_ReturnsViewWithCategory()
    {
        // Arrange
        var mockService = new Mock<ICategoryService>();
        var category = new Category { Id = 1, Name = "Bug" };
        mockService.Setup(s => s.GetCategoryAsync(1)).ReturnsAsync(category);

        var controller = new CategoriesController(mockService.Object);

        // Act
        var result = await controller.Delete(1);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal(category, viewResult.Model);
    }

    [Fact]
    public async Task Delete_InvalidId_ReturnsNotFound()
    {
        // Arrange
        var mockService = new Mock<ICategoryService>();
        mockService.Setup(s => s.GetCategoryAsync(999)).ReturnsAsync((Category?)null);

        var controller = new CategoriesController(mockService.Object);

        // Act
        var result = await controller.Delete(999);

        // Assert
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task DeleteConfirmed_ValidId_RedirectsToIndexWithSuccess()
    {
        // Arrange
        var mockService = new Mock<ICategoryService>();
        mockService.Setup(s => s.DeleteCategoryAsync(1)).Returns(Task.CompletedTask);

        var controller = new CategoriesController(mockService.Object)
        {
            TempData = new TempDataDictionary(new DefaultHttpContext(), Mock.Of<ITempDataProvider>())
        };

        // Act
        var result = await controller.DeleteConfirmed(1);

        // Assert
        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", redirectResult.ActionName);
        Assert.Equal("Category deleted successfully!", controller.TempData["SuccessMessage"]);
    }

    [Fact]
    public async Task DeleteConfirmed_ConcurrencyError_RedirectsWithErrorMessage()
    {
        // Arrange
        var mockService = new Mock<ICategoryService>();
        mockService.Setup(s => s.DeleteCategoryAsync(1))
            .ThrowsAsync(new DbUpdateConcurrencyException());

        var controller = new CategoriesController(mockService.Object)
        {
            TempData = new TempDataDictionary(new DefaultHttpContext(), Mock.Of<ITempDataProvider>())
        };

        // Act
        var result = await controller.DeleteConfirmed(1);

        // Assert
        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", redirectResult.ActionName);

        // Check ModelState error
        Assert.True(controller.ModelState.ContainsKey(""));
        Assert.Equal(
            "This record was edited by another user. Please refresh the page and try again.",
            controller.ModelState[""].Errors[0].ErrorMessage
        );
    }
}