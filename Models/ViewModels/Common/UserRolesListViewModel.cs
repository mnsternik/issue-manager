using System.ComponentModel.DataAnnotations;

namespace IssueManager.Models.ViewModels.Common
{
    public class ExistingUserRolesListViewModel
    {
        public IList<string> CurrentRoles { get; set; } = [];
        public IList<string> AvailableRoles { get; set; } = [];
        public IList<string> SelectedRoles { get; set; } = [];
    }

    public class NewUserRolesListViewModel
    {
        public IList<string> AvailableRoles { get; set; } = [];

        [Required(ErrorMessage = "At least one role must be selected.")]
        [MinLength(1, ErrorMessage = "At least one role must be selected.")]
        public IList<string> SelectedRoles { get; set; } = [];
    }
}
