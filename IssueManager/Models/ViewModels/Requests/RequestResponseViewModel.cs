﻿namespace IssueManager.Models.ViewModels.Requests
{
    public class RequestResponseViewModel
    {
        public int Id { get; set; }
        public int RequestId { get; set; }
        public DateTime CreateDate { get; set; }
        public string ResponseText { get; set; } = string.Empty;
        public string AuthorName { get; set; } = string.Empty;
    }
}
