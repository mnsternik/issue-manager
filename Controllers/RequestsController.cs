using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using IssueManager.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using IssueManager.Utilities;
using IssueManager.Models.ViewModels.Requests;
using IssueManager.Services.Requests;
using IssueManager.Exceptions;
using IssueManager.Services.Files;
using IssueManager.Services.SelectLists;

namespace IssueManager.Controllers
{
    public class RequestsController : Controller
    {
        private readonly UserManager<User> _userManager;
        private readonly IRequestService _requestsService;
        private readonly IFileService _fileService; 
        private readonly ISelectListService _selectListService;

        public RequestsController(UserManager<User> userManager, IRequestService requestsService, IFileService fileService, ISelectListService selectListService)
        {
            _userManager = userManager;
            _requestsService = requestsService;
            _fileService = fileService;
            _selectListService = selectListService;
        }

        // GET: Requests
        public async Task<IActionResult> Index(RequestSearchFilters filters, int pageIndex = 1)
        {
            var routeValues = filters.ToRouteValues(pageIndex);

            // Handle URL parameters cleanup
            if (Request.Query.Count != routeValues.Count)
            {
                return RedirectToAction("Index", routeValues);
            }

            var requestsListViewModel = await _requestsService.GetRequestsAsync(filters, pageIndex);

            return View(requestsListViewModel);
        }

        // GET: Requests/Details/5
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
            var requestViewModel = _requestsService.GetCreateRequest();
            return View(requestViewModel);
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
                int? newRequestId;

                try
                {
                   newRequestId = await _requestsService.CreateRequestAsync(requestViewModel, currentUserId);
                }
                catch (InvalidFileTypeException ex)
                {
                    ModelState.AddModelError("", ex.Message);
                    requestViewModel.SelectLists = _selectListService.PopulateRequestSelectLists(requestViewModel.AssignedUserId, requestViewModel.AssignedTeamId, requestViewModel.CategoryId);
                    return View(requestViewModel);
                }

                return RedirectToAction(nameof(Details), new { Id = newRequestId }); 
            }

            requestViewModel.SelectLists = _selectListService.PopulateRequestSelectLists(requestViewModel.AssignedUserId, requestViewModel.AssignedTeamId, requestViewModel.CategoryId);
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
            catch (DbUpdateConcurrencyException)
            {
                throw; // what to do?
            }

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

            var requestViewModel = await _requestsService.GetEditRequestAsync((int)id);

            if (requestViewModel == null)
            {
                return NotFound();
            }

            return View(requestViewModel);
        }

        // POST: Requests/Edit/5
        [HttpPost]
        [Authorize(Roles = "User,Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, EditRequestViewModel requestViewModel)
        {
            if (id != requestViewModel.Id)
            {
                return NotFound();
            }

            try
            {
                await _requestsService.EditRequestAsync(requestViewModel); 
            }
            catch (DbUpdateConcurrencyException)
            {
                // TODO
            }

            if (ModelState.IsValid)
            {

                return RedirectToAction(nameof(Details), new { Id = id });
            }

            requestViewModel.SelectLists = _selectListService.PopulateRequestSelectLists(requestViewModel.AssignedUserId, requestViewModel.AssignedTeamId, requestViewModel.CategoryId);
            return View(requestViewModel);
        }

        [HttpPost]
        [Authorize(Roles = "User,Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddResponse(int requestId, string responseText)
        {
            var currentUserId = _userManager.GetUserId(User)!;
            
            try
            {
                await _requestsService.AddResponseAsync(requestId, responseText, currentUserId);
            }
            catch (Exception) // what kinds of execptions may occur here?
            {
                // TODO 
            }

            return RedirectToAction("Edit", new { id = requestId });
        }
    }
}
