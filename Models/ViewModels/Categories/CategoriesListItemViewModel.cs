using System.ComponentModel.DataAnnotations;

namespace IssueManager.Models.ViewModels.Categories
{
    public class CategoriesListItemViewModel
    {
        public int Id { get; set; }
        [Required]
        [MaxLength(50)]
        public string Name { get; set; }
    }
}
