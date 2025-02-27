using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace IssueManager.Models
{
    public class User : IdentityUser
    {
        [Required]
        public string Name { get; set; } = string.Empty;

        [Required]
        public int TeamId { get; set; }

        [Required]
        public Team Team { get; set; }
    }
}
