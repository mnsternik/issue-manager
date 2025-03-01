namespace IssueManager.Exceptions
{
    public class FileProcessingException : Exception
    {
        public FileProcessingException(string message) : base(message) { }
        public FileProcessingException(string message, Exception ex) : base(message) { }
    }
}
