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
using IssueManager.Services.Requests;
using IssueManager.Exceptions;
using IssueManager.Services.Files;
using System.Data;

namespace IssueManager.Controllers
{
    public class RequestsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;
        private readonly IMapper _mapper;
        private readonly IRequestService _requestsService;
        private readonly IFileService _fileService; 

        const int pageSize = 10;

        public RequestsController(ApplicationDbContext context, UserManager<User> userManager, IMapper mapper, IRequestService requestsService, IFileService fileService)
        {
            _context = context;
            _userManager = userManager;
            _mapper = mapper;
            _requestsService = requestsService;
            _fileService = fileService;
        }

        // GET: Requests
        [HttpGet]
        public async Task<IActionResult> Index(RequestSearchFilters filters, int pageIndex = 1)
        {
            var routeValues = filters.ToRouteValues(pageIndex);

            // Handle URL parameters cleanup
            if (Request.Query.Count != routeValues.Count)
            {
                return RedirectToAction("Index", routeValues);
            }

            var requestsListViewModel = await _requestsService.GetRequestsAsync(filters, pageIndex);

            PopulateSelectsList();
            return View(requestsListViewModel);
        }

        // GET: Requests/Details/5
        [HttpGet]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var requestViewModel = await _requestsService.GetRequestDetailsAsync((int)id, User); 

            if (requestViewModel == null)
            {
                return NotFound();
            }

            return View(requestViewModel);
        }

        // GET: Requests/Create
        [Authorize(Roles = "User,Admin")]
        public IActionResult Create()
        {
            PopulateSelectsList();
            return View(new CreateRequestViewModel());
        }

        // POST: Requests/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "User,Admin")]
        public async Task<IActionResult> Create(CreateRequestViewModel requestViewModel)
        {
            if (ModelState.IsValid)
            {           
                var currentUserId = _userManager.GetUserId(User)!;

                try
                {
                    await _requestsService.CreateRequestAsync(requestViewModel, currentUserId);
                }
                catch (InvalidFileTypeException ex)
                {
                    ModelState.AddModelError("", ex.Message);
                    PopulateSelectsList();
                    return View(requestViewModel);
                }
             
                return RedirectToAction(nameof(Index));
            }

            PopulateSelectsList();
            return View(requestViewModel);
        }

        public async Task<IActionResult> DownloadFile(int id)
        {
            var file = await _fileService.GetAttachmentAsync(id); 
            if (file == null) return NotFound();

            return File(file.FileData, file.ContentType, file.FileName);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "User,Admin")]
        public async Task<IActionResult> Assign(int id)
        {
            try
            {
                await _requestsService.AssignRequestAsync(id, User); 
            }
            catch (InvalidOperationException)
            {
                return NotFound(); 
            }
            //Add DbUpdateConcurrencyException handling

            return RedirectToAction(nameof(Details), new { Id = id });
        }

        // GET: Requests/Edit/5
        [Authorize(Roles = "User,Admin")]
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
        [Authorize(Roles = "User,Admin")]
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
        [Authorize(Roles = "User,Admin")]
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
        [Authorize(Roles = "Admin")]
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
        [Authorize(Roles = "Admin")]
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
                    g => g.Select(u => new { id = u.Id, name = u.Name }).ToList()
                );

            ViewData["TeamSelectOptions"] = new SelectList(_context.Teams, "Id", "Name", selectedTeamId);
            ViewData["UserSelectOptions"] = new SelectList(_context.Users, "Id", "Name", selectedUserId);
            ViewData["CategorySelectOptions"] = new SelectList(_context.Categories, "Id", "Name", selectedCategoryId);
        }
    }
}
