using AutoMapper;
using IssueManager.Data;
using IssueManager.Models;
using IssueManager.Models.ViewModels.UserManagement;
using IssueManager.Utilities;
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

        [HttpGet]
        public async Task<IActionResult> Index(string search, int pageIndex = 1)
        {
            const int pageSize = 10;

            IQueryable<User> query = _context.Users;

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(r => r.UserName.ToLower().Contains(search) || r.Email.ToLower().Contains(search));
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

        [HttpGet]
        public IActionResult Create()
        {
            var model = new CreateUserViewModel
            {
                AvailableRoles = _roleManager.Roles.Select(r => r.Name).ToList()!,
                Teams = _context.Teams.Select(t => new SelectListItem
                {
                    Value = t.Id.ToString(),
                    Text = t.Name
                }).ToList()
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateUserViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new User
                {
                    UserName = model.UserName,
                    Email = model.Email,
                    TeamId = model.TeamId,
                };

                var result = await _userManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    foreach (var role in model.SelectedRoles)
                    {
                        await _userManager.AddToRoleAsync(user, role);
                    }

                    return RedirectToAction(nameof(Index));
                }
            }

            model.AvailableRoles = _roleManager.Roles.Select(r => r.Name).ToList()!;
            model.Teams = _context.Teams.Select(t => new SelectListItem
            {
                Value = t.Id.ToString(),
                Text = t.Name
            }).ToList();

            return View(model); 
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
                UserName = user.UserName,
                Email = user.Email,
                TeamId = user.TeamId,
                TeamName = assignedTeam?.Name,
                CurrentRoles = currentRoles,
                AvailableRoles = availableRoles!,
                SelectedRoles = new List<string>()
            };

            ViewData["AssignedTeam"] = new SelectList(_context.Teams, "Id", "Name", userViewModel.TeamId);

            return View(userViewModel);
        }

        [HttpPost]
        public async Task<IActionResult> ManageUser(ManageUserViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _context.Users
                    .Include(u => u.Team)
                    .FirstOrDefaultAsync(u => u.Id == model.Id);

                if (user == null)
                {
                    return NotFound("User not found");
                }

                var team = await _context.Teams.FindAsync(model.TeamId);

                user.Team = team;
                user.Email = model.Email;
                user.UserName = model.UserName;

                var userRoles = await _userManager.GetRolesAsync(user);
                await _userManager.RemoveFromRolesAsync(user, userRoles);
                await _userManager.AddToRolesAsync(user!, model.SelectedRoles); // TODO: what is no roles??

                try
                {
                    _context.Update(user);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    throw;
                }
            }

            return RedirectToAction(nameof(Index));
        }
    }
}

