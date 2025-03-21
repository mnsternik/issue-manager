using AutoMapper;
using IssueManager.Data;
using IssueManager.Exceptions;
using IssueManager.Models;
using IssueManager.Models.ViewModels.Requests;
using IssueManager.Services.Files;
using IssueManager.Utilities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace IssueManager.Services.Requests
{
    public class RequestService : IRequestService
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;
        private readonly IMapper _mapper;
        private readonly IFileService _fileService;
        private readonly ILogger<RequestService> _logger;

        public RequestService(ApplicationDbContext context, UserManager<User> userManager, IMapper mapper, IFileService fileService, ILogger<RequestService> logger)
        {
            _context = context;
            _userManager = userManager;
            _mapper = mapper;
            _fileService = fileService;
            _logger = logger;
        }

        public async Task<RequestsListViewModel> GetRequestsAsync(RequestSearchFilters filters, int pageIndex)
        {
            _logger.LogInformation("Getting requests with filters: {@Filters}, page index: {PageIndex}", filters, pageIndex);

            try
            {
                // Retriving request sorted by CreateDate, appling search filter and then mapping to view model
                IQueryable<Request> baseQuery = _context.Requests
                    .AsNoTracking()
                    .OrderByDescending(r => r.CreateDate);
                IQueryable<Request> filteredQuery = ApplyFiltersToQuery(baseQuery, filters);
                IQueryable<RequestsListItemViewModel> mappedQuery = _mapper.ProjectTo<RequestsListItemViewModel>(filteredQuery);

                var requestsListViewModel = new RequestsListViewModel
                {
                    Requests = await PaginatedList<RequestsListItemViewModel>.CreateAsync(mappedQuery, pageIndex),
                    Filters = filters
                };

                _logger.LogInformation("Retrieved {RequestCount} requests", requestsListViewModel.Requests.Count);
                return requestsListViewModel;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting requests");
                throw; 
            }
        }

        public async Task<DetailsRequestViewModel?> GetRequestDetailsAsync(int id, ClaimsPrincipal currentUser)
        {
            _logger.LogInformation("Getting details for request {RequestId}", id);

            try
            {
                var requestViewModel = await _mapper
                    .ProjectTo<DetailsRequestViewModel>(_context.Requests.AsNoTracking())
                    .FirstOrDefaultAsync(r => r.Id == id);

                if (requestViewModel == null)
                {
                    _logger.LogWarning("Request {RequestId} not found", id);
                    return null;
                }

                var user = await _userManager.GetUserAsync(currentUser);
                if (user == null)
                {
                    _logger.LogWarning("Current user not found");
                    return requestViewModel;
                }

                // Checking current user's roles and team, to see if he's allowed to assign request to himself 
                var userRoles = await _userManager.GetRolesAsync(user);
                var currentUserTeamId = await _context.Users
                    .AsNoTracking()
                    .Where(u => u.Id == user.Id)
                    .Select(u => u.Team!.Id)
                    .FirstOrDefaultAsync();

                bool isReqNotAssignedToAnyTeam = requestViewModel.AssignedTeamId == null;
                bool isUserMemberOfAssignedTeam = requestViewModel.AssignedTeamId == currentUserTeamId;
                bool isCurrentUserAlreadyAssigned = requestViewModel.AssignedUserId == user.Id;
                bool isUserAdmin = userRoles.Contains("Admin");

                requestViewModel.AllowAssign = !isCurrentUserAlreadyAssigned
                    && (isUserMemberOfAssignedTeam || isReqNotAssignedToAnyTeam || isUserAdmin);
                requestViewModel.AllowEdit = isCurrentUserAlreadyAssigned;

                _logger.LogDebug("Permission calculation for request {RequestId}: AllowAssign={AllowAssign}, AllowEdit={AllowEdit}",
                    id, requestViewModel.AllowAssign, requestViewModel.AllowEdit);

                return requestViewModel;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting request details for {RequestId}", id);
                throw;
            }
        }

        public async Task<int> CreateRequestAsync(CreateRequestViewModel requestViewModel, ClaimsPrincipal currentUser)
        {
            _logger.LogInformation("Creating new request");

            try
            {
                var user = await _userManager.GetUserAsync(currentUser);
                var request = _mapper.Map<Request>(requestViewModel);

                request.AuthorId = user!.Id;
                request.CreateDate = DateTime.UtcNow;

                if (requestViewModel.Files.Count > 0) 
                {
                    _logger.LogDebug("Processing files for new request");
                    request.Attachments = await _fileService.ProcessFilesAsync(requestViewModel.Files);
                }

                _context.Add(request);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Created new request {RequestId} for user {UserId}", request.Id, user.Id);
                return request.Id;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while creating new request");
                throw;
            }
        }

        public async Task<Attachment?> GetAttachmentAsync(int id)
        {
            _logger.LogInformation("Retrieving attachment {AttachmentId}", id);

            try
            {
                var file = await _fileService.GetAttachmentAsync(id);
                if (file == null)
                {
                    _logger.LogWarning("Attachment {AttachmentId} not found", id);
                }
                return file;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving attachment {AttachmentId}", id);
                throw;
            }
        }

        public async Task AssignRequestAsync(int id, ClaimsPrincipal currentUser)
        {
            _logger.LogInformation("Assigning request {RequestId}", id);

            try
            {
                var request = await _context.Requests.FindAsync(id);
                var user = await _userManager.GetUserAsync(currentUser);

                if (request == null)
                {
                    _logger.LogError("Request {RequestId} not found for assignment", id);
                    throw new InvalidOperationException("Request not found");
                }

                request.AssignedUser = user;
                request.AssignedUserId = user!.Id;
                request.AssignedTeam = user.Team;
                request.AssignedTeamId = user.TeamId;
                request.UpdateDate = DateTime.UtcNow;

                _context.Update(request);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Assigned request {RequestId} to user {UserId} in team {TeamId}",
                    id, user.Id, user.TeamId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error assigning request {RequestId}", id);
                throw;
            }
        }

        public async Task<EditRequestViewModel?> GetEditRequestAsync(int id)
        {
            _logger.LogInformation("Getting edit view for request {RequestId}", id);

            try
            {
                var requestViewModel = await _mapper
                    .ProjectTo<EditRequestViewModel>(_context.Requests.AsNoTracking())
                    .FirstOrDefaultAsync(r => r.Id == id);

                if (requestViewModel == null)
                {
                    _logger.LogWarning("Request {RequestId} not found for edit", id);
                }

                return requestViewModel;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting edit view for request {RequestId}", id);
                throw;
            }
        }

        public async Task EditRequestAsync(EditRequestViewModel requestViewModel)
        {
            _logger.LogInformation("Updating request {RequestId}", requestViewModel.Id);

            try
            {
                var request = await _context.Requests.FindAsync(requestViewModel.Id);
                
                if (request == null)
                {
                    _logger.LogError("Request with ID: {RequestId} not found" , requestViewModel.Id);
                    throw new EntityNotFoundException(nameof(Request), requestViewModel.Id);
                }

                request.UpdateDate = DateTime.UtcNow;
                request.Priority = requestViewModel.Priority;
                request.CategoryId = requestViewModel.CategoryId;
                request.AssignedTeamId = requestViewModel.AssignedTeamId;
                request.AssignedUserId = requestViewModel.AssignedUserId;
                request.Status = requestViewModel.Status;

                _context.Update(request);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Successfully updated request {RequestId}", requestViewModel.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating request {RequestId}", requestViewModel.Id);
                throw;
            }
        }

        public async Task AddResponseAsync(int requestId, string responseText, ClaimsPrincipal currentUser)
        {
            _logger.LogInformation("Adding response to request {RequestId}", requestId);

            try
            {
                var user = await _userManager.GetUserAsync(currentUser);
                var response = new RequestResponse
                {
                    RequestId = requestId,
                    ResponseText = responseText,
                    AuthorId = user!.Id,
                    CreateDate = DateTime.UtcNow
                };

                _context.RequestResponses.Add(response);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Added response {ResponseId} to request {RequestId}", response.Id, requestId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding response to request {RequestId}", requestId);
                throw;
            }
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
