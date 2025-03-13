using AutoMapper;
using IssueManager.Data;
using IssueManager.Exceptions;
using IssueManager.Mapping;
using IssueManager.Models.ViewModels.Teams;
using IssueManager.Services.Teams;
using IssueManager.Test.TestData;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;

namespace IssueManager.Test.Services
{
    public class TeamServiceTests
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly ILogger<TeamService> _logger;
        private readonly TeamService _service;

        public TeamServiceTests()
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

            _logger = Mock.Of<ILogger<TeamService>>();

            _service = new TeamService(_context, _mapper, _logger);

            SeedDatabase();
        }

        private void SeedDatabase()
        {
            _context.Database.EnsureDeleted();
            _context.Database.EnsureCreated();

            _context.Teams.AddRange(TeamTestData.GetSampleTeams());
            _context.SaveChanges();
        }

        [Fact]
        public async Task GetTeamsAsync_WithSearch_ReturnsFilteredResults()
        {
            // Arrange
            var team = await _context.Teams.FirstAsync();
            int pageIndex = 1;

            // Act
            var result = await _service.GetTeamsAsync(team.Name, pageIndex);

            // Assert
            Assert.Single(result.Teams);
            Assert.Equal(team.Name, result.Teams[0].Name);
        }

        [Fact]
        public async Task CreateTeamAsync_SuccessfullyAddsNewTeam()
        {
            // Arrange
            int initialTeamCount = await _context.Teams.CountAsync();
            var newTeamModel = new CreateTeamViewModel { Name = "New team" };

            // Act
            await _service.CreateTeamAsync(newTeamModel);

            // Assert
            var teams = await _context.Teams.ToListAsync();
            var newTeam = teams.FirstOrDefault(t => t.Name == newTeamModel.Name);

            Assert.NotNull(newTeam);
            Assert.Equal(initialTeamCount + 1, teams.Count);
            Assert.Equal(newTeamModel.Name, newTeam?.Name); 
        }

        [Fact]
        public async Task CreateTeamAsync_DuplicateName_ThrowsException()
        {
            // Arrange
            var team = await _context.Teams.FirstAsync();
            var model = new CreateTeamViewModel { Name = team.Name };

            // Act
            var exception = await Assert.ThrowsAsync<NameAlreadyExistsException>(() => _service.CreateTeamAsync(model));

            // Assert
            Assert.Equal("Team with this name already exists", exception.Message);
        }

        [Fact]
        public async Task EditTeamAsync_NewName_UpdatesSuccessfully()
        {
            // Arrange
            var team = await _context.Teams.FirstAsync();
            var newName = "New team name";

            // Act
            team.Name = newName;
            await _service.EditTeamAsync(team);
            var updated = await _context.Teams.FindAsync(team.Id);

            // Assert
            Assert.Equal(newName, updated!.Name);
        }

        [Fact]
        public async Task DeleteTeamAsync_ValidId_RemovesTeam()
        {
            // Arrange
            var team = await _context.Teams.FirstAsync();

            // Act
            await _service.DeleteTeamAsync(team.Id);
            var deleted = await _context.Teams.FindAsync(team.Id);

            // Assert
            Assert.Null(deleted);
        }

    }
}
