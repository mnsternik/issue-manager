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
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using IssueManager.Helpers;

namespace IssueManager.Controllers
{
    public class RequestsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;

        public RequestsController(ApplicationDbContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Requests
        public async Task<IActionResult> Index(string search, int pageIndex = 1)
        {
            const int pageSize = 10;

            IQueryable<Request> query = _context.Requests
                .Include(r => r.AssignedToTeam)
                .Include(r => r.AssignedToUser)
                .Include(r => r.Author)
                .Include(r => r.Category);

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(r => r.Title.ToLower().Contains(search));
            }

            var paginatedRequestsList = await PaginatedList<Request>.CreateAsync(query, pageIndex, pageSize);

            var requestsListViewModel = new RequestsListViewModel
            {
                Requests = paginatedRequestsList,
                SearchString = search
            };

            return View(requestsListViewModel);
        }

        // GET: Requests/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var requestViewModel = await _context.Requests
                .Where(r => r.Id == id)
                .Select(r => new DetailsRequestViewModel
                {
                    Id = r.Id,
                    Title = r.Title,
                    Description = r.Description,
                    Priority = r.Priority,
                    Status = r.Status,
                    CreatedDate = r.CreatedDate,
                    UpdatedDate = r.UpdatedDate,
                    Attachments = r.Attachments,
                    CategoryName = r.Category.Name,
                    AssignedUserName = r.AssignedToUser != null ? r.AssignedToUser.UserName : "Not assigned",
                    AssignedTeamName = r.AssignedToTeam != null ? r.AssignedToTeam.Name : "Not assigned",
                    AuthorName = r.Author.UserName,
                    Responses = r.Responses.Select(rr => new RequestResponseViewModel
                    {
                        Id = rr.Id,
                        RequestId = rr.RequestId,
                        AuthorName = rr.Author.UserName,
                        CreateDate = rr.CreateDate,
                        ResponseText = rr.ResponseText
                    }).ToList()

                })
                .AsNoTracking()
                .FirstOrDefaultAsync();

            if (requestViewModel == null)
            {
                return NotFound();
            }

            return View(requestViewModel);
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
                Text = u.Username
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
        [Authorize]
        public async Task<IActionResult> Create(CreateRequestViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                var request = new Request
                {
                    Title = viewModel.Title,
                    Description = viewModel.Description,
                    Status = RequestStatus.Open,
                    Priority = viewModel.Priority,
                    CategoryId = viewModel.SelectedCategoryId,
                    AssignedToTeamId = viewModel.SelectedTeamId,
                    AssignedToUserId = viewModel.SelectedUserId,
                    AuthorId = _userManager.GetUserId(User)!
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
            var requestViewModel = await _context.Requests
                .Where(r => r.Id == id)
                .Select(r => new DetailsRequestViewModel
                {
                    Id = r.Id,
                    Title = r.Title,
                    Description = r.Description,
                    Priority = r.Priority,
                    Status = r.Status,
                    CreatedDate = r.CreatedDate,
                    UpdatedDate = r.UpdatedDate,
                    Attachments = r.Attachments,
                    CategoryName = r.Category.Name,
                    AssignedUserName = r.AssignedToUser != null ? r.AssignedToUser.UserName : "Not assigned",
                    AssignedTeamName = r.AssignedToTeam != null ? r.AssignedToTeam.Name : "Not assigned",
                    AuthorName = r.Author.UserName,
                    Responses = r.Responses.Select(rr => new RequestResponseViewModel
                    {
                        Id = rr.Id,
                        RequestId = rr.RequestId,
                        AuthorName = rr.Author.UserName,
                        CreateDate = rr.CreateDate,
                        ResponseText = rr.ResponseText
                    }).ToList()

                })
                .AsNoTracking()
                .FirstOrDefaultAsync();

            if (requestViewModel == null)
            {
                return NotFound();
            }

            ViewData["TeamSelectOptions"] = new SelectList(_context.Teams, "Id", "Name", requestViewModel.AssignedTeamName);
            ViewData["UserSelectOptions"] = new SelectList(_context.Users, "Id", "Id", requestViewModel.AssignedUserName);
            ViewData["CategorySelectOptions"] = new SelectList(_context.Categories, "Id", "Name", requestViewModel.CategoryName);
            return View(requestViewModel);
        }

        // POST: Requests/Edit/5
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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddResponse(int requestId, string responseText)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return Unauthorized();
            }

            var response = new RequestResponse
            {
                RequestId = requestId,
                ResponseText = responseText,
                AuthorId = _userManager.GetUserId(User),
                CreateDate = DateTime.UtcNow
            };

            _context.RequestResponses.Add(response);
            await _context.SaveChangesAsync();

            return RedirectToAction("Details", new { id = requestId });
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
