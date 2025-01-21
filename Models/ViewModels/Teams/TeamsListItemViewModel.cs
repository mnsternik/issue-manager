using System.ComponentModel.DataAnnotations;

namespace IssueManager.Models.ViewModels.Teams
{
    public class TeamsListItemViewModel
    {
        public int Id { get; set; }
        [Required]
        [MaxLength(50)]
        public string Name { get; set; }
    }
}
