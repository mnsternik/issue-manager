using AutoMapper;
using IssueManager.Data;
using IssueManager.Exceptions;
using IssueManager.Models;
using IssueManager.Models.ViewModels.Teams;
using IssueManager.Utilities;
using Microsoft.EntityFrameworkCore;


namespace IssueManager.Services.Teams
{
    public class TeamService : ITeamService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly ILogger<TeamService> _logger;

        public TeamService(ApplicationDbContext context, IMapper mapper, ILogger<TeamService> logger)
        {
            _context = context;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<TeamsListViewModel> GetTeamsAsync(string search, int pageIndex)
        {
            _logger.LogInformation("Retrieving teams with search: {Search}, page index: {PageIndex}", search, pageIndex);

            try
            {
                IQueryable<Team> query = _context.Teams.AsNoTracking();

                if (!string.IsNullOrEmpty(search))
                {
                    _logger.LogDebug("Applying team search filter: {Search}", search);
                    query = query.Where(t => t.Name.ToLower().Contains(search.ToLower()));
                }

                IQueryable<TeamsListItemViewModel> mappedQuery = _mapper.ProjectTo<TeamsListItemViewModel>(query);

                var teamsViewModel = new TeamsListViewModel
                {
                    Teams = await PaginatedList<TeamsListItemViewModel>.CreateAsync(mappedQuery, pageIndex),
                    SearchString = search
                };

                _logger.LogInformation("Returned {TeamCount} teams", teamsViewModel.Teams.Count);
                return teamsViewModel;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving teams with search: {Search}", search);
                throw;
            }
        }

        public async Task CreateTeamAsync(CreateTeamViewModel teamViewModel)
        {
            _logger.LogInformation("Creating new team: {TeamName}", teamViewModel.Name);

            try
            {
                if (TeamNameExists(teamViewModel.Name))
                {
                    _logger.LogWarning("Team creation failed - name already exists: {TeamName}", teamViewModel.Name);
                    throw new NameAlreadyExistsException("Team with this name already exists");
                }

                var team = _mapper.Map<Team>(teamViewModel);

                _context.Add(team);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Successfully created team {TeamId}: {TeamName}", team.Id, team.Name);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating team {TeamName}", teamViewModel.Name);
                throw;
            }
        }

        public async Task<Team?> GetTeamAsync(int id)
        {
            _logger.LogInformation("Retrieving team {TeamId}", id);

            try
            {
                var team = await _context.Teams.FindAsync(id);

                if (team == null)
                {
                    _logger.LogWarning("Team {TeamId} not found", id);
                }

                return team;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving team {TeamId}", id);
                throw;
            }
        }

        public async Task EditTeamAsync(Team team)
        {
            _logger.LogInformation("Updating team {TeamId}: {TeamName}", team.Id, team.Name);

            try
            {
                if (TeamNameExists(team.Name))
                {
                    _logger.LogWarning("Team update failed - name already exists: {TeamName}", team.Name);
                    throw new NameAlreadyExistsException("Team with this name already exists");
                }

                _context.Update(team);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Successfully updated team {TeamId}", team.Id);
            }
            catch (DbUpdateConcurrencyException ex)
            {
                if (!TeamIdExists(team.Id))
                {
                    _logger.LogWarning("Concurrency conflict - team {TeamId} was deleted by another user", team.Id);
                }
                else
                {
                    _logger.LogError(ex, "Concurrency conflict while updating team {TeamId}", team.Id);
                }
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating team {TeamId}", team.Id);
                throw;
            }
        }

        public async Task DeleteTeamAsync(int id)
        {
            _logger.LogInformation("Deleting team {TeamId}", id);

            try
            {
                var team = await _context.Teams.FindAsync(id);
                if (team == null)
                {
                    _logger.LogWarning("Team {TeamId} not found for deletion", id);
                    return;
                }

                _context.Teams.Remove(team);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Successfully deleted team {TeamId}", id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting team {TeamId}", id);
                throw;
            }
        }

        private bool TeamIdExists(int id)
        {
            return _context.Teams.Any(e => e.Id == id);
        }

        private bool TeamNameExists(string name)
        {
            return _context.Teams.Any(e => e.Name == name);
        }
    }
}
