using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using IssueManager.Data;
using IssueManager.Models;
using IssueManager.Utilities;
using AutoMapper;
using IssueManager.Models.ViewModels.Categories;

namespace IssueManager.Controllers
{
    public class CategoriesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;

        public CategoriesController(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        // GET: Categories
        public async Task<IActionResult> Index(string search, int pageIndex = 1)
        {
            const int pageSize = 10;

            IQueryable<Category> query = _context.Categories;

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(t => t.Name.ToLower().Contains(search));
            }

            IQueryable<CategoriesListItemViewModel> mappedQuery = _mapper.ProjectTo<CategoriesListItemViewModel>(query);

            var paginatedCategories = await PaginatedList<CategoriesListItemViewModel>.CreateAsync(mappedQuery, pageIndex, pageSize);

            var categoriesViewModel = new CategoriesListViewModel
            {
                Categories = paginatedCategories,
                SearchString = search
            };

            return View(categoriesViewModel);
        }

        // GET: Categories/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Teams/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name")] CreateCategoryViewModel categoryViewModel)
        {
            if (ModelState.IsValid)
            {
                if (CategoryNameExists(categoryViewModel.Name))
                {
                    ModelState.AddModelError("Name", "Category with this name already exists");
                    return View(categoryViewModel); 
                }

                var category = _mapper.Map<Category>(categoryViewModel); 

                _context.Add(category);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(categoryViewModel);
        }

        // GET: Categories/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var category = await _context.Categories.FindAsync(id);
            if (category == null)
            {
                return NotFound();
            }
            return View(category);
        }

        // POST: Categories/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name")] Category category)
        {
            if (id != category.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                if (CategoryNameExists(category.Name))
                {
                    ModelState.AddModelError("Name", "Category with this name already exists");
                    return View(category);
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
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(category);
        }

        // GET: Categories/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var category = await _context.Categories
                .FirstOrDefaultAsync(m => m.Id == id);
            if (category == null)
            {
                return NotFound();
            }

            return View(category);
        }

        // POST: Categories/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category != null)
            {
                _context.Categories.Remove(category);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
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
