using IssueManager.Data;
using IssueManager.Models;
using IssueManager.Models.ViewModels.Common;
using Microsoft.AspNetCore.Mvc.Rendering;
using static IssueManager.Models.ViewModels.Common.RequestsSelectListsViewModel;

namespace IssueManager.Services.SelectLists
{
    public class SelectListService : ISelectListService
    {
        private readonly ApplicationDbContext _context;

        public SelectListService(ApplicationDbContext context) 
        {
            _context = context;
        }

        public RequestsSelectListsViewModel PopulateRequestSelectLists(string? selectedUserId = null, int? selectedTeamId = null, int? selectedCategoryId = null)
        {
            var viewModel =  new RequestsSelectListsViewModel
            {
                TeamSelectOptions = new SelectList(_context.Teams, nameof(Team.Id), nameof(Team.Name), selectedTeamId),
                UserSelectOptions = new SelectList(_context.Users, nameof(User.Id), nameof(User.Name), selectedUserId),
                CategorySelectOptions = new SelectList(_context.Categories, nameof(Category.Id), nameof(Category.Name), selectedCategoryId),
                UsersByTeam = _context.Users
                    .GroupBy(u => u.TeamId)
                    .ToDictionary(
                        g => g.Key.ToString(),
                        g => g.Select(u => new UsersByTeamItem { Id = u.Id, Name = u.Name }).ToList()
                    )
            };

            return viewModel; 
        }
    }
}
