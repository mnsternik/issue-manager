using System.ComponentModel.DataAnnotations;

namespace IssueManager.Models
{
    public class RequestResponse
    {
        public int Id { get; set; }

        [Required]
        public string AuthorId { get; set; } = string.Empty;

        [Required]
        public int RequestId { get; set; }

        [Required]
        public DateTime CreateDate { get; set; } 

        [Required]
        public string ResponseText { get; set; } = string.Empty;

        [Required]
        public User Author { get; set; }

        [Required]
        public Request Request { get; set; }
    }
}
