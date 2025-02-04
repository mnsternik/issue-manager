using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using IssueManager.Data;
using IssueManager.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using IssueManager.Utilities;
using AutoMapper;
using IssueManager.Models.ViewModels.Requests;

namespace IssueManager.Controllers
{
    public class RequestsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;
        private readonly IMapper _mapper;

        public RequestsController(ApplicationDbContext context, UserManager<User> userManager, IMapper mapper)
        {
            _context = context;
            _userManager = userManager;
            _mapper = mapper;
        }

        // GET: Requests
        [HttpGet]
        public async Task<IActionResult> Index(RequestSearchFilters filters, int pageIndex = 1)
        {
            const int pageSize = 10;

            IQueryable<Request> filteredQuery = ApplyFiltersToQuery(_context.Requests, filters);
            IQueryable<RequestsListItemViewModel> mappedQuery = _mapper.ProjectTo<RequestsListItemViewModel>(filteredQuery);

            var requestsListViewModel = new RequestsListViewModel
            {
                Requests = await PaginatedList<RequestsListItemViewModel>.CreateAsync(mappedQuery, pageIndex, pageSize),
                Filters = filters
            };

            ViewBag.UsersByTeam = _context.Users
                .GroupBy(u => u.TeamId)
                .ToDictionary(
                    g => g.Key.ToString(),
                    g => g.Select(u => new { id = u.Id, name = u.UserName }).ToList()
                );

            ViewData["TeamSelectOptions"] = new SelectList(_context.Teams, "Id", "Name");
            ViewData["UserSelectOptions"] = new SelectList(_context.Users, "Id", "UserName");
            ViewData["CategorySelectOptions"] = new SelectList(_context.Categories, "Id", "Name");

            return View(requestsListViewModel);
        }

        // GET: Requests/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var requestViewModel = await _mapper
                .ProjectTo<DetailsRequestViewModel>(_context.Requests.AsNoTracking())
                .FirstOrDefaultAsync(r => r.Id == id);

            if (requestViewModel == null)
            {
                return NotFound();
            }

            var user = await _userManager.GetUserAsync(User);
            if (user != null)
            {
                var userRoles = await _userManager.GetRolesAsync(user);

                var currentUserTeamId = await _context.Users
                    .Where(u => u.Id == user.Id)
                    .Select(u => u.Team!.Id)
                    .FirstOrDefaultAsync();

                // Allow assign if request is not assigned to user yet, but is assigned to the team of current user, or current user is admin (and he is not yet assigned to task)
                requestViewModel.AllowAssign =
                    (currentUserTeamId == requestViewModel.AssignedTeamId && requestViewModel.AssignedTeamId == null)
                    || (userRoles.Contains("Admin") && requestViewModel.AssignedUserId != user.Id);

                requestViewModel.AllowEdit = user.UserName == requestViewModel.AssignedUserName;
            }

