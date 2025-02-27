using System.ComponentModel.DataAnnotations;

namespace IssueManager.Models.ViewModels.Teams
{
    public class CreateTeamViewModel
    {
        [Required]
        [MaxLength(50)]
        public string Name { get; set; } = string.Empty;
    }
}
