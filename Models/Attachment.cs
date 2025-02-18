using System.ComponentModel.DataAnnotations;

namespace IssueManager.Models
{
    public class Attachment
    {
        public int Id { get; set; } 

        [Required]
        public string FileName { get; set; }

        [Required]
        public string ContentType { get; set; }

        [Required]
        public byte[] FileData { get; set; }

        [Required]
        public int RequestId { get; set; }

        [Required]
        public Request Request { get; set; } 
    }
}
