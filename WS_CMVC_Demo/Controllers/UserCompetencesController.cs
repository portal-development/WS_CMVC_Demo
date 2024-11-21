using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WS_CMVC_Demo.Data;
using WS_CMVC_Demo.Models;

namespace WS_CMVC_Demo.Controllers
{
    [Authorize(Roles = "administrator")]
    public class UserCompetencesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public UserCompetencesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: UserCompetences
        public async Task<IActionResult> Index()
        {
            return View(await _context.UserCompetences.ToListAsync());
        }

        // GET: UserCompetences/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: UserCompetences/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Title,Order,Id")] UserCompetence userCompetence)
        {
            if (ModelState.IsValid)
            {
                _context.Add(userCompetence);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(userCompetence);
        }

        // GET: UserCompetences/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.UserCompetences == null)
            {
                return NotFound();
            }

            var userCompetence = await _context.UserCompetences.FindAsync(id);
            if (userCompetence == null)
            {
                return NotFound();
            }
            return View(userCompetence);
        }

        // POST: UserCompetences/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Title,Order,Id")] UserCompetence userCompetence)
        {
            if (id != userCompetence.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(userCompetence);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!UserCompetenceExists(userCompetence.Id))
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
            return View(userCompetence);
        }

        // GET: UserCompetences/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.UserCompetences == null)
            {
                return NotFound();
            }

            var userCompetence = await _context.UserCompetences
                .FirstOrDefaultAsync(m => m.Id == id);
            if (userCompetence == null)
            {
                return NotFound();
            }

            return View(userCompetence);
        }

        // POST: UserCompetences/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.UserCompetences == null)
            {
                return Problem("Entity set 'ApplicationDbContext.UserCompetences'  is null.");
            }
            var userCompetence = await _context.UserCompetences.FindAsync(id);
            if (userCompetence != null)
            {
                _context.UserCompetences.Remove(userCompetence);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool UserCompetenceExists(int id)
        {
            return _context.UserCompetences.Any(e => e.Id == id);
        }
    }
}
