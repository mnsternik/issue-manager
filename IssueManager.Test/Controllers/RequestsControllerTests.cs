using IssueManager.Controllers;
using IssueManager.Models.ViewModels.Requests;
using IssueManager.Models.ViewModels.Teams;
using IssueManager.Services.DataLists;
using IssueManager.Services.Requests;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
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
            var actionResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", actionResult.ActionName);
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
    }
}
