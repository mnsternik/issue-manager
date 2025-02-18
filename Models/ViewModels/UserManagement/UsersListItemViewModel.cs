using System.ComponentModel;

namespace IssueManager.Models.ViewModels.UserManagement
{
    public class UsersListItemViewModel
    {
        public string Id { get; set; }
        public string? Name { get; set; }
        public string? Email { get; set; }

        [DisplayName("Assigned team")]
        public string? TeamName { get; set; }
    }
}
