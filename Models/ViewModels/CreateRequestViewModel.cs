using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace IssueManager.Models.ViewModels
{
    public class CreateRequestViewModel
    {
        [Required]
        [MaxLength(100)]
        public string Title { get; set; }

        [Required]
        [MaxLength(1000)]
        public string Description { get; set; }

        [Required]
        public RequestPriority Priority { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        public ICollection<Attachment>? Attachments { get; set; }

        [Required]
        [Display(Name = "Category")]
        public int SelectedCategoryId { get; set; }

        [Display(Name = "Target user")]
        public string? SelectedUserId { get; set; }

        [Display(Name = "Target team")]
        public int? SelectedTeamId { get; set; }
    }
}
