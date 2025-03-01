using IssueManager.Models;
using Microsoft.AspNetCore.Identity;

namespace IssueManager.Utilities
{
    public class DataSeeder
    {
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IConfiguration _configuration;
        private readonly ILogger<DataSeeder> _logger;

        public DataSeeder(UserManager<User> userManager, RoleManager<IdentityRole> roleManager, IConfiguration configuration, ILogger<DataSeeder> logger)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task CreateRoles()
        {
            var roles = new[] { "Admin", "User" };

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

            if (email == null)
            {
                _logger.LogError("Address email for admin account not found");
                return;
            }

            if (password == null)
            {
                _logger.LogError("Password for admin account not found");
                return;
            }

            if (await _userManager.FindByEmailAsync(email) == null)
            {
                var user = new User
                {
                    Name = "Admin",
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
                    var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                    _logger.LogError("Creation of admin user failed. Errors: {errors}", errors);
                }   
            }
        }
    }
}
