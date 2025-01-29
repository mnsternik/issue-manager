using System.ComponentModel.DataAnnotations;

namespace IssueManager.Models
{
    public class RequestResponse
    {
        public int Id { get; set; }
        public string AuthorId { get; set; }
        public int RequestId { get; set; }
        public DateTime CreateDate { get; set; }
        [Required]
        public string ResponseText { get; set; }

        public User Author { get; set; }
        public Request Request { get; set; }
    }
}
