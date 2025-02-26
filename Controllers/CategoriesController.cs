using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using IssueManager.Models;
using IssueManager.Models.ViewModels.Categories;
using IssueManager.Services.Categories;
using IssueManager.Exceptions;
using Microsoft.AspNetCore.Authorization;

namespace IssueManager.Controllers
{
    public class CategoriesController : Controller
    {
        private readonly ICategoryService _categoryService; 

        public CategoriesController(ICategoryService categoryService)
        {
            _categoryService = categoryService; 
        }

        // GET: Categories
        public async Task<IActionResult> Index(string search, int pageIndex = 1)
        {
            var categoriesViewModel = await _categoryService.GetCategoriesAsync(search, pageIndex); 
            return View(categoriesViewModel);
        }

        // GET: Categories/Create
        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            return View();
        }

        // POST: Categories/Create
        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateCategoryViewModel categoryViewModel)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    await _categoryService.CreateCategoryAsync(categoryViewModel);
                }
                catch (NameAlreadyExistsException)
                {
                    ModelState.AddModelError("Name", "Category with this name already exists");
                    return View(categoryViewModel);
                }
       
                return RedirectToAction(nameof(Index));
            }

            return View(categoryViewModel);
        }

        // GET: Categories/Edit/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var category = await _categoryService.GetCategoryAsync((int)id);

            if (category == null)
            {
                return NotFound();
            }

            return View(category);
        }

        // POST: Categories/Edit/5
        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Category category)
        {
            if (id != category.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    await _categoryService.EditCategoryAsync(category);
                }
                catch (NameAlreadyExistsException)
                {
                    ModelState.AddModelError("Name", "Category with this name already exists");
                    return View(category);
                }
                catch (DbUpdateConcurrencyException)
                {
                    // TODO 
                }

                return RedirectToAction(nameof(Index));
            }

            return View(category);
        }

        // GET: Categories/Delete/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var category = await _categoryService.GetCategoryAsync((int)id);

            if (category == null)
            {
                return NotFound();
            }

            return View(category);
        }

        // POST: Categories/Delete/5
        [HttpPost, ActionName("Delete")]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                await _categoryService.DeleteCategoryAsync(id);
            }
            catch
            {
                // TODO
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
