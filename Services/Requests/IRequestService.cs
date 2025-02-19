using IssueManager.Models.ViewModels.Requests;
using System.Security.Claims;

namespace IssueManager.Services.Requests
{
    public interface IRequestService
    {
        public Task<RequestsListViewModel> GetRequestsAsync(RequestSearchFilters filters, int pageIndex);
        public Task<DetailsRequestViewModel?> GetRequestDetailsAsync(int id, ClaimsPrincipal currentUser);
        public Task CreateRequestAsync(CreateRequestViewModel requestViewModel, string currentUserId);
    }
}
