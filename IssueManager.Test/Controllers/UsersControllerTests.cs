using IssueManager.Controllers;
using IssueManager.Models.ViewModels.Users;
using IssueManager.Services.DataLists;
using IssueManager.Services.Users;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace IssueManager.Tests.Controllers
{
    public class UsersControllerTests
    {
        private readonly Mock<IUserService> _mockUserService;
        private readonly Mock<ISelectListService> _mockSelectListService;
        private readonly Mock<IRoleListService> _mockRoleListService;
        private readonly UsersController _controller;

        public UsersControllerTests()
        {
            _mockUserService = new Mock<IUserService>();
            _mockSelectListService = new Mock<ISelectListService>();
            _mockRoleListService = new Mock<IRoleListService>();

            _controller = new UsersController(
                _mockUserService.Object,
                _mockSelectListService.Object,
                _mockRoleListService.Object
            );

            _controller.TempData = new TempDataDictionary(new DefaultHttpContext(), new Mock<ITempDataProvider>().Object);
        }

        [Fact]
        public async Task Index_ReturnsViewWithPaginatedUsers()
        {
            // Arrange
            string searchString = "";
            int pageIndex = 1; 

            var expectedModel = new UsersListViewModel();

            _mockUserService.Setup(s => s.GetUsersAsync(searchString, pageIndex)).ReturnsAsync(expectedModel);

            // Act
            var result = await _controller.Index(searchString, pageIndex);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal(expectedModel, viewResult.Model);
        }

        [Fact]
        public void Create_GET_PopulatesSelectLists()
        {
            // Arrange
            var expectedTeams = new List<SelectListItem> { new SelectListItem { Text = "Team 1", Value = "1" } };
            var expectedRoles = new NewUserRolesListViewModel { AvailableRoles = ["role1", "role2"] };
            
            _mockSelectListService.Setup(s => s.PopulateTeamSelectList(null))
                .Returns(expectedTeams);
            _mockRoleListService.Setup(s => s.PopulateNewUserRolesList(null))
                .Returns(expectedRoles);

            // Act
            var result = _controller.Create();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<CreateUserViewModel>(viewResult.Model);
            Assert.Equal(expectedTeams, model.TeamSelectOptions);
            Assert.Equal(expectedRoles, model.RolesList);
        }

        [Fact]
        public async Task Create_POST_ValidModel_RedirectsToIndexAndShowsMessage()
        {
            // Arrange
            var model = new CreateUserViewModel
            {
                Email = "user@test.com",
                Password = "P@ssw0rd!",
                TeamId = 1
            };

            // Act
            var result = await _controller.Create(model);

            // Assert
            var actionResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", actionResult.ActionName);
            Assert.Equal("New account created successfully!", _controller.TempData["SuccessMessage"]);
        }

        [Fact]
        public async Task ChangeUserDetails_GET_ValidId_ReturnsView()
        {
            // Arrange
            var expectedModel = new ChangeUserDetailsViewModel();

            _mockUserService.Setup(s => s.GetChangeUserDetailsAsync(It.IsAny<string>()))
                .ReturnsAsync(expectedModel);

            // Act
            var result = await _controller.ChangeUserDetails(new Guid().ToString());

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal(expectedModel, viewResult.Model);
        }

        [Fact]
        public async Task ChangeUserDetails_POST_ConcurrencyError_AddsModelError()
        {
            // Arrange
            var model = new ChangeUserDetailsViewModel { Id = "1" };

            _mockUserService.Setup(s => s.ChangeUserDetailsAsync(model))
                .ThrowsAsync(new DbUpdateConcurrencyException());

            // Act
            await _controller.ChangeUserDetails(model);

            // Assert
            Assert.True(_controller.ModelState.ErrorCount > 0);
            Assert.Contains("refresh", _controller.ModelState[""]?.Errors[0].ErrorMessage);
        }

        [Fact]
        public async Task ChangeUserPassword_GET_ValidId_ReturnsView()
        {
            // Arrange
            var expectedModel = new ChangeUserPasswordViewModel();

            _mockUserService.Setup(s => s.GetChangeUserPasswordAsync(It.IsAny<string>()))
                .ReturnsAsync(expectedModel);

            // Act
            var result = await _controller.ChangeUserPassword(new Guid().ToString());

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal(expectedModel, viewResult.Model);
        }

        [Fact]
        public async Task ChangeUserPassword_POST_ValidModel_RedirectsToIndexAndShowsMessage()
        {
            // Arrange
            var model = new ChangeUserPasswordViewModel
            {
                Id = "1",
                Password = "NewP@ssw0rd!"
            };

            // Act
            var result = await _controller.ChangeUserPassword(model);

            // Assert
            var actionResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", actionResult.ActionName);
            Assert.Equal("Password changed successfully.", _controller.TempData["SuccessMessage"]);
        }

        [Fact]
        public async Task ChangeUserRoles_GET_ValidId_ReturnsView()
        {
            // Arrange
            var expectedModel = new ChangeUserRolesViewModel();

            _mockUserService.Setup(s => s.GetChangeUserRolesAsync(It.IsAny<string>()))
                .ReturnsAsync(expectedModel);

            // Act
            var result = await _controller.ChangeUserRoles(new Guid().ToString());

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal(expectedModel, viewResult.Model);
        }

        [Fact]
        public async Task ChangeUserRoles_POST_ValidModel_RedirectsToIndexAndShowsMessage()
        {
            // Arrange
            var model = new ChangeUserRolesViewModel
            {
                Id = "1",
                RolesList = new ExistingUserRolesListViewModel()
            };

            // Act
            var result = await _controller.ChangeUserRoles(model);

            // Assert
            var actionResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", actionResult.ActionName);
            Assert.Equal("User roles updated successfully.", _controller.TempData["SuccessMessage"]);
        }
    }
}