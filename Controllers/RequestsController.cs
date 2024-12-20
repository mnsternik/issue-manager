using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using IssueManager.Data;
using IssueManager.Models;
using IssueManager.Models.ViewModels;

namespace IssueManager.Controllers
{
    public class RequestsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public RequestsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Requests
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.Requests.Include(r => r.AssignedToTeam).Include(r => r.AssignedToUser).Include(r => r.Category);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: Requests/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var request = await _context.Requests
                .Include(r => r.AssignedToTeam)
                .Include(r => r.AssignedToUser)
                .Include(r => r.Category)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (request == null)
            {
                return NotFound();
            }

            return View(request);
        }

        // GET: Requests/Create
        public IActionResult Create()
        {
            ViewData["CategorySelectOptions"] = _context.Categories.Select(c => new SelectListItem
            {
                Value = c.Id.ToString(),
                Text = c.Name
            });

            ViewData["UserSelectOptions"] = _context.Users.Select(u => new SelectListItem
            {
                Value = u.Id,
                Text = u.FullName
            });

            ViewData["TeamSelectOptions"] = _context.Teams.Select(t => new SelectListItem
            {
                Value = t.Id.ToString(),
                Text = t.Name
             });

            return View(new CreateRequestViewModel());
        }

        // POST: Requests/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateRequestViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                var request = new Request
                {
                    Title = viewModel.Title,
                    Description = viewModel.Description,
                    Status = RequestStatus.New,
                    Priority = viewModel.Priority,
                    CategoryId = viewModel.SelectedCategoryId,
                    AssignedToTeamId = viewModel.SelectedTeamId,
                    AssignedToUserId = viewModel.SelectedUserId,
                };

                _context.Add(request);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            return View(viewModel);
        }

        // GET: Requests/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var request = await _context.Requests.FindAsync(id);
            if (request == null)
            {
                return NotFound();
            }
            ViewData["AssignedToTeamId"] = new SelectList(_context.Teams, "Id", "Name", request.AssignedToTeamId);
            ViewData["AssignedToUserId"] = new SelectList(_context.Users, "Id", "Id", request.AssignedToUserId);
            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name", request.CategoryId);
            return View(request);
        }

        // POST: Requests/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Title,Description,Status,Priority,CreatedDate,UpdatedDate,CategoryId,AssignedToUserId,AssignedToTeamId")] Request request)
        {
            if (id != request.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(request);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!RequestExists(request.Id))
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
            ViewData["AssignedToTeamId"] = new SelectList(_context.Teams, "Id", "Name", request.AssignedToTeamId);
            ViewData["AssignedToUserId"] = new SelectList(_context.Users, "Id", "Id", request.AssignedToUserId);
            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name", request.CategoryId);
            return View(request);
        }

        // GET: Requests/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var request = await _context.Requests
                .Include(r => r.AssignedToTeam)
                .Include(r => r.AssignedToUser)
                .Include(r => r.Category)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (request == null)
            {
                return NotFound();
            }

            return View(request);
        }

        // POST: Requests/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var request = await _context.Requests.FindAsync(id);
            if (request != null)
            {
                _context.Requests.Remove(request);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool RequestExists(int id)
        {
            return _context.Requests.Any(e => e.Id == id);
        }
    }
}
