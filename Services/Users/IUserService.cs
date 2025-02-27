using IssueManager.Models;
using IssueManager.Models.ViewModels.Users;

namespace IssueManager.Services.Users
{
    public interface IUserService
    {
        public Task<UsersListViewModel> GetUsersAsync(string search, int pageIndex);
        public Task CreateUserAsync(CreateUserViewModel userViewModel);
        public Task<ChangeUserDetailsViewModel?> GetChangeUserDetailsAsync(string id);
        public Task ChangeUserDetailsAsync(ChangeUserDetailsViewModel userViewModel);
        public Task<ChangeUserRolesViewModel?> GetChangeUserRolesAsync(string id);
        public Task ChangeUserRolesAsync(ChangeUserRolesViewModel userViewModel);
        public Task<ChangeUserPasswordViewModel?> GetChangeUserPasswordAsync(string id);
        public Task ChangeUserPasswordAsync(ChangeUserPasswordViewModel userViewModel);
    }
}
