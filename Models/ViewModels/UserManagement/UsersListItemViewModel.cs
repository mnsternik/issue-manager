namespace IssueManager.Models.ViewModels.UserManagement
{
    public class UsersListItemViewModel
    {
        public string Id { get; set; }
        public string? Name { get; set; }
        public string? Email { get; set; }
        public int? TeamId { get; set; } // is this needed?
        public string? TeamName { get; set; }
    }
}
