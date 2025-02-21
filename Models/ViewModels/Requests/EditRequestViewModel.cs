using IssueManager.Models.ViewModels.Common;
using System.ComponentModel.DataAnnotations;

namespace IssueManager.Models.ViewModels.Requests
{
    public class EditRequestViewModel
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Title { get; set; }

        [Required]
        [MaxLength(1000)]
        public string Description { get; set; }

        [Required]
        public RequestPriority Priority { get; set; }

        [Required]
        public RequestStatus Status { get; set; }

        public DateTime CreateDate { get; set; }

        [Display(Name = "Last update date")]
        public DateTime? UpdateDate { get; set; }

        public string AuthorName { get; set; }

        public string AuthorId { get; set; }

        [Required]
        [Display(Name = "Category")]
        public int CategoryId { get; set; }

        [Display(Name = "Assigned user")]
        public string? AssignedUserId { get; set; }

        [Display(Name = "Assigned team")]
        public int? AssignedTeamId { get; set; }

        public ICollection<Attachment>? Attachments { get; set; }

        public ICollection<RequestResponseViewModel>? Responses { get; set; }

        public RequestsSelectListsViewModel SelectLists { get; set; } = new RequestsSelectListsViewModel();
    }
}
