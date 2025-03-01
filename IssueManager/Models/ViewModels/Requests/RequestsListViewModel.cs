using IssueManager.Utilities;
using System.ComponentModel.DataAnnotations;

namespace IssueManager.Models.ViewModels.Requests
{
    public class RequestsListViewModel
    {
        public PaginatedList<RequestsListItemViewModel> Requests { get; set; }

        public RequestSearchFilters? Filters { get; set; }

        public RequestsSelectListsViewModel SelectLists { get; set; } = new RequestsSelectListsViewModel();
    }

    public class RequestSearchFilters
    {
        [Display(Name = "Request ID")]
        public int? RequestId { get; set; }

        [Display(Name = "Title includes")]
        public string? Title { get; set; }

        [Display(Name = "Description includes")]
        public string? Description { get; set; }
        public RequestStatus? Status { get; set; }
        public RequestPriority? Priority { get; set; }

        [Display(Name = "Category")]
        public int? CategoryId { get; set; }

        [Display(Name = "Assigned team")]
        public int? AssignedTeamId { get; set; }

        [Display(Name = "Assigned user")]
        public string? AssignedUserId { get; set; }

        [Display(Name = "Author")]
        public string? AuthorId { get; set; }

        [Display(Name = "Created before")]
        public DateTime? CreatedBefore { get; set; }

        [Display(Name = "Created after")]
        public DateTime? CreatedAfter { get; set; }

        [Display(Name = "Updated before")]
        public DateTime? UpdatedBefore { get; set; }

        [Display(Name = "Updated after")]
        public DateTime? UpdatedAfter { get; set; }
    }
}
