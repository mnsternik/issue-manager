using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using IssueManager.Data;
using IssueManager.Models;
using IssueManager.Utilities;
using IssueManager.Models.ViewModels.Teams;
using AutoMapper;

namespace IssueManager.Controllers
{
    public class TeamsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper; 

        public TeamsController(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        // GET: Teams
        public async Task<IActionResult> Index(string search, int pageIndex = 1)
        {
            const int pageSize = 10;

            IQueryable<Team> query = _context.Teams;

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(t => t.Name.ToLower().Contains(search));
            }

            IQueryable<TeamsListItemViewModel> mappedQuery = _mapper.ProjectTo<TeamsListItemViewModel>(query);

            var paginatedTeams = await PaginatedList<TeamsListItemViewModel>.CreateAsync(mappedQuery, pageIndex, pageSize);

            var teamsViewModel = new TeamsListViewModel
            {
                Teams = paginatedTeams,
                SearchString = search
            };

            return View(teamsViewModel);
        }

        // GET: Teams/Create
        public IActionResult Create()
        {
            return View();
        }


        // POST: Teams/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name")] CreateTeamViewModel teamViewModel)
        {
            if (ModelState.IsValid)
            {
                if (TeamNameExists(teamViewModel.Name))
                {
                    ModelState.AddModelError("Name", "Team with this name already exists");
                    return View(teamViewModel);
                }

                var team = _mapper.Map<Team>(teamViewModel); 

                _context.Add(team);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(teamViewModel);
        }

        // GET: Teams/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var team = await _context.Teams.FindAsync(id);
            if (team == null)
            {
                return NotFound();
            }
            return View(team);
        }

        // POST: Teams/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name")] Team team)
        {
            if (id != team.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                if (TeamNameExists(team.Name))
                {
                    ModelState.AddModelError("Name", "Team with this name already exists");
                    return View(team);
                }

                try
                {
                    _context.Update(team);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TeamIdExists(team.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(team);
        }

        // GET: Teams/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var team = await _context.Teams
                .FirstOrDefaultAsync(m => m.Id == id);
            if (team == null)
            {
                return NotFound();
            }

            return View(team);
        }

        // POST: Teams/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var team = await _context.Teams.FindAsync(id);
            if (team != null)
            {
                _context.Teams.Remove(team);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool TeamIdExists(int id)
        {
            return _context.Teams.Any(e => e.Id == id);
        }

        private bool TeamNameExists(string name)
        {
            return _context.Teams.Any(e => e.Name == name);
        }
    }
}
