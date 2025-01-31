using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace IssueManager.Models.ViewModels.UserManagement
{
    public class CreateUserViewModel
    {
        [Required]
        [MaxLength(50)]
        public string UserName { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Passwords do not match.")]
        public string ConfirmPassword { get; set; }

        [ValidateNever]
        public IList<string> AvailableRoles { get; set; }

        [Required]
        public IList<string> SelectedRoles { get; set; }

        [Required]
        public int TeamId { get; set; }

        [ValidateNever]
        public IEnumerable<SelectListItem> Teams { get; set; }
    }
}
