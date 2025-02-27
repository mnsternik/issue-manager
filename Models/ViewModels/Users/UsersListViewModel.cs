using IssueManager.Utilities;

namespace IssueManager.Models.ViewModels.Users
{
    public class UsersListViewModel
    {
        public PaginatedList<UsersListItemViewModel> Users { get; set; }
        public string? SearchString { get; set; }
    }
}
