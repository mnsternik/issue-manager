using IssueManager.Utilities;

namespace IssueManager.Models.ViewModels.Teams
{
    public class TeamsListViewModel
    {
        public PaginatedList<TeamsListItemViewModel> Teams { get; set; }
        public string? SearchString { get; set; }
    }
}
