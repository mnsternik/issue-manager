using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

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
        public IEnumerable<SelectListItem> Teams { get; set; } = [];

        [DataType(DataType.Password)]
        [RegularExpression(@"^(?=.*[^a-zA-Z0-9]).*$", ErrorMessage = "Password must contain at least one non-alphanumeric character.")]
        public string? Password { get; set; }

        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Passwords do not match.")]
        [DisplayName("Confirm password")]
        public string? ConfirmPassword { get; set; }

        public IList<string> CurrentRoles { get; set; } = [];
        public IList<string> AvailableRoles { get; set; } = [];
        public IList<string> SelectedRoles { get; set; } = [];

    }
}
