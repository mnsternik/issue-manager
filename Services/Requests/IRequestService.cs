using IssueManager.Models.ViewModels.Requests;
using System.Security.Claims;

namespace IssueManager.Services.Requests
{
    public interface IRequestService
    {
        public Task<RequestsListViewModel> GetRequestsAsync(RequestSearchFilters filters, int pageIndex);
        public Task<DetailsRequestViewModel?> GetRequestDetailsAsync(int id, ClaimsPrincipal currentUser);
        public CreateRequestViewModel GetCreateRequest();
        public Task<int> CreateRequestAsync(CreateRequestViewModel requestViewModel, string currentUserId);
        public Task AssignRequestAsync(int id, ClaimsPrincipal currentUser);
        public Task<EditRequestViewModel?> GetEditRequestAsync(int id);
        public Task EditRequestAsync(EditRequestViewModel requestViewModel);
        public Task AddResponseAsync(int requestId, string responseText, string currentUserId);
    }
}
