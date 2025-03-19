using IssueManager.Controllers;
using IssueManager.Exceptions;
using IssueManager.Models;
using IssueManager.Models.ViewModels.Categories;
using IssueManager.Models.ViewModels.Requests;
using IssueManager.Models.ViewModels.Teams;
using IssueManager.Services.DataLists;
using IssueManager.Services.Requests;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.DataCollection;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace IssueManager.Test.Controllers
{
    public class RequestsControllerTests
    {
        private readonly Mock<IRequestService> _mockRequestService;
        private readonly Mock<ISelectListService> _mockSelectListService;
        private readonly RequestsController _controller;

        const int requestId = 999;  

        public RequestsControllerTests()
        {
            _mockRequestService = new Mock<IRequestService>();
            _mockSelectListService = new Mock<ISelectListService>();

            _controller = new RequestsController(
                _mockRequestService.Object,
                _mockSelectListService.Object
            );

            _controller.TempData = new TempDataDictionary(new DefaultHttpContext(), new Mock<ITempDataProvider>().Object);

            // Set up default HttpContext with a mocked User
            var httpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, "test-user-id"),
                    new Claim(ClaimTypes.Name, "Test User"),
                    new Claim(ClaimTypes.Email, "test@example.com")
                }, "mock"))
            };

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };
        }

        [Fact]
        public async Task Index_ReturnsViewWithRequests()
        {
            // Arrange
            int pageIndex = 1;
            var filters = new RequestSearchFilters();
            var expectedModel = new RequestsListViewModel();

            _mockRequestService.Setup(s => s.GetRequestsAsync(filters, pageIndex)).ReturnsAsync(expectedModel);

            // Mock HttpContext and set QueryCollection with pageIndex=1
            var httpContext = new DefaultHttpContext();
            var queryCollection = new QueryCollection(new Dictionary<string, Microsoft.Extensions.Primitives.StringValues>
            {
                { "pageIndex", pageIndex.ToString() }
            });
            httpContext.Request.Query = queryCollection;

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };

            // Act
            var result = await _controller.Index(filters, pageIndex);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal(expectedModel, viewResult.Model);
        }

        [Fact]
        public async Task Index_RedirectsToItsfelWhenSomeFiltersExists()
        {
            // Arrange
            int pageIndex = 1;
            var filters = new RequestSearchFilters { Title = "test " };
            var expectedModel = new RequestsListViewModel();

            _mockRequestService.Setup(s => s.GetRequestsAsync(filters, pageIndex)).ReturnsAsync(expectedModel);

            // Mock HttpContext and set QueryCollection with pageIndex=1
            var httpContext = new DefaultHttpContext();
            var queryCollection = new QueryCollection(new Dictionary<string, Microsoft.Extensions.Primitives.StringValues>
            {
                { "pageIndex", pageIndex.ToString() }
            });
            httpContext.Request.Query = queryCollection;

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };

            // Act
            var result = await _controller.Index(filters, pageIndex);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectResult.ActionName);
        }

        [Fact]
        public async Task Details_ReturnsViewWithRequest()
        {
            // Arrange
            var expectedModel = new DetailsRequestViewModel();

            _mockRequestService.Setup(s => s.GetRequestDetailsAsync(It.IsAny<int>(), _controller.User)).ReturnsAsync(expectedModel);

            // Act
            var result = await _controller.Details(999);

            // Assert 
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal(expectedModel, viewResult.Model);
        }

        [Fact]
        public async Task Create_Post_ValidModel_RedirectsToRequest()
        {
            // Arrange
            var model = new CreateRequestViewModel();

            _mockRequestService.Setup(s => s.CreateRequestAsync(model, _controller.User)).ReturnsAsync(requestId);

            // Act
            var result = await _controller.Create(model);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Details", redirectResult.ActionName);
            Assert.Equal(999, redirectResult.RouteValues?["id"]);
        }

        [Fact]
        public async Task Create_Post_ReturnsView_WhenModelStateHasErrors()
        {
            // Arrange
            var model = new CreateRequestViewModel();

            _controller.ModelState.AddModelError("Title", "Title is required");

            // Act
            var result = await _controller.Create(model);

            // Assert
            Assert.False(_controller.ModelState.IsValid);
            Assert.Single(_controller.ModelState["Title"]!.Errors);
            Assert.Equal("Title is required", _controller.ModelState["Title"]!.Errors[0].ErrorMessage);

            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal(model, viewResult.Model); 
        }

        [Fact]
        public async Task Create_Post_FileProcessingException_AddsModelErrorAndReturnsView()
        {
            // Arrange
            string exMessage = "File processing error";
            var model = new CreateRequestViewModel();

            _mockRequestService.Setup(s => s.CreateRequestAsync(model, _controller.User)).ThrowsAsync(new FileProcessingException(exMessage));

            // Act
            var result = await _controller.Create(model);

            // Assert
            Assert.False(_controller.ModelState.IsValid);
            Assert.Single(_controller.ModelState[string.Empty]!.Errors);
            Assert.Equal(exMessage, _controller.ModelState[string.Empty]!.Errors[0].ErrorMessage);

            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal(model, viewResult.Model);
        }

        [Fact]
        public async Task DownloadFile_ReturnsFileResultWhenFileExists()
        {
            // Arrange
            var file = new Attachment { FileData = new byte[] { 0x12 }, ContentType = "text/plain", FileName = "test.txt" };
            _mockRequestService.Setup(s => s.GetAttachmentAsync(It.IsAny<int>())).ReturnsAsync(file);

            // Act
            var result = await _controller.DownloadFile(999);

            // Assert
            var fileResult = Assert.IsType<FileContentResult>(result);
            Assert.Equal(file.FileData, fileResult.FileContents);
            Assert.Equal(file.ContentType, fileResult.ContentType);
            Assert.Equal(file.FileName, fileResult.FileDownloadName);
        }

        [Fact]
        public async Task DownloadFile_ReturnsNotFoundWhenFileDoesNotExist()
        {
            // Arrange
            _mockRequestService.Setup(s => s.GetAttachmentAsync(It.IsAny<int>())).ReturnsAsync((Attachment?)null);

            // Act
            var result = await _controller.DownloadFile(999);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Assign_Post_RedirectsToDetailsOnSuccess()
        {
            // Arrange
            _mockRequestService.Setup(s => s.AssignRequestAsync(requestId, It.IsAny<ClaimsPrincipal>())).Returns(Task.CompletedTask);

            // Act
            var result = await _controller.Assign(requestId);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Details", redirectResult.ActionName);
            Assert.Equal(requestId, redirectResult.RouteValues?["id"]);
        }

        [Fact]
        public async Task Assign_Post_ReturnsNotFoundWhenInvalidOperationException()
        {
            // Arrange
            _mockRequestService.Setup(s => s.AssignRequestAsync(requestId, It.IsAny<ClaimsPrincipal>())).ThrowsAsync(new InvalidOperationException());

            // Act
            var result = await _controller.Assign(requestId);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Assign_Post_SetsTempDataOnConcurrencyException()
        {
            // Arrange
            _mockRequestService.Setup(s => s.AssignRequestAsync(requestId, It.IsAny<ClaimsPrincipal>())).ThrowsAsync(new DbUpdateConcurrencyException());

            // Act
            var result = await _controller.Assign(requestId);

            // Assert
            Assert.True(_controller.TempData.ContainsKey("ErrorMessage"));
            Assert.Equal("This record was edited by another user. Please refresh the page and try again.", _controller.TempData["ErrorMessage"]);
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Details", redirectResult.ActionName);
        }

        [Fact]
        public async Task Edit_Get_ReturnsViewWithModel()
        {
            // Arrange
            var expectedModel = new EditRequestViewModel { Id = requestId };
            _mockRequestService.Setup(s => s.GetEditRequestAsync(requestId)).ReturnsAsync(expectedModel);
            _mockSelectListService.Setup(s => s.PopulateRequestSelectLists(It.IsAny<string?>(), It.IsAny<int?>(), It.IsAny<int?>())).Returns(new RequestsSelectListsViewModel());

            // Act
            var result = await _controller.Edit(requestId);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal(expectedModel, viewResult.Model);
            Assert.NotNull(((EditRequestViewModel)viewResult.Model!).SelectLists);
        }

        [Fact]
        public async Task Edit_Get_ReturnsNotFoundWhenIdIsNull()
        {
            // Act
            var result = await _controller.Edit(null);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Edit_Get_ReturnsNotFoundWhenRequestNotFound()
        {
            // Arrange
            _mockRequestService.Setup(s => s.GetEditRequestAsync(requestId)).ReturnsAsync((EditRequestViewModel?)null);

            // Act
            var result = await _controller.Edit(requestId);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Edit_Post_RedirectsToDetailsOnSuccess()
        {
            // Arrange
            var model = new EditRequestViewModel { Id = requestId };
            _mockRequestService.Setup(s => s.EditRequestAsync(model)).Returns(Task.CompletedTask);

            // Act
            var result = await _controller.Edit(requestId, model);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Details", redirectResult.ActionName);
            Assert.Equal(requestId, redirectResult.RouteValues?["id"]);
        }

        [Fact]
        public async Task Edit_Post_AddsModelErrorOnConcurrencyException()
        {
            // Arrange
            var model = new EditRequestViewModel { Id = requestId };
            _mockRequestService.Setup(s => s.EditRequestAsync(model)).ThrowsAsync(new DbUpdateConcurrencyException());

            // Act
            var result = await _controller.Edit(requestId, model);

            // Assert
            Assert.False(_controller.ModelState.IsValid);
            Assert.Single(_controller.ModelState[string.Empty]!.Errors);
            Assert.Equal("This record was edited by another user. Please refresh the page and try again.", _controller.ModelState[string.Empty]!.Errors[0].ErrorMessage);
        }

        [Fact]
        public async Task Edit_Post_ReturnsViewWithModelWhenModelStateInvalid()
        {
            // Arrange
            var model = new EditRequestViewModel { Id = requestId };
            _controller.ModelState.AddModelError("Title", "Title is required");

            _mockSelectListService.Setup(s => s.PopulateRequestSelectLists(
                It.IsAny<string?>(),
                It.IsAny<int?>(),
                It.IsAny<int?>()
            )).Returns(new RequestsSelectListsViewModel());

            // Act
            var result = await _controller.Edit(requestId, model);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal(model, viewResult.Model);
            Assert.NotNull(model.SelectLists);
        }

        [Fact]
        public async Task Edit_Post_ReturnsNotFoundWhenIdMismatch()
        {
            // Arrange
            var model = new EditRequestViewModel { Id = 1000 }; // Different ID

            // Act
            var result = await _controller.Edit(requestId, model);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task AddResponse_Post_RedirectsToEdit()
        {
            // Arrange
            string responseText = "Test response";
            _mockRequestService.Setup(s => s.AddResponseAsync(requestId, responseText, It.IsAny<ClaimsPrincipal>())).Returns(Task.CompletedTask);

            // Act
            var result = await _controller.AddResponse(requestId, responseText);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Edit", redirectResult.ActionName);
            Assert.Equal(requestId, redirectResult.RouteValues?["id"]);
        }

        [Fact]
        public async Task AddResponse_Post_SetsTempDataOnException()
        {
            // Arrange
            string responseText = "Test response";
            _mockRequestService.Setup(s => s.AddResponseAsync(requestId, responseText, It.IsAny<ClaimsPrincipal>())).ThrowsAsync(new Exception());

            // Act
            var result = await _controller.AddResponse(requestId, responseText);

            // Assert
            Assert.True(_controller.TempData.ContainsKey("ErrorMessage"));
            Assert.Equal("Error occurs while trying to create response.", _controller.TempData["ErrorMessage"]);
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Edit", redirectResult.ActionName);
        }
    }
}
