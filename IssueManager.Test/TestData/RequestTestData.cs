using IssueManager.Models;

namespace IssueManager.Test.TestData
{
    public class RequestTestData
    {
        public static List<Request> GetSampleRequests() 
        {
            return new List<Request>
            {
                new Request {
                    Id = 1,
                    Title = "Creating new account for user",
                    Description = "Create new account for user John Doe in application IssueManager",
                    Status = RequestStatus.Open,
                    Priority = RequestPriority.Medium,
                    CreateDate = new DateTime(2025, 03, 01),
                    UpdateDate = null,
                    CategoryId = 1,   
                    AuthorId = "uid2", 
                    AssignedUserId = null,
                    AssignedTeamId = 1,
                 },
                new Request {
                    Id = 2,
                    Title = "Password reset request",
                    Description = "The user is unable to log in due to a forgotten password and requires assistance in resetting it.",
                    Status = RequestStatus.InProgress,
                    Priority = RequestPriority.Low,
                    CreateDate = new DateTime(2025, 03, 01),
                    UpdateDate = new DateTime(2025, 03, 02),
                    CategoryId = 2, 
                    AuthorId = "uid1",
                    AssignedUserId = "uid2", 
                    AssignedTeamId = 2,
                 }
            };
        }
    }
}
