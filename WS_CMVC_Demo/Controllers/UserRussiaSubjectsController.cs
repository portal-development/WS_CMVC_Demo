using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WS_CMVC_Demo.Data;
using WS_CMVC_Demo.Models;

namespace WS_CMVC_Demo.Controllers
{
    [Authorize(Roles = "administrator")]
    public class UserRussiaSubjectsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public UserRussiaSubjectsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: UserRussiaSubjects
        public async Task<IActionResult> Index()
        {
            return View(await _context.UserRussiaSubjects.ToListAsync());
        }

        // GET: UserRussiaSubjects/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: UserRussiaSubjects/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Title,Order,Id")] UserRussiaSubject userRussiaSubject)
        {
            if (ModelState.IsValid)
            {
                _context.Add(userRussiaSubject);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(userRussiaSubject);
        }

        // GET: UserRussiaSubjects/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.UserRussiaSubjects == null)
            {
                return NotFound();
            }

            var userRussiaSubject = await _context.UserRussiaSubjects.FindAsync(id);
            if (userRussiaSubject == null)
            {
                return NotFound();
            }
            return View(userRussiaSubject);
        }

        // POST: UserRussiaSubjects/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Title,Order,Id")] UserRussiaSubject userRussiaSubject)
        {
            if (id != userRussiaSubject.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(userRussiaSubject);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!UserRussiaSubjectExists(userRussiaSubject.Id))
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
            return View(userRussiaSubject);
        }

        // GET: UserRussiaSubjects/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.UserRussiaSubjects == null)
            {
                return NotFound();
            }

            var userRussiaSubject = await _context.UserRussiaSubjects
                .FirstOrDefaultAsync(m => m.Id == id);
            if (userRussiaSubject == null)
            {
                return NotFound();
            }

            return View(userRussiaSubject);
        }

        // POST: UserRussiaSubjects/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.UserRussiaSubjects == null)
            {
                return Problem("Entity set 'ApplicationDbContext.UserRussiaSubjects'  is null.");
            }
            var userRussiaSubject = await _context.UserRussiaSubjects.FindAsync(id);
            if (userRussiaSubject != null)
            {
                _context.UserRussiaSubjects.Remove(userRussiaSubject);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool UserRussiaSubjectExists(int id)
        {
            return _context.UserRussiaSubjects.Any(e => e.Id == id);
        }
    }
}
