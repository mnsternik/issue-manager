using IssueManager.Models.ViewModels.Common;
using IssueManager.Services.SelectLists;
using System.ComponentModel.DataAnnotations;

namespace IssueManager.Models.ViewModels.Requests
{
    public class CreateRequestViewModel
    {
        [Required]
        [MaxLength(100)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [MaxLength(1000)]
        public string Description { get; set; } = string.Empty;

        [Required]
        public RequestPriority Priority { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        [Required]
        [Display(Name = "Category")]
        public int CategoryId { get; set; }

        [Display(Name = "Target user")]
        public string? AssignedUserId { get; set; }

        [Display(Name = "Target team")]
        public int? AssignedTeamId { get; set; }

        public List<IFormFile> Files { get; set; } = [];

        public RequestsSelectListsViewModel SelectLists { get; set; } = new RequestsSelectListsViewModel(); 
    }
}
