using System.ComponentModel.DataAnnotations;

namespace IssueManager.Models.ViewModels
{
    public class RequestsListItemViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public RequestPriority Priority { get; set; }
        public RequestStatus Status { get; set; }

        [Display(Name = "Create date")]
        public DateTime CreateDate { get; set; } = DateTime.UtcNow;

        [Display(Name = "Update date")]
        public DateTime? UpdateDate { get; set; }

        [Display(Name = "Category")]
        public string CategoryName { get; set; }

        [Display(Name = "Assigned user")]
        public string? AssignedUserName { get; set; }

        [Display(Name = "Assigned team")]
        public string? AssignedTeamName { get; set; }

        //[Display(Name = "Author name")]
        //public string AuthorName { get; set; }
    }
}
