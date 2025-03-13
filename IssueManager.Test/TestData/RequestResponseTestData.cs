using IssueManager.Models;

namespace IssueManager.Test.TestData
{
    public class RequestResponseTestData
    {
        public static List<RequestResponse> GetRequestResponsesSample()
        {
            return new List<RequestResponse>
            {
                new RequestResponse
                {
                    Id = 1,
                    AuthorId = "uid1",
                    RequestId = 2,
                    CreateDate = new DateTime(2025, 03, 02, 12, 25, 30),
                    ResponseText = "Please specify the name of application",
                },
                new RequestResponse
                {
                    Id = 2,
                    AuthorId = "uid2",
                    RequestId = 2,
                    CreateDate = new DateTime(2025, 03, 02, 12, 30, 30 ),
                    ResponseText = "Application name: IssueManager",
                }
            };
        }
    }
}
