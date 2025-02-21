using IssueManager.Models.ViewModels.Common;

namespace IssueManager.Services.SelectLists
{
    public interface ISelectListService
    {
        public RequestsSelectListsViewModel PopulateRequestSelectLists(string? selectedUserId = null, int? selectedTeamId = null, int? selectedCategoryId = null);
    }
}
