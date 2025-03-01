using System.ComponentModel.DataAnnotations;

namespace IssueManager.Models.ViewModels.Users
{
    public class ExistingUserRolesListViewModel
    {
        public List<string> CurrentRoles { get; set; } = [];
        public List<string> AvailableRoles { get; set; } = [];
        public List<string> SelectedRoles { get; set; } = [];
    }

    public class NewUserRolesListViewModel
    {
        public List<string> AvailableRoles { get; set; } = [];

        [Required(ErrorMessage = "At least one role must be selected.")]
        [MinLength(1, ErrorMessage = "At least one role must be selected.")]
        public List<string> SelectedRoles { get; set; } = [];
    }
}
