using System.ComponentModel.DataAnnotations;

namespace IssueManager.Models
{
    public class Request
    {
        [Key]
        public int Id { get; set; } 

        [Required]
        [MaxLength(100)]
        public string Title { get; set; } 

        [Required]
        [MaxLength(1000)]
        public string Description { get; set; }

        [Required]
        public RequestStatus Status { get; set; }

        [Required]
        public RequestPriority Priority { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedDate { get; set; }

        public ICollection<Attachment> Attachments { get; set; }

        [Required]
        public int CategoryId { get; set; } 
        public Category Category { get; set; }

        public string AssignedToUserId { get; set; } 
        public User AssignedToUser { get; set; }

    }

    public enum RequestStatus
    {
        New,         
        InProgress,  
        Completed,   
        Closed       
    }

    public enum RequestPriority
    {
        Low,         
        Medium,      
        High,      
        Critical    
    }
}
