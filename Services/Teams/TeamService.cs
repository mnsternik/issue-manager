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

        public TeamService(ApplicationDbContext context, IMapper mapper) 
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<TeamsListViewModel> GetTeamsAsync(string search, int pageIndex)
        {
            IQueryable<Team> query = _context.Teams.AsNoTracking();

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(t => t.Name.ToLower().Contains(search));
            }

            IQueryable<TeamsListItemViewModel> mappedQuery = _mapper.ProjectTo<TeamsListItemViewModel>(query);

            var teamsViewModel = new TeamsListViewModel
            {
                Teams = await PaginatedList<TeamsListItemViewModel>.CreateAsync(mappedQuery, pageIndex),
                SearchString = search
            };

            return teamsViewModel;
        }
        
        public async Task CreateTeamAsync(CreateTeamViewModel teamViewModel)
        {
            if (TeamNameExists(teamViewModel.Name))
            {
                throw new NameAlreadyExistsException("Team with this name already exists");
            }

            var team = _mapper.Map<Team>(teamViewModel);

            _context.Add(team);
            await _context.SaveChangesAsync();
        }

        public async Task<Team?> GetTeamAsync(int id)
        {
            var teamViewModel = await _context.Teams.FindAsync(id);

            if (teamViewModel == null) 
            {
                return null;
            }

            return teamViewModel;
        }

        public async Task EditTeamAsync(Team team)
        {
            if (TeamNameExists(team.Name))
            {
                throw new NameAlreadyExistsException("Team with this name already exists");
            }

            try
            {
                _context.Update(team);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!TeamIdExists(team.Id))
                {
                   // TODO 
                }
                else
                {
                    throw; // TODO
                }
            }
        }

        public async Task DeleteTeamAsync(int id)
        {
            var team = await _context.Teams.FindAsync(id);
            if (team != null)
            {
                _context.Teams.Remove(team);
            }

            await _context.SaveChangesAsync();
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
