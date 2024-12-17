using System.ComponentModel.DataAnnotations;

namespace IssueManager.Models
{
    public class Attachment
    {
        [Key]
        public int Id { get; set; } 

        [Required]
        [MaxLength(200)]
        public string FilePath { get; set; } 

        [Required]
        public int RequestId { get; set; } 
        public Request Request { get; set; }
    }
}
