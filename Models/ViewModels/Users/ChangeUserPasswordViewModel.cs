using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace IssueManager.Models.ViewModels.Users
{
    public class ChangeUserPasswordViewModel
    {
        public string Id { get; set; } = string.Empty;

        [ValidateNever]
        public string Name { get; set; } = string.Empty;

        [ValidateNever]
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
    }
}
