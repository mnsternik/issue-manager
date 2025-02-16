using IssueManager.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore; 
using IssueManager.Data; 

namespace IssueManager.Areas.Identity.Pages.Account.Manage
{
    public class IndexModel : PageModel
    {
        private readonly UserManager<User> _userManager;
        private readonly ApplicationDbContext _context;  

        public IndexModel(
            UserManager<User> userManager,
            ApplicationDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }


        public string FullName { get; set; }
        public string Email { get; set; }
        public string AssignedTeam { get; set; }

        [TempData]
        public string StatusMessage { get; set; }

        private async Task LoadAsync(User user)
        {

            FullName = user.UserName;
            Email = await _userManager.GetEmailAsync(user);


            var loadedUser = await _context.Users
                                    .Include(u => u.Team)
                                    .SingleOrDefaultAsync(u => u.Id == user.Id);

            AssignedTeam = loadedUser?.Team?.Name ?? "Not Assigned";
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            await LoadAsync(user);
            return Page();
        }
    }
}