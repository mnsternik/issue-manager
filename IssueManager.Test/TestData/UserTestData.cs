using IssueManager.Models;

namespace IssueManager.Test.TestData
{
    public class UserTestData
    {
        public static List<User> GetUsersSample() 
        { 
            return new List<User>
            {
                new User
                {
                    Id = "uid1",
                    Name = "John Doe",
                    Email = "johndoe@test.com",
                    TeamId = 1
                },
                new User
                {
                    Id = "uid2",
                    Name = "Vincent Cole",
                    Email = "vincentcole@test.com",
                    TeamId = 2
                }
            }; 
        } 
    }
}
