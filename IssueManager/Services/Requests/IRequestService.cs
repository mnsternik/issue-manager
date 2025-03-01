using IssueManager.Models;
using IssueManager.Models.ViewModels.Requests;
using System.Security.Claims;

namespace IssueManager.Services.Requests
{
    public interface IRequestService
    {
        public Task<RequestsListViewModel> GetRequestsAsync(RequestSearchFilters filters, int pageIndex);
        public Task<DetailsRequestViewModel?> GetRequestDetailsAsync(int id, ClaimsPrincipal currentUser);
        public Task<int> CreateRequestAsync(CreateRequestViewModel requestViewModel, ClaimsPrincipal currentUser);
        public Task<Attachment?> GetAttachmentAsync(int id);
        public Task AssignRequestAsync(int id, ClaimsPrincipal currentUser);
        public Task<EditRequestViewModel?> GetEditRequestAsync(int id);
        public Task EditRequestAsync(EditRequestViewModel requestViewModel);
        public Task AddResponseAsync(int requestId, string responseText, ClaimsPrincipal currentUser);
    }
}
