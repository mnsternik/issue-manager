using AutoMapper;
using IssueManager.Data;
using IssueManager.Exceptions;
using IssueManager.Models;
using IssueManager.Models.ViewModels.Users;
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
        private readonly ILogger<UserService> _logger;

        public UserService(ApplicationDbContext context, UserManager<User> userManager, IMapper mapper, ILogger<UserService> logger)
        {
            _context = context;
            _userManager = userManager; 
            _mapper = mapper;
            _logger = logger; 
        }

        public async Task<UsersListViewModel> GetUsersAsync(string search, int pageIndex)
        {
            _logger.LogInformation("Retrieving users with search: {Search}, page index: {PageIndex}", search, pageIndex);

            try
            {
                IQueryable<User> query = _context.Users;

                // Applying search filters if there are any 
                if (!string.IsNullOrEmpty(search))
                {
                    _logger.LogDebug("Applying user search filter: {Search}", search);
                    query = query.Where(r =>
                        r.Name.ToLower().Contains(search.ToLower()) ||
                        r.Email!.ToLower().Contains(search.ToLower()));
                }

                IQueryable<UsersListItemViewModel> mappedQuery = _mapper.ProjectTo<UsersListItemViewModel>(query);

                var usersListViewModel = new UsersListViewModel
                {
                    Users = await PaginatedList<UsersListItemViewModel>.CreateAsync(mappedQuery, pageIndex),
                    SearchString = search
                };

                _logger.LogInformation("Returned {UserCount} users", usersListViewModel.Users.Count);
                return usersListViewModel;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving users with search: {Search}", search);
                throw;
            }
        }

        public async Task CreateUserAsync(CreateUserViewModel userViewModel)
        {
            _logger.LogInformation("Creating new user with email: {UserEmail}", userViewModel.Email);

            try
            {
                var user = _mapper.Map<User>(userViewModel);
                user.UserName = user.Email;

                // Creating new account and checking for errors
                var createResult = await _userManager.CreateAsync(user, userViewModel.Password);
                if (!createResult.Succeeded)
                {
                    var errors = string.Join(", ", createResult.Errors.Select(e => e.Description));
                    _logger.LogError("User creation failed for {Email}. Errors: {Errors}", userViewModel.Email, errors);
                    throw new UserOperationException(createResult.Errors.Select(e => e.Description).ToList());
                }

                _logger.LogInformation("Successfully created user {UserId}", user.Id);

                // Assigning selected roles to newly created account, and checking for errors
                foreach (var role in userViewModel.RolesList.SelectedRoles)
                {
                    _logger.LogDebug("Adding role {Role} to user {UserId}", role, user.Id);
                    var addRoleResult = await _userManager.AddToRoleAsync(user, role);

                    if (!addRoleResult.Succeeded)
                    {
                        var errors = string.Join(", ", addRoleResult.Errors.Select(e => e.Description));
                        _logger.LogError("Failed to add role {Role} to user {UserId}. Errors: {Errors}", role, user.Id, errors);
                        throw new UserOperationException(addRoleResult.Errors.Select(e => e.Description).ToList());
                    }
                    _logger.LogInformation("Added role {Role} to user {UserId}", role, user.Id);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating user {Email}", userViewModel.Email);
                throw;
            }
        }

        public async Task<ChangeUserDetailsViewModel?> GetChangeUserDetailsAsync(string id)
        {
            _logger.LogInformation("Retrieving user details for {UserId}", id);

            try
            {
                var user = await _context.Users.FindAsync(id);
                if (user == null)
                {
                    _logger.LogWarning("User {UserId} not found for details view", id);
                    return null;
                }

                _logger.LogDebug("Successfully retrieved details for user {UserId}", id);
                return _mapper.Map<ChangeUserDetailsViewModel>(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving details for user {UserId}", id);
                throw;
            }
        }

        public async Task ChangeUserDetailsAsync(ChangeUserDetailsViewModel userViewModel)
        {
            _logger.LogInformation("Updating details for user {UserId}", userViewModel.Id);

            try
            {
                // Retriving user record to edit 
                var user = await _context.Users.FindAsync(userViewModel.Id);
                if (user == null)
                {
                    _logger.LogError("User {UserId} not found for update", userViewModel.Id);
                    throw new UserOperationException(["User not found"]);
                }

                // Retriving team of user record to edit
                var team = await _context.Teams.FindAsync(userViewModel.TeamId);
                if (team == null)
                {
                    _logger.LogWarning("Team {TeamId} not found for user {UserId}", userViewModel.TeamId, userViewModel.Id);
                }

                // Updating user's data
                user.Team = team!;
                user.Email = userViewModel.Email!;
                user.Name = userViewModel.Name!;

                _context.Update(user);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Successfully updated details for user {UserId}", userViewModel.Id);
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogError(ex, "Concurrency error updating user {UserId}", userViewModel.Id);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating details for user {UserId}", userViewModel.Id);
                throw;
            }
        }

        public async Task<ChangeUserRolesViewModel?> GetChangeUserRolesAsync(string id)
        {
            _logger.LogInformation("Retrieving roles for user {UserId}", id);

            try
            {
                var user = await _context.Users.FindAsync(id);
                if (user == null)
                {
                    _logger.LogWarning("User {UserId} not found for roles view", id);
                    return null;
                }

                _logger.LogDebug("Retrieved roles view for user {UserId}", id);
                return _mapper.Map<ChangeUserRolesViewModel>(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving roles for user {UserId}", id);
                throw;
            }
        }

        public async Task ChangeUserRolesAsync(ChangeUserRolesViewModel userViewModel)
        {
            _logger.LogInformation("Updating roles for user {UserId}", userViewModel.Id);

            try
            {
                // Retriving user record to edit 
                var user = await _context.Users.FindAsync(userViewModel.Id);
                if (user == null)
                {
                    _logger.LogError("User {UserId} not found for role update", userViewModel.Id);
                    throw new UserOperationException(["User not found"]);
                }

                // Retriving user's roles 
                var currentRoles = await _userManager.GetRolesAsync(user);
                _logger.LogDebug("Current roles for user {UserId}: {Roles}", user.Id, string.Join(", ", currentRoles));

                // Deleting current roles if there are any
                if (currentRoles.Any())
                {
                    _logger.LogInformation("Removing existing roles from user {UserId}", user.Id);
                    var removeResult = await _userManager.RemoveFromRolesAsync(user, currentRoles);

                    if (!removeResult.Succeeded)
                    {
                        var errors = string.Join(", ", removeResult.Errors.Select(e => e.Description));
                        _logger.LogError("Failed to remove roles from user {UserId}. Errors: {Errors}", user.Id, errors);
                        throw new UserOperationException(removeResult.Errors.Select(r => r.Description).ToList());
                    }
                }

                // Adding new roles if there are some selected
                if (userViewModel.RolesList.SelectedRoles.Count > 0)
                {
                    _logger.LogInformation("Adding new roles to user {UserId}: {Roles}",
                        user.Id, string.Join(", ", userViewModel.RolesList.SelectedRoles));

                    var addResult = await _userManager.AddToRolesAsync(user, userViewModel.RolesList.SelectedRoles);

                    if (!addResult.Succeeded)
                    {
                        var errors = string.Join(", ", addResult.Errors.Select(e => e.Description));
                        _logger.LogError("Failed to add roles to user {UserId}. Errors: {Errors}", user.Id, errors);
                        throw new UserOperationException(addResult.Errors.Select(r => r.Description).ToList());
                    }
                }

                _logger.LogInformation("Successfully updated roles for user {UserId}", user.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating roles for user {UserId}", userViewModel.Id);
                throw;
            }
        }

        public async Task<ChangeUserPasswordViewModel?> GetChangeUserPasswordAsync(string id)
        {
            _logger.LogInformation("Retrieving password change view for user {UserId}", id);

            try
            {
                var user = await _context.Users.FindAsync(id);
                if (user == null)
                {
                    _logger.LogWarning("User {UserId} not found for password change", id);
                    return null;
                }

                _logger.LogDebug("Retrieved password change view for user {UserId}", id);
                return _mapper.Map<ChangeUserPasswordViewModel>(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving password view for user {UserId}", id);
                throw;
            }
        }

        public async Task ChangeUserPasswordAsync(ChangeUserPasswordViewModel userViewModel)
        {
            _logger.LogInformation("Changing password for user {UserId}", userViewModel.Id);

            try
            {
                // Retriving user record to edit 
                var user = await _context.Users.FindAsync(userViewModel.Id);
                if (user == null)
                {
                    _logger.LogError("User {UserId} not found for password change", userViewModel.Id);
                    throw new UserOperationException(["User not found"]);
                }

                // Deleting old password
                if (!string.IsNullOrEmpty(userViewModel.Password))
                {
                    _logger.LogInformation("Removing existing password for user {UserId}", user.Id);
                    var removeResult = await _userManager.RemovePasswordAsync(user);

                    if (!removeResult.Succeeded)
                    {
                        var errors = string.Join(", ", removeResult.Errors.Select(e => e.Description));
                        _logger.LogError("Password removal failed for user {UserId}. Errors: {Errors}", user.Id, errors);
                        throw new UserOperationException(removeResult.Errors.Select(r => r.Description).ToList());
                    }

                    // Adding new password
                    _logger.LogInformation("Adding new password for user {UserId}", user.Id);
                    var addResult = await _userManager.AddPasswordAsync(user, userViewModel.Password);
                    if (!addResult.Succeeded)
                    {
                        var errors = string.Join(", ", addResult.Errors.Select(e => e.Description));
                        _logger.LogError("Password addition failed for user {UserId}. Errors: {Errors}", user.Id, errors);
                        throw new UserOperationException(addResult.Errors.Select(r => r.Description).ToList());
                    }
                }

                _logger.LogInformation("Successfully updated password for user {UserId}", user.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error changing password for user {UserId}", userViewModel.Id);
                throw;
            }
        }
    }
}
