using Microsoft.AspNetCore.Mvc.Rendering;

namespace IssueManager.Models.ViewModels.Common
{
    public class RequestsSelectListsViewModel
    {
        public class UsersByTeamItem
        {
            public string Id { get; set; } = string.Empty;
            public string Name { get; set; } = string.Empty;
        }

        public Dictionary<string, List<UsersByTeamItem>> UsersByTeam { get; set; } = [];
        public IEnumerable<SelectListItem> TeamSelectOptions { get; set; } = [];
        public IEnumerable<SelectListItem> UserSelectOptions { get; set; } = [];
        public IEnumerable<SelectListItem> CategorySelectOptions { get; set; } = [];
    }
}
