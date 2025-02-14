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

        const int pageSize = 10;

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

            var routeValues = filters.ToRouteValues(pageIndex);

            // If the incoming request URL contains unnecessary parameters, redirect to the clean version
            if (Request.Query.Count != routeValues.Count)
            {
                return RedirectToAction("Index", routeValues);
            }

            IQueryable<Request> query = _context.Requests.AsNoTracking().OrderByDescending(r => r.CreateDate);
            IQueryable<Request> filteredQuery = ApplyFiltersToQuery(query, filters);
            IQueryable<RequestsListItemViewModel> mappedQuery = _mapper.ProjectTo<RequestsListItemViewModel>(filteredQuery);

            var requestsListViewModel = new RequestsListViewModel
            {
                Requests = await PaginatedList<RequestsListItemViewModel>.CreateAsync(mappedQuery, pageIndex, pageSize),
                Filters = filters
            };

            PopulateSelectsList();
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
                    .AsNoTracking()
                    .Where(u => u.Id == user.Id)
                    .Select(u => u.Team!.Id)
                    .FirstOrDefaultAsync();

                bool isReqNotAssignedToAnyTeam = requestViewModel.AssignedTeamId == null; 
                bool isUserMemberOfAssignedTeam = requestViewModel.AssignedTeamId == currentUserTeamId;
                bool isCurrentUserAlreadyAssigned = requestViewModel.AssignedUserId == user.Id;

                requestViewModel.AllowAssign = (isUserMemberOfAssignedTeam && !isCurrentUserAlreadyAssigned) 
                    || (isReqNotAssignedToAnyTeam) 
                    || (userRoles.Contains("Admin") && !isCurrentUserAlreadyAssigned);
                requestViewModel.AllowEdit = isCurrentUserAlreadyAssigned;
            }

            return View(requestViewModel);
        }

        // GET: Requests/Create
        public IActionResult Create()
        {
            PopulateSelectsList(); 
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
                request.AuthorId = _userManager.GetUserId(User)!;

                if (viewModel.Files != null && viewModel.Files.Any())
                {
                    foreach (var file in viewModel.Files)
                    {
                        if (file.Length > 0 && file.Length < (2 * 1024 * 1024))
                        {
                            var allowedExtensions = new[] { ".jpg", ".png", ".pdf", ".docx", "doc", ".txt" };
                            var fileExtension = Path.GetExtension(file.FileName).ToLower();

                            if (!allowedExtensions.Contains(fileExtension))
                            {
                                ModelState.AddModelError("Files", "Invalid file type.");
                                return View(viewModel);
                            }

                            using (var memoryStream = new MemoryStream())
                            {
                                await file.CopyToAsync(memoryStream);
                                request.Attachments.Add(new Attachment
                                {
                                    FileName = file.FileName,
                                    ContentType = file.ContentType,
                                    FileData = memoryStream.ToArray()
                                });
                            }
                        }
                    }
                }

                _context.Add(request);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            return View(viewModel);
        }

        public async Task<IActionResult> DownloadFile(int id)
        {
            var file = await _context.Attachments.FindAsync(id);
            if (file == null) return NotFound();

            return File(file.FileData, file.ContentType, file.FileName);
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


            PopulateSelectsList(requestViewModel.AssignedUserId, requestViewModel.AssignedTeamId, requestViewModel.CategoryId);
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

            PopulateSelectsList(requestViewModel.AssignedUserId, requestViewModel.AssignedTeamId, requestViewModel.CategoryId);
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

        private void PopulateSelectsList(string? selectedUserId = null, int? selectedTeamId = null, int? selectedCategoryId = null)
        {
            ViewBag.UsersByTeam = _context.Users
                .GroupBy(u => u.TeamId)
                .ToDictionary(
                    g => g.Key.ToString(),
                    g => g.Select(u => new { id = u.Id, name = u.UserName }).ToList()
                );

            ViewData["TeamSelectOptions"] = new SelectList(_context.Teams, "Id", "Name", selectedTeamId);
            ViewData["UserSelectOptions"] = new SelectList(_context.Users, "Id", "UserName", selectedUserId);
            ViewData["CategorySelectOptions"] = new SelectList(_context.Categories, "Id", "Name", selectedCategoryId);
        }

        private IQueryable<Request> ApplyFiltersToQuery(IQueryable<Request> query, RequestSearchFilters filters)
        {
            if (filters.RequestId.HasValue)
            {
                query = query.Where(r => r.Id == filters.RequestId);
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
