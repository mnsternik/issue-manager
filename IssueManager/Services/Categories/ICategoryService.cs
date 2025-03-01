using IssueManager.Models;
using IssueManager.Models.ViewModels.Categories;

namespace IssueManager.Services.Categories
{
    public interface ICategoryService
    {
        public Task<CategoriesListViewModel> GetCategoriesAsync(string search, int pageIndex);
        public Task CreateCategoryAsync(CreateCategoryViewModel categoryViewModel);
        public Task<Category?> GetCategoryAsync(int id);
        public Task EditCategoryAsync(Category category);
        public Task DeleteCategoryAsync(int id);
    }
}
