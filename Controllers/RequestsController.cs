using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using IssueManager.Utilities;
using IssueManager.Models.ViewModels.Requests;
using IssueManager.Services.Requests;
using IssueManager.Exceptions;
using IssueManager.Services.DataLists;

namespace IssueManager.Controllers
{
    public class RequestsController : Controller
    {
        private readonly IRequestService _requestsService;
        private readonly ISelectListService _selectListService;

        public RequestsController(IRequestService requestsService, ISelectListService selectListService)
        {
            _requestsService = requestsService;
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
            requestsListViewModel.SelectLists = _selectListService.PopulateRequestSelectLists();
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
            var requestViewModel = new CreateRequestViewModel
            {
                SelectLists = _selectListService.PopulateRequestSelectLists()
            };

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
                int? newRequestId;

                try
                {
                   newRequestId = await _requestsService.CreateRequestAsync(requestViewModel, User);
                   return RedirectToAction(nameof(Details), new { Id = newRequestId });
                }
                catch (InvalidFileTypeException ex)
                {
                    ModelState.AddModelError("", ex.Message);
                }
            }

            requestViewModel.SelectLists = _selectListService.PopulateRequestSelectLists(
                requestViewModel.AssignedUserId,
                requestViewModel.AssignedTeamId,
                requestViewModel.CategoryId
            );
            return View(requestViewModel);
        }

        public async Task<IActionResult> DownloadFile(int id)
        {
            var file = await _requestsService.GetAttachmentAsync(id);
            if (file == null)
            {
                return NotFound();
            }

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
                TempData["ErrorMessage"] = "This record was edited by another user. Please refresh the page and try again.";
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

            requestViewModel.SelectLists = _selectListService.PopulateRequestSelectLists(
                requestViewModel.AssignedUserId, 
                requestViewModel.AssignedTeamId, 
                requestViewModel.CategoryId
            );
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
                ModelState.AddModelError("", "This record was edited by another user. Please refresh the page and try again.");
            }

            if (ModelState.IsValid)
            {

                return RedirectToAction(nameof(Details), new { Id = id });
            }

            requestViewModel.SelectLists = _selectListService.PopulateRequestSelectLists(
                requestViewModel.AssignedUserId, 
                requestViewModel.AssignedTeamId, 
                requestViewModel.CategoryId
            );
            return View(requestViewModel);
        }

        [HttpPost]
        [Authorize(Roles = "User,Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddResponse(int requestId, string responseText)
        {            
            try
            {
                await _requestsService.AddResponseAsync(requestId, responseText, User);
            }
            catch (Exception) 
            {
                return NotFound(); 
            }

            return RedirectToAction("Edit", new { id = requestId });
        }
    }
}
