using System.ComponentModel.DataAnnotations;

namespace IssueManager.Models
{
    public class RequestResponse
    {
        public int Id { get; set; }

        [Required]
        public string AuthorId { get; set; }

        [Required]
        public int RequestId { get; set; }

        [Required]
        public DateTime CreateDate { get; set; } = DateTime.UtcNow;

        [Required]
        public string ResponseText { get; set; }

        [Required]
        public User Author { get; set; }

        [Required]
        public Request Request { get; set; }
    }
}
