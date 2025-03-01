using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace IssueManager.Models.ViewModels.Users
{
    public class ChangeUserDetailsViewModel
    {
        public string Id { get; set; } = string.Empty;

        [Required]
        public string Name { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [DisplayName("Assigned team")]
        public int TeamId { get; set; }
        public string TeamName { get; set; } = string.Empty;

        public IEnumerable<SelectListItem> TeamSelectOptions { get; set; } = [];
    }
}
