using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WS_CMVC_Demo.Data;
using WS_CMVC_Demo.Models;

namespace WS_CMVC_Demo.Controllers
{
    [Authorize(Roles = "administrator")]
    public class UserCountriesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public UserCountriesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: UserCountries
        public async Task<IActionResult> Index()
        {
            return View(await _context.UserCountries.ToListAsync());
        }

        // GET: UserCountries/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: UserCountries/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Title,Order,Id")] UserCountry userCountry)
        {
            if (ModelState.IsValid)
            {
                _context.Add(userCountry);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(userCountry);
        }

        // GET: UserCountries/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.UserCountries == null)
            {
                return NotFound();
            }

            var userCountry = await _context.UserCountries.FindAsync(id);
            if (userCountry == null)
            {
                return NotFound();
            }
            return View(userCountry);
        }

        // POST: UserCountries/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Title,Order,Id")] UserCountry userCountry)
        {
            if (id != userCountry.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(userCountry);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!UserCountryExists(userCountry.Id))
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
            return View(userCountry);
        }

        // GET: UserCountries/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.UserCountries == null)
            {
                return NotFound();
            }

            var userCountry = await _context.UserCountries
                .FirstOrDefaultAsync(m => m.Id == id);
            if (userCountry == null)
            {
                return NotFound();
            }

            return View(userCountry);
        }

        // POST: UserCountries/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.UserCountries == null)
            {
                return Problem("Entity set 'ApplicationDbContext.UserCountries'  is null.");
            }
            var userCountry = await _context.UserCountries.FindAsync(id);
            if (userCountry != null)
            {
                _context.UserCountries.Remove(userCountry);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool UserCountryExists(int id)
        {
            return _context.UserCountries.Any(e => e.Id == id);
        }
    }
}
