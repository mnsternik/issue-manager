using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace IssueManager.Models
{
    public class User : IdentityUser
    {
        [Required]
        [MaxLength(100)]
        public string FullName { get; set; }

        [Required]
        public int TeamId { get; set; } 

        public Team Team { get; set; }
    }
}
