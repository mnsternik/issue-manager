using IssueManager.Models;
using Microsoft.AspNetCore.Identity;

namespace IssueManager.Utilities
{
    public class DataSeeder
    {
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IConfiguration _configuration;

        public DataSeeder(UserManager<User> userManager, RoleManager<IdentityRole> roleManager, IConfiguration configuration)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _configuration = configuration;
        }

        public async Task CreateRoles()
        {
            var roles = new[] { "Admin", "User", "Guest" };

            foreach (var role in roles)
            {
                if (!await _roleManager.RoleExistsAsync(role))
                {
                    await _roleManager.CreateAsync(new IdentityRole(role));
                }
            }
        }

        public async Task CreateAdminUser()
        {
            var email = _configuration["AdminUser:Email"];
            var password = _configuration["AdminUser:Password"];

            if (email == null || password == null)
            {
                // TODO: Log that no data in secrets
                return;
            }

            if (await _userManager.FindByEmailAsync(email) == null)
            {
                var user = new User
                {
                    UserName = "Admin",
                    Email = email,
                    EmailConfirmed = true,
                    TeamId = 2 // ID of a team "Helpdesk" seeded in ApplicationDbContext 
                };

                var result = await _userManager.CreateAsync(user, password);

                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(user, "Admin");
                }
                else
                {
                    // TODO: Log that failed to create account 
                }   
            }
        }
    }
}