            return View(requestViewModel);
        }

        // GET: Requests/Create
        public IActionResult Create()
        {
            ViewBag.UsersByTeam = _context.Users
                .GroupBy(u => u.TeamId)
                .ToDictionary(
                    g => g.Key.ToString(),
                    g => g.Select(u => new { id = u.Id, name = u.UserName }).ToList()
                );

            ViewData["TeamSelectOptions"] = new SelectList(_context.Teams, "Id", "Name");
            ViewData["UserSelectOptions"] = new SelectList(_context.Users, "Id", "UserName");
            ViewData["CategorySelectOptions"] = new SelectList(_context.Categories, "Id", "Name");

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
                var request = _mapper.Map<Request>(viewModel);
                request.Status = RequestStatus.Open;
                request.AuthorId = _userManager.GetUserId(User)!;

                _context.Add(request);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Assign(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            var request = await _context.Requests.FindAsync(id);

            if (request == null)
            {
                return NotFound();
            };

            request.AssignedUser = user;
            request.AssignedUserId = user!.Id;
            request.AssignedTeam = user.Team;
            request.AssignedTeamId = user.TeamId;
            request.Status = RequestStatus.InProgress; 
            request.UpdateDate = DateTime.UtcNow;

            try
            {
                _context.Update(request);
                await _context.SaveChangesAsync();

            }
            catch (DbUpdateConcurrencyException)
            {
                throw;
            }

            return RedirectToAction(nameof(Details), new { Id = id });
        }

        // GET: Requests/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var requestViewModel = await _mapper
                .ProjectTo<EditRequestViewModel>(_context.Requests.AsNoTracking())
                .FirstOrDefaultAsync(r => r.Id == id);

            if (requestViewModel == null)
            {
                return NotFound();
            }

            ViewBag.UsersByTeam = _context.Users
                .GroupBy(u => u.TeamId)
                .ToDictionary(
                    g => g.Key.ToString(),
                    g => g.Select(u => new { id = u.Id, name = u.UserName }).ToList()
                );


            ViewData["TeamSelectOptions"] = new SelectList(_context.Teams, "Id", "Name", requestViewModel.AssignedTeamId);
            ViewData["UserSelectOptions"] = new SelectList(_context.Users, "Id", "UserName", requestViewModel.AssignedUserId);
            ViewData["CategorySelectOptions"] = new SelectList(_context.Categories, "Id", "Name", requestViewModel.CategoryId);

            return View(requestViewModel);
        }

        // POST: Requests/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Title,Description,Status,Priority,CreateDate,UpdateDate,AuthorName,AuthorId,CategoryId,AssignedUserId,AssignedTeamId")] EditRequestViewModel requestViewModel)
        {
            if (id != requestViewModel.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                var request = _mapper.Map<Request>(requestViewModel);
                request.UpdateDate = DateTime.UtcNow;

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

            ViewData["AssignedToTeamId"] = new SelectList(_context.Teams, "Id", "Name", requestViewModel.AssignedTeamId);
            ViewData["AssignedToUserId"] = new SelectList(_context.Users, "Id", "Id", requestViewModel.AssignedUserId);
            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name", requestViewModel.CategoryId);

            return View(requestViewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddResponse(int requestId, string responseText)
        {
            if (!User.Identity!.IsAuthenticated)
            {
                return Unauthorized();
            }

            var response = new RequestResponse
            {
                RequestId = requestId,
                ResponseText = responseText,
                AuthorId = _userManager.GetUserId(User)!,
                CreateDate = DateTime.UtcNow
            };

            _context.RequestResponses.Add(response);
            await _context.SaveChangesAsync();

            return RedirectToAction("Edit", new { id = requestId });
        }

        // GET: Requests/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var request = await _context.Requests
                .Include(r => r.AssignedTeam)
                .Include(r => r.AssignedUser)
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

        private IQueryable<Request> ApplyFiltersToQuery(IQueryable<Request> query, RequestSearchFilters filters)
        {
            if (!string.IsNullOrWhiteSpace(filters.Id))
            {
                query = query.Where(r => r.Id.ToString().Contains(filters.Id));
            }
            if (!string.IsNullOrWhiteSpace(filters.Title))
            {
                query = query.Where(r => r.Title.Contains(filters.Title));
            }
            if (filters.Priority.HasValue)
            {
                query = query.Where(r => r.Priority == filters.Priority);
            }
            if (filters.Status.HasValue)
            {
                query = query.Where(r => r.Status == filters.Status);
            }
            if (filters.CategoryId.HasValue)
            {
                query = query.Where(r => r.CategoryId == filters.CategoryId);
            }
            if (!string.IsNullOrWhiteSpace(filters.AssignedUserId))
            {
                query = query.Where(r => r.AssignedUserId == filters.AssignedUserId);
            }
            if (!string.IsNullOrWhiteSpace(filters.AuthorId))
            {
                query = query.Where(r => r.AuthorId == filters.AuthorId);
            }
            if (filters.AssignedTeamId.HasValue)
            {
                query = query.Where(r => r.AssignedTeamId == filters.AssignedTeamId);
            }
            if (filters.CreatedBefore.HasValue)
            {
                query = query.Where(r => r.CreateDate <= filters.CreatedBefore);
            }
            if (filters.CreatedAfter.HasValue)
            {
                query = query.Where(r => r.CreateDate >= filters.CreatedAfter);
            }
            if (filters.UpdatedBefore.HasValue)
            {
                query = query.Where(r => r.UpdateDate <= filters.UpdatedBefore);
            }
            if (filters.UpdatedAfter.HasValue)
            {
                query = query.Where(r => r.UpdateDate >= filters.UpdatedAfter);
            }

            return query;
        }
    }
}
