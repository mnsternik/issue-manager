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

        public CategoryService(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper; 
        }

        public async Task<CategoriesListViewModel> GetCategoriesAsync(string search, int pageIndex)
        {
            IQueryable<Category> query = _context.Categories;

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(t => t.Name.ToLower().Contains(search));
            }

            IQueryable<CategoriesListItemViewModel> mappedQuery = _mapper.ProjectTo<CategoriesListItemViewModel>(query);

            var categoriesViewModel = new CategoriesListViewModel
            {
                Categories = await PaginatedList<CategoriesListItemViewModel>.CreateAsync(mappedQuery, pageIndex),
                SearchString = search
            };

            return categoriesViewModel;
        }

        public async Task CreateCategoryAsync(CreateCategoryViewModel categoryViewModel)
        {
            if (CategoryNameExists(categoryViewModel.Name))
            {
                throw new NameAlreadyExistsException("Category with this name already exists");
            }

            var category = _mapper.Map<Category>(categoryViewModel);

            _context.Add(category);
            await _context.SaveChangesAsync();
        }

        public async Task<Category?> GetCategoryAsync(int id)
        {
            var categoryViewModel = await _context.Categories.FindAsync(id);

            if (categoryViewModel == null)
            {
                return null;
            }

            return categoryViewModel;
        }

        public async Task EditCategoryAsync(Category category)
        {
            if (CategoryNameExists(category.Name))
            {
                throw new NameAlreadyExistsException("Category with this name already exists");
            }

            try
            {
                _context.Update(category);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CategoryIdExists(category.Id))
                {
                    // TODO 
                }
                else
                {
                    throw; // TODO
                }
            }
        }

        public async Task DeleteCategoryAsync(int id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category != null)
            {
                _context.Categories.Remove(category);
            }

            await _context.SaveChangesAsync();
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
