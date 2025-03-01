namespace IssueManager.Exceptions
{
    public class UserOperationException : Exception
    {
        public List<string> Errors { get; }

        public UserOperationException(List<string> errors)
            : base($"User operation failed: {string.Join("; ", errors)}")
        {
            Errors = errors;
        }
    }
}
