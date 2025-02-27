using IssueManager.Models.ViewModels.Requests;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace IssueManager.Services.DataLists
{
    public interface ISelectListService
    {
        public RequestsSelectListsViewModel PopulateRequestSelectLists(string? selectedUserId = null, int? selectedTeamId = null, int? selectedCategoryId = null);
        public IEnumerable<SelectListItem> PopulateTeamSelectList(int? selectedTeamId = null);
    }
}
