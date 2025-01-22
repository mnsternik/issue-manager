using IssueManager.Models.ViewModels.Teams;
using IssueManager.Utilities;

namespace IssueManager.Models.ViewModels.Categories
{
    public class CategoriesListViewModel
    {
        public PaginatedList<CategoriesListItemViewModel> Categories { get; set; }
        public string? SearchString { get; set; }
    }
}
