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
        const int pageIndex = 1;
        const string searchString = "";

        var expectedModel = new CategoriesListViewModel();
        _mockService.Setup(s => s.GetCategoriesAsync(searchString, pageIndex)).ReturnsAsync(expectedModel);

        // Act
        var result = await _controller.Index(searchString, pageIndex);

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
        Assert.Equal(1, _controller.ModelState.ErrorCount);
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
        Assert.Equal("Category updated successfully!", _controller.TempData["SuccessMessage"]);
    }

    [Fact]
    public async Task Edit_Post_DuplicateName_ReturnsViewWithError()
    {
        // Arrange
        var model = new Category { Id = 1, Name = "Duplicated" };
        _mockService.Setup(s => s.EditCategoryAsync(model)).ThrowsAsync(new NameAlreadyExistsException("Name already exists"));

        // Act
        var result = await _controller.Edit(1, model);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal(1, _controller.ModelState.ErrorCount);
    }

    [Fact]
    public async Task Delete_ValidId_ReturnsViewWithCategory()
    {
        // Arrange
        var model = new Category { Id = 1, Name = "Bug" };
        _mockService.Setup(s => s.GetCategoryAsync(1)).ReturnsAsync(model);

        // Act
        var result = await _controller.Delete(1);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal(model, viewResult.Model);
    }

    [Fact]
    public async Task Delete_InvalidId_ReturnsNotFound()
    {
        // Arrange
        _mockService.Setup(s => s.GetCategoryAsync(1)).ReturnsAsync((Category?)null);

        // Act
        var result = await _controller.Delete(1);

        // Assert
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task DeleteConfirmed_ValidId_RedirectsToIndexWithSuccess()
    {
        // Arrange
        _mockService.Setup(s => s.DeleteCategoryAsync(1)).Returns(Task.CompletedTask);

        // Act
        var result = await _controller.DeleteConfirmed(1);

        // Assert
        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", redirectResult.ActionName);
        Assert.Equal("Category deleted successfully!", _controller.TempData["SuccessMessage"]);
    }

    [Fact]
    public async Task DeleteConfirmed_ConcurrencyError_RedirectsWithErrorMessage()
    {
        // Arrange
        _mockService.Setup(s => s.DeleteCategoryAsync(1)).ThrowsAsync(new DbUpdateConcurrencyException());

        // Act
        var result = await _controller.DeleteConfirmed(1);

        // Assert
        var actionResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", actionResult.ActionName);
        Assert.Equal("This record was edited by another user. Please refresh the page and try again.", _controller.TempData["ErrorMessage"]);
    }
}