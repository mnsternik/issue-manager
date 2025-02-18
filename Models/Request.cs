using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IssueManager.Models
{
    public class Request
    {
        public int Id { get; set; } 

        [Required]
        [MaxLength(100)]
        public string Title { get; set; } 

        [Required]
        [MaxLength(1000)]
        public string Description { get; set; }

        [Required]
        public RequestStatus Status { get; set; } = RequestStatus.Open; 

        [Required]
        public RequestPriority Priority { get; set; }

        [Required]
        public DateTime CreateDate { get; set; } = DateTime.UtcNow;

        public DateTime? UpdateDate { get; set; } 

        [Required]
        public int CategoryId { get; set; }

        [Required]
        [ForeignKey(nameof(Author))]
        public string AuthorId { get; set; }

        [ForeignKey(nameof(AssignedUser))]
        public string? AssignedUserId { get; set; } 

        public int? AssignedTeamId { get; set; }
        public Team? AssignedTeam { get; set; }
        public User? AssignedUser { get; set; }

        [Required]
        public Category Category { get; set; }

        [Required]
        public User Author { get; set; }

        public ICollection<RequestResponse>? Responses { get; set; } = [];
        public ICollection<Attachment>? Attachments { get; set; } = [];
    }

    public enum RequestStatus
    {
        Open,         
        InProgress,  
        Resolved,   
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
