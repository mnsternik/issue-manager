using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace IssueManager.Models.ViewModels.Users
{
    public class CreateUserViewModel
    {
        [Required]
        [MaxLength(50)]
        [DisplayName("Full name")]
        public string Name { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Password)]
        [RegularExpression(@"^(?=.*[^a-zA-Z0-9]).*$", ErrorMessage = "Password must contain at least one non-alphanumeric character.")]
        public string Password { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Passwords do not match.")]
        [DisplayName("Confirm password")]
        public string ConfirmPassword { get; set; } = string.Empty;

        [Required]
        [DisplayName("Assigned team")]
        public int TeamId { get; set; }

        public IEnumerable<SelectListItem> TeamSelectOptions { get; set; } = [];
        public NewUserRolesListViewModel RolesList { get; set; } = new NewUserRolesListViewModel();

    }
}
