using IssueManager.Exceptions;
using IssueManager.Models;
using IssueManager.Models.ViewModels.Users;
using Microsoft.AspNetCore.Identity;

namespace IssueManager.Services.DataLists
{
    public class RoleListService : IRoleListService
    {
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UserManager<User> _userManager;
        
        public RoleListService(RoleManager<IdentityRole> roleManager, UserManager<User> userManager) 
        {
            _roleManager = roleManager;
            _userManager = userManager; 
        }

        public async Task<ExistingUserRolesListViewModel> PopulateExistingUserRolesListAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId) ?? throw new UserOperationException(["User not found"]); 
            var currentRoles = (List<string>)await _userManager.GetRolesAsync(user);

            var viewModel = new ExistingUserRolesListViewModel
            {
                AvailableRoles = _roleManager.Roles.Select(r => r.Name).ToList()!,
                CurrentRoles = currentRoles,
                SelectedRoles = currentRoles
            };

            return viewModel;
        }

        public NewUserRolesListViewModel PopulateNewUserRolesList(List<string>? selectedRoles = null)
        {
            var viewModel = new NewUserRolesListViewModel
            {
                AvailableRoles = _roleManager.Roles.Select(r => r.Name).ToList()!,
                SelectedRoles = selectedRoles ?? []
            };

            return viewModel;
        }
    }
}
