using System.ComponentModel;

namespace IssueManager.Models.ViewModels.UserManagement
{
    public class ManageUserViewModel
    {
        public string Id { get; set; }
        public string? Name { get; set; }
        public string? Email { get; set; }

        [DisplayName("Assigned team")]
        public int? TeamId { get; set; }
        public string? TeamName { get; set; }
        public IList<string> CurrentRoles { get; set; }
        public IList<string> AvailableRoles { get; set; }
        public IList<string>? SelectedRoles { get; set; }

    }
}
