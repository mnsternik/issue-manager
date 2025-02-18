using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace IssueManager.Models.ViewModels.UserManagement
{
    public class CreateUserViewModel
    {
        [Required]
        [MaxLength(50)]
        [DisplayName("Full name")]
        public string Name { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [RegularExpression(@"^(?=.*[^a-zA-Z0-9]).*$", ErrorMessage = "Password must contain at least one non-alphanumeric character.")]
        public string Password { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Passwords do not match.")]
        [DisplayName("Confirm password")]
        public string ConfirmPassword { get; set; }

        [ValidateNever]
        public IList<string> AvailableRoles { get; set; }

        [Required(ErrorMessage = "At least one role must be selected.")]
        [MinLength(1, ErrorMessage = "At least one role must be selected.")]
        public IList<string> SelectedRoles { get; set; } = [];

        [Required]
        [DisplayName("Assigned team")]
        public int TeamId { get; set; }

        [ValidateNever]
        public IEnumerable<SelectListItem> Teams { get; set; }
    }
}
