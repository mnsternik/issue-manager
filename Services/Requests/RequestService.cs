using AutoMapper;
using IssueManager.Data;
using IssueManager.Models;
using IssueManager.Models.ViewModels.Requests;
using IssueManager.Services.Files;
using IssueManager.Services.SelectLists;
using IssueManager.Utilities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace IssueManager.Services.Requests
{
    public class RequestService : IRequestService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly UserManager<User> _userManager;
        private readonly IFileService _fileService;
        private readonly ISelectListService _selectListService;

        private const int _pageSize = 10;

        public RequestService(ApplicationDbContext context, IMapper mapper, UserManager<User> userManager, IFileService fileService, ISelectListService selectListService)
        {
            _context = context;
            _mapper = mapper;
            _userManager = userManager;
            _fileService = fileService;
            _selectListService = selectListService;
        }

        public async Task<RequestsListViewModel> GetRequestsAsync(RequestSearchFilters filters, int pageIndex)
        {
            IQueryable<Request> baseQuery = _context.Requests
                .AsNoTracking()
                .OrderByDescending(r => r.CreateDate);
            IQueryable<Request> filteredQuery = ApplyFiltersToQuery(baseQuery, filters);
            IQueryable<RequestsListItemViewModel> mappedQuery = _mapper.ProjectTo<RequestsListItemViewModel>(filteredQuery);

            var requestsListViewModel = new RequestsListViewModel
            {
                Requests = await PaginatedList<RequestsListItemViewModel>.CreateAsync(mappedQuery, pageIndex),
                Filters = filters,
                SelectLists = _selectListService.PopulateRequestSelectLists()
            };

            return requestsListViewModel;
        }

        public CreateRequestViewModel GetCreateRequest()
        {
            var viewModel = new CreateRequestViewModel();
            viewModel.SelectLists = _selectListService.PopulateRequestSelectLists();

            return viewModel; 
        }

        public async Task<DetailsRequestViewModel?> GetRequestDetailsAsync(int id, ClaimsPrincipal currentUser)
        {
            var requestViewModel = await _mapper
                .ProjectTo<DetailsRequestViewModel>(_context.Requests.AsNoTracking())
                .FirstOrDefaultAsync(r => r.Id == id);

            if (requestViewModel == null)
            {
                return null;
            }

            var user = await _userManager.GetUserAsync(currentUser);
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

                requestViewModel.AllowAssign =
                    (isUserMemberOfAssignedTeam && !isCurrentUserAlreadyAssigned)
                    || (userRoles.Contains("Admin") && !isCurrentUserAlreadyAssigned)
                    || (isReqNotAssignedToAnyTeam);

                requestViewModel.AllowEdit = isCurrentUserAlreadyAssigned;
            }

            return requestViewModel;
        }

        public async Task<int> CreateRequestAsync(CreateRequestViewModel requestViewModel, string currentUserId)
        {
            var request = _mapper.Map<Request>(requestViewModel);

            request.AuthorId = currentUserId;
            request.Attachments = await _fileService.ProcessFilesAsync(requestViewModel.Files);

            _context.Add(request);
            await _context.SaveChangesAsync();

            return request.Id;
        }

        public async Task AssignRequestAsync(int id, ClaimsPrincipal currentUser) 
        {
            var request = await _context.Requests.FindAsync(id);
            var user = await _userManager.GetUserAsync(currentUser);

            if (request == null)
            {
                throw new InvalidOperationException("Request not found");
            };

            request.AssignedUser = user;
            request.AssignedUserId = user!.Id;
            request.AssignedTeam = user.Team;
            request.AssignedTeamId = user.TeamId;
            request.UpdateDate = DateTime.UtcNow;

            _context.Update(request);
            await _context.SaveChangesAsync();
        }

        public async Task<EditRequestViewModel?> GetEditRequestAsync(int id)
        {
            var requestViewModel = await _mapper
                .ProjectTo<EditRequestViewModel>(_context.Requests.AsNoTracking())
                .FirstOrDefaultAsync(r => r.Id == id);

            if (requestViewModel == null)
            {
                return null;
            }
            else
            {
                requestViewModel.SelectLists = _selectListService.PopulateRequestSelectLists(requestViewModel.AssignedUserId, requestViewModel.AssignedTeamId, requestViewModel.CategoryId);
                return requestViewModel;
            }
        }

        public async Task EditRequestAsync(EditRequestViewModel requestViewModel)
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
                // TODO
            }
        }

        public async Task AddResponseAsync(int requestId, string responseText, string currentUserId)
        {
            var response = new RequestResponse
            {
                RequestId = requestId,
                ResponseText = responseText,
                AuthorId = currentUserId,
                CreateDate = DateTime.UtcNow
            };

            _context.RequestResponses.Add(response);
            await _context.SaveChangesAsync();
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
