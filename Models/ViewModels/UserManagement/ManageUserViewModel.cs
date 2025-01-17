namespace IssueManager.Models.ViewModels.UserManagement
{
    public class ManageUserViewModel
    {
        public string Id { get; set; }
        public string? Name { get; set; }
        public string? Email { get; set; }
        public int? TeamId { get; set; }
        public string? TeamName { get; set; }
        public IEnumerable<string> CurrentRoles { get; set; }
        public IEnumerable<string>? AvailableRoles { get; set; }
    }
}
