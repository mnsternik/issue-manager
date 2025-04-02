using System.ComponentModel.DataAnnotations;

namespace IssueManager.Models
{
    public class Team
    {
        public int Id { get; set; }
        [Required]
        [MaxLength(50)]
        public string Name { get; set; } = string.Empty;
        public ICollection<User> Users { get; set; } = new List<User>();
    }
}
