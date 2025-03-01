using System.ComponentModel;

namespace IssueManager.Models.ViewModels.Users
{
    public class UsersListItemViewModel
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;

        [DisplayName("Assigned team")]
        public string? TeamName { get; set; }
    }
}
