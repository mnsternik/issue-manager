using AutoMapper;
using IssueManager.Data;
using IssueManager.Helpers;
using IssueManager.Models;
using IssueManager.Models.ViewModels.UserManagement;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace IssueManager.Controllers
{
    public class UserManagementController : Controller
    {

        private readonly UserManager<User> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;

        public UserManagementController(
            UserManager<User> userManager,
            RoleManager<IdentityRole> roleManager,
            ApplicationDbContext context,
            IMapper mapper)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _context = context;
            _mapper = mapper;
        }

        public async Task<IActionResult> Index(string search, int pageIndex = 1)
        {
            const int pageSize = 10;

            IQueryable<User> query = _context.Users;

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(r => r.Name.ToLower().Contains(search) || r.Email.ToLower().Contains(search));
            }

            IQueryable<UsersListItemViewModel> mappedQuery = _mapper.ProjectTo<UsersListItemViewModel>(query);

            var paginatedRequestsList = await PaginatedList<UsersListItemViewModel>.CreateAsync(mappedQuery, pageIndex, pageSize);

            var usersListViewModel = new UsersListViewModel
            {
                Users = paginatedRequestsList,
                SearchString = search
            };

            return View(usersListViewModel);
        }

        public async Task<IActionResult> ManageUser(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _context.Users.FindAsync(id);

            if (user == null)
            {
                return NotFound();
            }

            var currentRoles = await _userManager.GetRolesAsync(user);
            var availableRoles = _roleManager.Roles.Select(r => r.Name).ToList();
            var assignedTeam = await _context.Teams.FindAsync(user.TeamId);

            var userViewModel = new ManageUserViewModel
            {
                Id = user.Id,
                Name = user.Name,
                Email = user.Email,
                TeamId = user.TeamId,
                TeamName = assignedTeam?.Name,
                CurrentRoles = currentRoles,
                AvailableRoles = availableRoles!,
            };

            ViewData["AssignedTeam"] = new SelectList(_context.Teams, "Id", "Name", userViewModel.TeamId);

            return View(userViewModel);
        }

        [HttpPost]
        public async Task<IActionResult> ManageUser(ManageUserViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByIdAsync(model.Id);

                await _userManager.AddToRolesAsync(user!, model.SelectedRoles);
            }

            return View(model);
        }
    }
}

