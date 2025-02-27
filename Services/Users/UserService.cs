using AutoMapper;
using IssueManager.Data;
using IssueManager.Exceptions;
using IssueManager.Models;
using IssueManager.Models.ViewModels.Users;
using IssueManager.Services.DataLists;
using IssueManager.Utilities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace IssueManager.Services.Users
{
    public class UserService : IUserService
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;
        private readonly IMapper _mapper;

        public UserService(ApplicationDbContext context, UserManager<User> userManager, IMapper mapper)
        {
            _context = context;
            _userManager = userManager; 
            _mapper = mapper; 
        }

        public async Task<UsersListViewModel> GetUsersAsync(string search, int pageIndex)
        {
            IQueryable<User> query = _context.Users;

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(r => r.Name.ToLower().Contains(search) || r.Email!.ToLower().Contains(search));
            }

            IQueryable<UsersListItemViewModel> mappedQuery = _mapper.ProjectTo<UsersListItemViewModel>(query);

            var usersListViewModel = new UsersListViewModel
            {
                Users = await PaginatedList<UsersListItemViewModel>.CreateAsync(mappedQuery, pageIndex),
                SearchString = search
            };

            return usersListViewModel; 
        }

        public async Task CreateUserAsync(CreateUserViewModel userViewModel)
        {
            var user = _mapper.Map<User>(userViewModel);
            user.UserName = user.Email; 
           
            var createResult = await _userManager.CreateAsync(user, userViewModel.Password);

            if (!createResult.Succeeded)
            {
                throw new UserOperationException(createResult.Errors.Select(e => e.Description).ToList());
            }

            foreach (var role in userViewModel.RolesList.SelectedRoles)
            {
                var addRoleReuslt = await _userManager.AddToRoleAsync(user, role);

                if (!addRoleReuslt.Succeeded)
                {
                    throw new UserOperationException(createResult.Errors.Select(e => e.Description).ToList());
                }
            }
        }

        public async Task<ChangeUserDetailsViewModel?> GetChangeUserDetailsAsync(string id)
        {
            var user = await _context.Users.FindAsync(id);

            if (user == null)
            {
                return null;
            }

            var userViewModel = _mapper.Map<ChangeUserDetailsViewModel>(user);
            return userViewModel; 
        }

        public async Task ChangeUserDetailsAsync(ChangeUserDetailsViewModel userViewModel)
        {
            var user = await _context.Users.FindAsync(userViewModel.Id) ?? throw new UserOperationException(["User not found"]);
            var team = await _context.Teams.FindAsync(userViewModel.TeamId);

            user.Team = team!;
            user.Email = userViewModel.Email!;
            user.Name = userViewModel.Name!;

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

        public async Task<ChangeUserRolesViewModel?> GetChangeUserRolesAsync(string id)
        {
            var user = await _context.Users.FindAsync(id);

            if (user == null)
            {
                return null;
            }

            var userViewModel = _mapper.Map<ChangeUserRolesViewModel>(user);
            return userViewModel;
        }

        public async Task ChangeUserRolesAsync(ChangeUserRolesViewModel userViewModel)
        {
            var user = await _context.Users.FindAsync(userViewModel.Id) ?? throw new UserOperationException(["User not found"]); 
            var currentRoles = await _userManager.GetRolesAsync(user);

            var removeResult = await _userManager.RemoveFromRolesAsync(user, currentRoles);

            if (!removeResult.Succeeded)
            {
                throw new UserOperationException(removeResult.Errors.Select(r => r.Description).ToList());
            }

            if (userViewModel.RolesList.SelectedRoles.Count > 0)
            {
                var addResult = await _userManager.AddToRolesAsync(user, userViewModel.RolesList.SelectedRoles);

                if (!addResult.Succeeded)
                {
                    throw new UserOperationException(addResult.Errors.Select(r => r.Description).ToList());
                }
            }
        }

        public async Task<ChangeUserPasswordViewModel?> GetChangeUserPasswordAsync(string id)
        {
            var user = await _context.Users.FindAsync(id);

            if (user == null)
            {
                return null;
            }

            var userViewModel = _mapper.Map<ChangeUserPasswordViewModel>(user);
            return userViewModel;
        }

        public async Task ChangeUserPasswordAsync(ChangeUserPasswordViewModel userViewModel)
        {
            var user = await _context.Users.FindAsync(userViewModel.Id) ?? throw new UserOperationException(["User not found"]);

            if (!string.IsNullOrEmpty(userViewModel.Password))
            {
                var removeResult = await _userManager.RemovePasswordAsync(user);

                if (!removeResult.Succeeded)
                {
                    throw new UserOperationException(removeResult.Errors.Select(r => r.Description).ToList());
                }

                var addResult = await _userManager.AddPasswordAsync(user, userViewModel.Password);

                if (!addResult.Succeeded)
                {
                    throw new UserOperationException(addResult.Errors.Select(r => r.Description).ToList());
                }
            }
        }
    }
}
