using System.ComponentModel.DataAnnotations;

namespace IssueManager.Models.ViewModels.Categories
{
    public class CategoriesListItemViewModel
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; } = string.Empty;
    }
}
