using IssueManager.Models;

namespace IssueManager.Test.TestData
{
    public class CategoryTestData
    {
        public static List<Category> GetSampleCategories()
        {
            return new List<Category>
            {
                new Category { Id = 1, Name = "Bug" },
                new Category {Id = 2, Name = "Feature "}
            };
        }
    }
}
