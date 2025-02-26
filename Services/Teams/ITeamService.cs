using IssueManager.Models;
using IssueManager.Models.ViewModels.Teams;

namespace IssueManager.Services.Teams
{
    public interface ITeamService
    {
        public Task<TeamsListViewModel> GetTeamsAsync(string search, int pageIndex);
        public Task CreateTeamAsync(CreateTeamViewModel teamViewModel);
        public Task<Team?> GetTeamAsync(int id);
        public Task EditTeamAsync(Team team);
        public Task DeleteTeamAsync(int id);
    }
}
