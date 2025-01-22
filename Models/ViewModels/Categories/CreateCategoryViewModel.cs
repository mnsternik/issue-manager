using System.ComponentModel.DataAnnotations;

namespace IssueManager.Models.ViewModels.Categories
{
    public class CreateCategoryViewModel
    {
        [Required]
        [MaxLength(50)]
        public string Name { get; set; }
    }
}
