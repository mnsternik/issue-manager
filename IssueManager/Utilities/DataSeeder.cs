using IssueManager.Data;
using IssueManager.Models;
using Microsoft.AspNetCore.Identity;
using System.Data;

namespace IssueManager.Utilities
{
    public class DataSeeder
    {
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly ILogger<DataSeeder> _logger;

        public DataSeeder(
            UserManager<User> userManager,
            RoleManager<IdentityRole> roleManager,
            ApplicationDbContext context,
            IConfiguration configuration, 
            ILogger<DataSeeder> logger)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _context = context;
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

        // If there is no team defined in db, call CreateAndReturnDefaultTeam to create a default team and assign admin account to it 
        public async Task CreateAdminUser()
        {
            var email = _configuration["AdminUser:Email"];
            var password = _configuration["AdminUser:Password"];
            var team = _context.Teams.FirstOrDefault();

            if (team == null)
            {
                team = await CreateAndReturnDefaultTeam();
            }

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
                    UserName = email,
                    Email = email,
                    EmailConfirmed = true,
                    TeamId = team.Id
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

        private async Task<Team> CreateAndReturnDefaultTeam()
        {
            var defaultTeam = new Team { Name = "Default" };
            try
            {
                _context.Add(defaultTeam);
                await _context.SaveChangesAsync();
                return defaultTeam;
            }
            catch (Exception ex)
            {
                _logger.LogError("Creation of default team failed: Error: {ex}", ex.Message);
                throw; 
            }
        }
    }
}
