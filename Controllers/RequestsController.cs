using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using IssueManager.Data;
using IssueManager.Models;
using IssueManager.Models.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using IssueManager.Helpers;
using AutoMapper;

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
        public async Task<IActionResult> Index(string search, int pageIndex = 1)
        {
            const int pageSize = 10;

            IQueryable<Request> query = _context.Requests;

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(r => r.Title.ToLower().Contains(search));
            }

            IQueryable<RequestsListItemViewModel> mappedQuery = _mapper.ProjectTo<RequestsListItemViewModel>(query);

            var paginatedRequestsList = await PaginatedList<RequestsListItemViewModel>.CreateAsync(mappedQuery, pageIndex, pageSize);

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
                var teamName = await _context.Users
                    .Where(u => u.Id == user.Id)
                    .Select(u => u.Team!.Name)
                    .FirstOrDefaultAsync();

                requestViewModel.AllowAssign = teamName == requestViewModel.AssignedTeamName; // TODO: Change it to compare ID's, not names. 
                requestViewModel.AllowEdit = user.Name == requestViewModel.AssignedUserName;
            }

            return View(requestViewModel);
        }

        // GET: Requests/Create
        public IActionResult Create()
        {
            ViewData["TeamSelectOptions"] = new SelectList(_context.Teams, "Id", "Name");
            ViewData["UserSelectOptions"] = new SelectList(_context.Users, "Id", "Name");
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

            ViewData["TeamSelectOptions"] = new SelectList(_context.Teams, "Id", "Name", requestViewModel.AssignedTeamId);
            ViewData["UserSelectOptions"] = new SelectList(_context.Users, "Id", "Name", requestViewModel.AssignedUserId);
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
    }
}
