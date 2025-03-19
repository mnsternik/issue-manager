using IssueManager.Controllers;
using IssueManager.Exceptions;
using IssueManager.Models;
using IssueManager.Models.ViewModels.Teams;
using IssueManager.Services.Teams;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace IssueManager.Test.Controllers
{
    public class TeamsControllerTests
    {

        private readonly Mock<ITeamService> _mockTeamService;
        private readonly TeamsController _controller;

        public TeamsControllerTests()
        {
            _mockTeamService = new Mock<ITeamService>();
            _controller = new TeamsController(_mockTeamService.Object);
            _controller.TempData = new TempDataDictionary(new DefaultHttpContext(), Mock.Of<ITempDataProvider>());
        }

        [Fact]
        public async Task Index_ReturnsViewWithTeams()
        {
            // Arrange
            int pageIndex = 1;
            string searchString = "";

            var expectedModel = new TeamsListViewModel();
            _mockTeamService.Setup(s => s.GetTeamsAsync(searchString, pageIndex)).ReturnsAsync(expectedModel);

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
            var model = new CreateTeamViewModel { Name = "Test" };

            // Act
            var result = await _controller.Create(model);

            // Assert
            var actionResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", actionResult.ActionName);
            Assert.Equal("New team created successfully!", _controller.TempData["SuccessMessage"]);
        }

        [Fact]
        public async Task Create_Post_DuplicateName_ReturnsViewWithError()
        {
            // Arrange
            var model = new CreateTeamViewModel { Name = "Duplicate" };
            _mockTeamService.Setup(s => s.CreateTeamAsync(model)).ThrowsAsync(new NameAlreadyExistsException("Name already exists"));

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
            var model = new Team { Id = 1, Name = "Updated" };

            // Act
            var result = await _controller.Edit(1, model);

            // Assert
            var actionResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", actionResult.ActionName);
            Assert.Equal("Team updated successfully!", _controller.TempData["SuccessMessage"]);
        }

        [Fact]
        public async Task Edit_Post_DuplicateName_ReturnsViewWithError()
        {
            // Arrange
            var model = new Team { Id = 1, Name = "Duplicated" };
            _mockTeamService.Setup(s => s.EditTeamAsync(model)).ThrowsAsync(new NameAlreadyExistsException("Name already exists"));

            // Act
            var result = await _controller.Edit(1, model);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal(1, _controller.ModelState.ErrorCount); 
        }

        [Fact]
        public async Task Delete_ValidId_ReturnsViewWithTeam()
        {
            // Arrange
            var model =  new Team { Id = 1, Name = "Test" }; 
            _mockTeamService.Setup(s => s.GetTeamAsync(model.Id)).ReturnsAsync(model);

            // Act
            var result = await _controller.Delete(model.Id);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal(model, viewResult.Model); 
        }

        [Fact]
        public async Task Delete_InvalidId_ReturnsNotFound()
        {
            // Arrange
            _mockTeamService.Setup(s => s.GetTeamAsync(1)).ReturnsAsync((Team?)null);

            // Act
            var result = await _controller.Delete(1);

            // Assert
            Assert.IsType<NotFoundResult>(result); 
        }

        [Fact]
        public async Task DeleteConfirmed_ValidId_RedirectsToIndexWithSuccess()
        {
            // Arrange
            _mockTeamService.Setup(s => s.DeleteTeamAsync(1)).Returns(Task.CompletedTask);

            // Act
            var result = await _controller.DeleteConfirmed(1);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectResult.ActionName);
            Assert.Equal("Team deleted successfully!", _controller.TempData["SuccessMessage"]);
        }

        [Fact]
        public async Task DeleteConfirmed_ConcurrencyError_RedirectsWithErrorMessage()
        {
            // Arrange
            _mockTeamService.Setup(s => s.DeleteTeamAsync(1)).ThrowsAsync(new DbUpdateConcurrencyException());

            // Act
            var result = await _controller.DeleteConfirmed(1);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectResult.ActionName);
            Assert.Equal("This record was edited by another user. Please refresh the page and try again.", _controller.TempData["ErrorMessage"]);
        }
    }
}
