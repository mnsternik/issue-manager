using AutoMapper;
using IssueManager.Data;
using IssueManager.Exceptions;
using IssueManager.Mapping;
using IssueManager.Models;
using IssueManager.Models.ViewModels.Requests;
using IssueManager.Services.Files;
using IssueManager.Services.Requests;
using IssueManager.Test.TestData;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Xunit;

namespace IssueManager.Test.Services;

public class RequestServiceTests
{
    private readonly ApplicationDbContext _context;
    private readonly Mock<UserManager<User>> _userManager;
    private readonly Mock<IFileService> _fileService;
    private readonly IMapper _mapper;
    private readonly ILogger<RequestService> _logger;
    private readonly RequestService _service;

    public RequestServiceTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _context = new ApplicationDbContext(options);

        var store = new Mock<IUserStore<User>>();
        _userManager = new Mock<UserManager<User>>(store.Object, null!, null!, null!, null!, null!, null!, null!, null!);

        var config = new MapperConfiguration(cfg => cfg.AddProfile(new MappingProfile()));
        _mapper = config.CreateMapper();

        _fileService = new Mock<IFileService>();

        _logger = Mock.Of<ILogger<RequestService>>();
        _service = new RequestService(_context, _userManager.Object, _mapper, _fileService.Object, _logger);

        SeedDatabase();
    }

    private void SeedDatabase()
    {
        _context.Database.EnsureDeleted();
        _context.Database.EnsureCreated();

        _context.Users.AddRange(UserTestData.GetSampleUsers());
        _context.Categories.AddRange(CategoryTestData.GetSampleCategories());
        _context.Requests.AddRange(RequestTestData.GetSampleRequests());
        _context.SaveChanges();
    }

    [Fact]
    public async Task GetRequestsAsync_WithFilters_ReturnsFilteredResults()
    {
        // Arrange
        int pageIndex = 1; 
        var filters = new RequestSearchFilters { Status = RequestStatus.Open };

        // Act
        var result = await _service.GetRequestsAsync(filters, pageIndex);

        // Assert
        Assert.All(result.Requests, r => Assert.Equal(RequestStatus.Open, r.Status));
    }

    [Fact]
    public async Task CreateRequestAsync_ValidData_CreatesNewRequest()
    {
        // Arrange
        var initialCount = await _context.Requests.CountAsync();
        var user = await _context.Users.FirstAsync();
        var model = new CreateRequestViewModel
        {
            Title = "New Request",
            Description = "Test Description",
            Priority = RequestPriority.Medium,
            CategoryId = 1
        };

        _userManager.Setup(s => s.GetUserAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync(user);

        // Act
        var newId = await _service.CreateRequestAsync(model, GetTestUser());

        // Assert
        var newRequest = await _context.Requests.FindAsync(newId);
        Assert.NotNull(newRequest);
        Assert.Equal(initialCount + 1, await _context.Requests.CountAsync());
    }

    //[Fact]
    //public async Task CreateRequestAsync_InvalidCategory_ThrowsException()
    //{
    //    // Arrange
    //    var model = new CreateRequestViewModel
    //    {
    //        Title = "Invalid Category",
    //        Description = "Test",
    //        Priority = RequestPriority.Low,
    //        CategoryId = 999 // Non-existent
    //    };

    //    // Act & Assert
    //    await Assert.ThrowsAsync<NotFoundException>(() =>
    //        _service.CreateRequestAsync(model, GetTestUser()));
    //}

    [Fact]
    public async Task AssignRequestAsync_ValidRequest_UpdatesAssignment()
    {
        // Arrange
        var request = await _context.Requests.FirstAsync();
        var user = await _context.Users.FirstAsync();
        _userManager.Setup(s => s.GetUserAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync(user);

        // Act
        await _service.AssignRequestAsync(request.Id, GetTestUser());

        // Assert
        var updated = await _context.Requests.FindAsync(request.Id);
        Assert.Equal(user.Id, updated!.AssignedUserId);
    }

    [Fact]
    public async Task AddResponseAsync_ValidRequest_AddsResponse()
    {
        // Arrange
        var request = await _context.Requests.FirstAsync();
        var user = await _context.Users.FirstAsync();

        var initialCount = request.Responses?.Count ?? 0;

        _userManager.Setup(s => s.GetUserAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync(user);

        // Act
        await _service.AddResponseAsync(request.Id, "Test response", GetTestUser());

        // Assert
        var updated = await _context.Requests
            .Include(r => r.Responses)
            .FirstAsync(r => r.Id == request.Id);
        Assert.Equal(initialCount + 1, updated.Responses!.Count);
    }

    [Fact]
    public async Task EditRequestAsync_ValidData_UpdatesRequest()
    {
        // Arrange
        var request = await _context.Requests.FirstAsync();
        var model = _mapper.Map<EditRequestViewModel>(request);
        model.Priority = RequestPriority.Low;
        model.Status = RequestStatus.InProgress;

        // Act
        await _service.EditRequestAsync(model);

        // Assert
        var updated = await _context.Requests.FirstAsync(r => r.Id == request.Id);
        Assert.Equal(RequestPriority.Low, updated.Priority);
        Assert.Equal(RequestStatus.InProgress, updated.Status);
    }

    [Fact]
    public async Task GetRequestDetailsAsync_InvalidId_ReturnsNull()
    {
        // Act
        var result = await _service.GetRequestDetailsAsync(999, GetTestUser());

        // Assert
        Assert.Null(result);
    }

    private ClaimsPrincipal GetTestUser()
    {
        return new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, "test-user-id"),
            new Claim(ClaimTypes.Name, "Test User")
        }));
    }
}