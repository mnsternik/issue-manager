using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace IssueManager.Models.ViewModels.Teams
{
    public class TeamsListItemViewModel
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }
    }
}
