namespace IssueManager.Exceptions
{
    public class NameAlreadyExistsException : Exception
    {
        public NameAlreadyExistsException(string message) : base(message) { }
    }
}
