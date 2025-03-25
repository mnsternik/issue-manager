using AutoMapper;
using IssueManager.Data;
using IssueManager.Mapping;
using IssueManager.Models;
using IssueManager.Models.ViewModels.Users;
using IssueManager.Services.Users;
using IssueManager.Test.TestData;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;

namespace IssueManager.Tests.Services.Users
{
    public class UserServiceTests
    {
        private readonly Mock<UserManager<User>> _userManager;
        private readonly Mock<ILogger<UserService>> _logger;
        private readonly ApplicationDbContext _context;
        private readonly UserService _service;
        private readonly IMapper _mapper;

        public UserServiceTests()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            _context = new ApplicationDbContext(options);

            var store = new Mock<IUserStore<User>>();
            _userManager = new Mock<UserManager<User>>(store.Object, null!, null!, null!, null!, null!, null!, null!, null!);

            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile(new MappingProfile());
            });
            _mapper = config.CreateMapper();

            _logger = new Mock<ILogger<UserService>>();

            _service = new UserService(
                _context,
                _userManager.Object,
                _mapper,
                _logger.Object
            );

            SeedDatabase();
        }

        private void SeedDatabase()
        {
            _context.Database.EnsureDeleted();
            _context.Database.EnsureCreated();

            _context.Users.AddRange(UserTestData.GetSampleUsers());
            _context.Teams.AddRange(TeamTestData.GetSampleTeams());
            _context.SaveChanges();
        }

        [Fact]
        public async Task GetUsersAsync_ReturnsPaginatedList()
        {
            // Arrange
            string searchString = "";
            int pageIndex = 1;

            var expectedUsers = await _context.Users.ToListAsync();

            // Act
            var result = await _service.GetUsersAsync(searchString, pageIndex);

            // Assert
            Assert.Equal(expectedUsers.Count, result.Users.Count);
            Assert.Equal(pageIndex, result.Users.PageIndex);
        }

        [Fact]
        public async Task CreateUserAsync_ValidModel_CreatesUserWithRoles()
        {
            // Arrange
            var model = new CreateUserViewModel
            {
                Email = "new@test.com",
                Password = "P@ssw0rd!",
                RolesList = new NewUserRolesListViewModel
                {
                    SelectedRoles = ["Admin"]
                }
            };

            _userManager.Setup(m => m.CreateAsync(It.IsAny<User>(), model.Password))
                .ReturnsAsync(IdentityResult.Success);
            _userManager.Setup(m => m.AddToRoleAsync(It.IsAny<User>(), It.IsAny<string>()))
                .ReturnsAsync(IdentityResult.Success);

            // Act
            await _service.CreateUserAsync(model);

            // Assert
            _userManager.Verify(m => m.CreateAsync(It.IsAny<User>(), model.Password), Times.Once);
            _userManager.Verify(m => m.AddToRoleAsync(It.IsAny<User>(), "Admin"), Times.Once);
        }

        [Fact]
        public async Task GetChangeUserDetailsAsync_ValidId_ReturnsViewModel()
        {
            // Arrange
            var user = await _context.Users.FirstAsync();

            // Act
            var result = await _service.GetChangeUserDetailsAsync(user.Id);

            // Assert
            Assert.Equal(user.Id, result?.Id);
            Assert.Equal(user.Email, result?.Email);
        }

        [Fact]
        public async Task ChangeUserDetailsAsync_UpdatesUserData()
        {
            // Arrange
            var user = await _context.Users.FirstAsync();
            var model = new ChangeUserDetailsViewModel
            {
                Id = user.Id,
                TeamId = user.TeamId,
                Name = "Updated Name",
                Email = "updated@test.com"
            };

            // Act
            await _service.ChangeUserDetailsAsync(model);

            // Assert
            var updatedUser = await _context.Users.FindAsync(model.Id);
            Assert.Equal(model.Name, updatedUser?.Name);
            Assert.Equal(model.Email, updatedUser?.Email);
        }

        [Fact]
        public async Task GetChangeUserRolesAsync_ValidId_ReturnsViewModel()
        {
            // Arrange
            var user = await _context.Users.FirstAsync();
            var expectedModel = new ChangeUserRolesViewModel
            {
                Id = user.Id,
                Name = user.Name,
            };

            // Act
            var result = await _service.GetChangeUserRolesAsync(user.Id);

            // Assert
            Assert.Equal(expectedModel.Id, result?.Id);
            Assert.Equal(expectedModel.Name, result?.Name);
        }

        [Fact]
        public async Task ChangeUserRolesAsync_UpdatesRoles()
        {
            // Arrange
            var user = await _context.Users.FirstAsync();
            var model = new ChangeUserRolesViewModel
            {
                Id = user.Id,
                RolesList = new ExistingUserRolesListViewModel
                {
                    SelectedRoles = ["Admin"],
                    CurrentRoles = ["User"]
                }
            };

            _userManager.Setup(m => m.GetRolesAsync(user))
                .ReturnsAsync(model.RolesList.CurrentRoles);
            _userManager.Setup(m => m.RemoveFromRolesAsync(It.IsAny<User>(), It.IsAny<IEnumerable<string>>()))
                .ReturnsAsync(IdentityResult.Success);
            _userManager.Setup(m => m.AddToRolesAsync(It.IsAny<User>(), It.IsAny<IEnumerable<string>>()))
                .ReturnsAsync(IdentityResult.Success);

            // Act
            await _service.ChangeUserRolesAsync(model);

            // Assert
            _userManager.Verify(m => m.RemoveFromRolesAsync(user, model.RolesList.CurrentRoles), Times.Once);
            _userManager.Verify(m => m.AddToRolesAsync(user, model.RolesList.SelectedRoles), Times.Once);
        }

        [Fact]
        public async Task GetChangeUserPasswordAsync_ValidId_ReturnsViewModel()
        {
            // Arrange
            var user = await _context.Users.FirstAsync();
            var expectedModel = new ChangeUserPasswordViewModel
            {
                Id = user.Id,
                Name = user.Name
            };

            // Act
            var result = await _service.GetChangeUserPasswordAsync(user.Id);

            // Assert
            Assert.Equal(expectedModel.Id, result?.Id);
            Assert.Equal(expectedModel.Name, result?.Name);
        }

        [Fact]
        public async Task ChangeUserPasswordAsync_UpdatesPassword()
        {
            // Arrange
            var user = await _context.Users.FirstAsync();
            var model = new ChangeUserPasswordViewModel
            {
                Id = user.Id,
                Password = "NewP@ssw0rd!"
            };

            _userManager.Setup(m => m.RemovePasswordAsync(It.IsAny<User>()))
                .ReturnsAsync(IdentityResult.Success);
            _userManager.Setup(m => m.AddPasswordAsync(It.IsAny<User>(), model.Password))
                .ReturnsAsync(IdentityResult.Success);

            // Act
            await _service.ChangeUserPasswordAsync(model);

            // Assert
            _userManager.Verify(m => m.RemovePasswordAsync(user), Times.Once);
            _userManager.Verify(m => m.AddPasswordAsync(user, model.Password), Times.Once);
        }
    }
} 