using IssueManager.Exceptions;
using IssueManager.Models.ViewModels.Users;
using IssueManager.Services.DataLists;
using IssueManager.Services.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace IssueManager.Controllers
{
    public class UsersController : Controller
    {
        private readonly IUserService _userService;
        private readonly ISelectListService _selectListService;
        private readonly IRoleListService _roleListService;

        public UsersController(IUserService userService, ISelectListService selectListService, IRoleListService roleListService)
        {
            _userService = userService;
            _selectListService = selectListService;
            _roleListService = roleListService;
        }

        // GET: Users
        public async Task<IActionResult> Index(string search, int pageIndex = 1)
        {
            var usersListViewModel = await _userService.GetUsersAsync(search, pageIndex);
            return View(usersListViewModel);
        }

        // GET: Users/Create
        public IActionResult Create()
        {
            var userViewModel = new CreateUserViewModel
            {
                TeamSelectOptions = _selectListService.PopulateTeamSelectList(),
                RolesList = _roleListService.PopulateNewUserRolesList()
            };

            return View(userViewModel);
        }

        // POST: Users/Create
        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateUserViewModel userViewModel)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    await _userService.CreateUserAsync(userViewModel);
                    TempData["SuccessMessage"] = "New account created successfully!";
                    return RedirectToAction(nameof(Index));
                }
                catch (UserOperationException ex)
                {
                    foreach (var error in ex.Errors)
                    {
                        ModelState.AddModelError("", error);
                    }
                }
            }

            userViewModel.TeamSelectOptions = _selectListService.PopulateTeamSelectList();
            userViewModel.RolesList = _roleListService.PopulateNewUserRolesList(userViewModel.RolesList.SelectedRoles);
            return View(userViewModel);
        }

        // GET: Users/ChangeUserDetails/{id}
        public async Task<IActionResult> ChangeUserDetails(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            var userViewModel = await _userService.GetChangeUserDetailsAsync(id);

            if (userViewModel == null)
            {
                return NotFound();
            }

            userViewModel.TeamSelectOptions = _selectListService.PopulateTeamSelectList(userViewModel.TeamId);
            return View(userViewModel);
        }

        // POST: Users/ChangeUserDetails/{id}
        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangeUserDetails(ChangeUserDetailsViewModel userViewModel)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    await _userService.ChangeUserDetailsAsync(userViewModel);
                    TempData["SuccessMessage"] = "User details updated successfully.";
                    return RedirectToAction(nameof(Index));
                }
                catch (UserOperationException ex)
                {
                    foreach (var error in ex.Errors)
                    {
                        ModelState.AddModelError("", error);
                    }
                }
                catch (DbUpdateConcurrencyException)
                {
                    ModelState.AddModelError("", "This record was edited by another user. Please refresh the page and try again.");
                }
            }

            userViewModel.TeamSelectOptions = _selectListService.PopulateTeamSelectList(userViewModel.TeamId);
            return View(userViewModel);
        }

        // GET: Users/ChangeUserPassword/{id}
        public async Task<IActionResult> ChangeUserPassword(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            var userViewModel = await _userService.GetChangeUserPasswordAsync(id);

            if (userViewModel == null)
            {
                return NotFound();
            }

            return View(userViewModel);
        }

        // POST: Users/ChangeUserPassword/{id}
        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangeUserPassword(ChangeUserPasswordViewModel userViewModel)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    await _userService.ChangeUserPasswordAsync(userViewModel);
                    TempData["SuccessMessage"] = "Password changed successfully.";
                    return RedirectToAction(nameof(Index));
                }
                catch (UserOperationException ex)
                {
                    foreach (var error in ex.Errors)
                    {
                        ModelState.AddModelError("", error);
                    }
                }
                catch (DbUpdateConcurrencyException)
                {
                    ModelState.AddModelError("", "This record was edited by another user. Please refresh the page and try again.");
                }
            }

            return View(userViewModel);
        }

        // GET: Users/ChangeUserRoles/{id}
        public async Task<IActionResult> ChangeUserRoles(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            var userViewModel = await _userService.GetChangeUserRolesAsync(id);

            if (userViewModel == null)
            {
                return NotFound();
            }

            userViewModel.RolesList = await _roleListService.PopulateExistingUserRolesListAsync(userViewModel.Id);
            return View(userViewModel);
        }

        // POST: Users/ChangeUserRoles/{id}
        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangeUserRoles(ChangeUserRolesViewModel userViewModel)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    await _userService.ChangeUserRolesAsync(userViewModel);
                    TempData["SuccessMessage"] = "User roles updated successfully.";
                    return RedirectToAction(nameof(Index));
                }
                catch (UserOperationException ex)
                {
                    foreach (var error in ex.Errors)
                    {
                        ModelState.AddModelError("", error);
                    }
                }
                catch (DbUpdateConcurrencyException)
                {
                    ModelState.AddModelError("", "This record was edited by another user. Please refresh the page and try again.");
                }
            }

            userViewModel.RolesList = await _roleListService.PopulateExistingUserRolesListAsync(userViewModel.Id);
            return View(userViewModel);
        }
    }
}

