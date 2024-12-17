using System.ComponentModel.DataAnnotations;

namespace IssueManager.Models
{
    public class Notification
    {
        [Key]
        public int Id { get; set; } 

        [Required]
        public string UserId { get; set; } 
        public User User { get; set; } 

        [Required]
        [MaxLength(200)]
        public string Message { get; set; } 

        public bool IsRead { get; set; } = false; 

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    }
}
