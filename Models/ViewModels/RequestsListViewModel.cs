using IssueManager.Helpers;

namespace IssueManager.Models.ViewModels
{
    public class RequestsListViewModel
    {
        public PaginatedList<RequestsListItemViewModel> Requests { get; set; }
        public string? SearchString { get; set; }
    }
}
