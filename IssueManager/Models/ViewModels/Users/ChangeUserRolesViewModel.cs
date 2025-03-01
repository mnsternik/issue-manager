using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace IssueManager.Models.ViewModels.Users
{
    public class ChangeUserRolesViewModel
    {
        public string Id { get; set; } = string.Empty;

        [ValidateNever]
        public string Name { get; set; } = string.Empty;

        [ValidateNever]
        public string Email { get; set; } = string.Empty; 
        public ExistingUserRolesListViewModel RolesList { get; set; } = new ExistingUserRolesListViewModel();
    }
}
