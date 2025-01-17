using IssueManager.Helpers;

namespace IssueManager.Models.ViewModels.Requests
{
    public class RequestsListViewModel
    {
        public PaginatedList<RequestsListItemViewModel> Requests { get; set; }
        public string? SearchString { get; set; }
    }
}
