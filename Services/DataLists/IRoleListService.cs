using IssueManager.Models;
using IssueManager.Models.ViewModels.Users;

namespace IssueManager.Services.DataLists
{
    public interface IRoleListService
    {
        public Task<ExistingUserRolesListViewModel> PopulateExistingUserRolesListAsync(string userId);
        public NewUserRolesListViewModel PopulateNewUserRolesList(List<string>? selectedRoles = null);
    }
}
