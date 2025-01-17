using IssueManager.Helpers;

namespace IssueManager.Models.ViewModels.UserManagement
{
    public class UsersListViewModel
    {
        public PaginatedList<UsersListItemViewModel> Users { get; set; }
        public string? SearchString { get; set; }
    }
}
