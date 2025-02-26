using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using IssueManager.Models;
using IssueManager.Models.ViewModels.Teams;
using IssueManager.Services.Teams;
using IssueManager.Exceptions;
using Microsoft.AspNetCore.Authorization;

namespace IssueManager.Controllers
{
    public class TeamsController : Controller
    {
        private readonly ITeamService _teamService;

        public TeamsController(ITeamService teamService)
        {
            _teamService = teamService;
        }

        // GET: Teams
        public async Task<IActionResult> Index(string search, int pageIndex = 1)
        {
            var teamsViewModel = await _teamService.GetTeamsAsync(search, pageIndex);
            return View(teamsViewModel);
        }

        // GET: Teams/Create
        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            return View();
        }

        // POST: Teams/Create
        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateTeamViewModel teamViewModel)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    await _teamService.CreateTeamAsync(teamViewModel);
                } 
                catch (NameAlreadyExistsException) 
                {
                    ModelState.AddModelError("Name", "Team with this name already exists");
                    return View(teamViewModel);
                }

                return RedirectToAction(nameof(Index));
            }

            return View(teamViewModel);
        }

        // GET: Teams/Edit/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var team = await _teamService.GetTeamAsync((int)id);

            if (team == null)
            {
                return NotFound();
            }

            return View(team);
        }

        // POST: Teams/Edit/5
        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Team team)
        {
            if (id != team.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    await _teamService.EditTeamAsync(team);
                }
                catch (NameAlreadyExistsException)
                {
                    ModelState.AddModelError("Name", "Team with this name already exists");
                    return View(team);
                }
                catch (DbUpdateConcurrencyException)
                {
                    // TODO 
                }

                return RedirectToAction(nameof(Index));
            }

            return View(team);
        }

        // GET: Teams/Delete/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var team = await _teamService.GetTeamAsync((int)id);

            if (team == null)
            {
                return NotFound();
            }

            return View(team);
        }

        // POST: Teams/Delete/5
        [HttpPost, ActionName("Delete")]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                await _teamService.DeleteTeamAsync(id); 
            }
            catch
            {
                // TODO
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
