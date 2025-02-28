using AutoMapper;
using IssueManager.Data;
using IssueManager.Exceptions;
using IssueManager.Models;
using IssueManager.Models.ViewModels.Categories;
using IssueManager.Utilities;
using Microsoft.EntityFrameworkCore;

namespace IssueManager.Services.Categories
{
    public class CategoryService : ICategoryService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly ILogger<CategoryService> _logger;

        public CategoryService(ApplicationDbContext context, IMapper mapper, ILogger<CategoryService> logger)
        {
            _context = context;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<CategoriesListViewModel> GetCategoriesAsync(string search, int pageIndex)
        {
            _logger.LogInformation("Retrieving categories with search: {Search}, page index: {PageIndex}", search, pageIndex);

            try
            {
                IQueryable<Category> query = _context.Categories;

                if (!string.IsNullOrEmpty(search))
                {
                    _logger.LogDebug("Applying search filter: {Search}", search);
                    query = query.Where(t => t.Name.ToLower().Contains(search.ToLower()));
                }

                IQueryable<CategoriesListItemViewModel> mappedQuery = _mapper.ProjectTo<CategoriesListItemViewModel>(query);

                var categoriesViewModel = new CategoriesListViewModel
                {
                    Categories = await PaginatedList<CategoriesListItemViewModel>.CreateAsync(mappedQuery, pageIndex),
                    SearchString = search
                };

                _logger.LogInformation("Returning {CategoryCount} categories", categoriesViewModel.Categories.Count);
                return categoriesViewModel;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving categories with search: {Search}", search);
                throw;
            }
        }

        public async Task CreateCategoryAsync(CreateCategoryViewModel categoryViewModel)
        {
            _logger.LogInformation("Creating new category: {CategoryName}", categoryViewModel.Name);

            try
            {
                if (CategoryNameExists(categoryViewModel.Name))
                {
                    _logger.LogWarning("Category creation failed - name already exists: {CategoryName}", categoryViewModel.Name);
                    throw new NameAlreadyExistsException("Category with this name already exists");
                }

                var category = _mapper.Map<Category>(categoryViewModel);

                _context.Add(category);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Successfully created category {CategoryId}: {CategoryName}",
                    category.Id, category.Name);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating category {CategoryName}", categoryViewModel.Name);
                throw;
            }
        }

        public async Task<Category?> GetCategoryAsync(int id)
        {
            _logger.LogInformation("Retrieving category {CategoryId}", id);

            try
            {
                var category = await _context.Categories.FindAsync(id);

                if (category == null)
                {
                    _logger.LogWarning("Category {CategoryId} not found", id);
                }

                return category;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving category {CategoryId}", id);
                throw;
            }
        }

        public async Task EditCategoryAsync(Category category)
        {
            _logger.LogInformation("Updating category {CategoryId}: {CategoryName}",
                category.Id, category.Name);

            try
            {
                if (CategoryNameExists(category.Name))
                {
                    _logger.LogWarning("Category update failed - name already exists: {CategoryName}", category.Name);
                    throw new NameAlreadyExistsException("Category with this name already exists");
                }

                _context.Update(category);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Successfully updated category {CategoryId}", category.Id);
            }
            catch (DbUpdateConcurrencyException ex)
            {
                if (!CategoryIdExists(category.Id))
                {
                    _logger.LogWarning("Concurrency conflict - category {CategoryId} was deleted by another user",
                        category.Id);
                }
                else
                {
                    _logger.LogError(ex, "Concurrency conflict while updating category {CategoryId}", category.Id);
                }
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating category {CategoryId}", category.Id);
                throw;
            }
        }

        public async Task DeleteCategoryAsync(int id)
        {
            _logger.LogInformation("Deleting category {CategoryId}", id);

            try
            {
                var category = await _context.Categories.FindAsync(id);
                if (category == null)
                {
                    _logger.LogWarning("Category {CategoryId} not found for deletion", id);
                    return;
                }

                _context.Categories.Remove(category);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Successfully deleted category {CategoryId}", id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting category {CategoryId}", id);
                throw;
            }
        }

        private bool CategoryIdExists(int id)
        {
            return _context.Categories.Any(e => e.Id == id);
        }

        private bool CategoryNameExists(string name)
        {
            return _context.Categories.Any(c => c.Name == name);
        }
    }
}
