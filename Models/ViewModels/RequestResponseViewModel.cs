namespace IssueManager.Models.ViewModels
{
    public class RequestResponseViewModel
    {
        public int Id { get; set; }
        public int RequestId { get; set; }
        public DateTime CreateDate { get; set; }
        public string ResponseText { get; set; }
        public string AuthorName { get; set; }
    }
}
