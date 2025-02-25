using Microsoft.AspNetCore.Mvc.Rendering;

namespace IssueManager.Models.ViewModels.Common
{
    public class UserSelectListsViewModel
    {
        public IEnumerable<SelectListItem> TeamSelectOptions { get; set; } = [];
    }
}
