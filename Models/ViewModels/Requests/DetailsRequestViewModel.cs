using System.ComponentModel.DataAnnotations;

namespace IssueManager.Models.ViewModels.Requests
{
    public class DetailsRequestViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public RequestPriority Priority { get; set; }
        public RequestStatus Status { get; set; }

        [Display(Name = "Create date")]
        public DateTime CreateDate { get; set; }

        [Display(Name = "Last update date")]
        public DateTime? UpdateDate { get; set; }

        [Display(Name = "Category")]
        public string CategoryName { get; set; }

        [Display(Name = "Assigned user")]
        public string? AssignedUserName { get; set; } = "Not Assigned";

        [Display(Name = "Assigned team")]
        public string? AssignedTeamName { get; set; } = "Not Assigned";

        [Display(Name = "Author name")]
        public string AuthorName { get; set; }

        public bool AllowAssign { get; set; } = false;
        public bool AllowEdit { get; set; } = false;

        public ICollection<RequestResponseViewModel>? Responses { get; set; }
        public ICollection<Attachment>? Attachments { get; set; }
    }
}
