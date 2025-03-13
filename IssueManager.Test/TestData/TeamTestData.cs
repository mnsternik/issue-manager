using IssueManager.Models;

namespace IssueManager.Test.TestData
{
    public class TeamTestData
    {
        public static List<Team> GetSampleTeams()
        {
            return new List<Team>
            {
                new Team { Id = 1, Name = "Administrators" },
                new Team { Id = 2, Name = "Human Resources" }
            };
        }
    }
}
