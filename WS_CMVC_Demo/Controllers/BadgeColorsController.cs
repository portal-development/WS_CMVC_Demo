using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WS_CMVC_Demo.Data;
using WS_CMVC_Demo.Models.Badge;

namespace WS_CMVC_Demo.Controllers
{
    [Authorize(Roles = "administrator")]
    public class BadgeColorsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public BadgeColorsController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            return View(await _context.BadgeColors.ToListAsync());
        }

        public IActionResult Create()
        {
            ViewData["Title"] = "Добавление";
            return View("Edit", new BadgeColor());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ColorGraphHex")] BadgeColor badgeColor)
        {
            if (ModelState.IsValid)
            {
                _context.Add(badgeColor);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["Title"] = "Добавление";
            return View("Edit", badgeColor);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.BadgeColors == null)
            {
                return NotFound();
            }

            var badgeColor = await _context.BadgeColors.FindAsync(id);
            if (badgeColor == null)
            {
                return NotFound();
            }
            ViewData["Title"] = "Редактирование";
            return View(badgeColor);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id)
        {
            var badgeColor = await _context.BadgeColors.FindAsync(id);
            if (badgeColor == null)
            {
                return NotFound();
            }

            await TryUpdateModelAsync(badgeColor, "", bc => bc.ColorGraphHex);

            if (ModelState.IsValid)
            {
                _context.Update(badgeColor);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["Title"] = "Редактирование";
            return View(badgeColor);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.BadgeColors == null)
            {
                return NotFound();
            }

            var badgeColor = await _context.BadgeColors
                .FirstOrDefaultAsync(m => m.Id == id);
            if (badgeColor == null)
            {
                return NotFound();
            }

            return View(badgeColor);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.BadgeColors == null)
            {
                return Problem("Entity set 'ApplicationDbContext.BadgeColors'  is null.");
            }
            var badgeColor = await _context.BadgeColors.FindAsync(id);
            if (badgeColor != null)
            {
                var items = await _context.UserSubcategories.Where(usc => usc.BadgeColor == badgeColor).ToListAsync();
                for (int i = 0; i < items.Count; i++)
                {
                    items[i].BadgeColor = null;
                    _context.Entry(items[i]).State = EntityState.Modified;
                }

                items = await _context.UserSubcategories.Where(usc => usc.BadgeTextColor == badgeColor).ToListAsync();
                for (int i = 0; i < items.Count; i++)
                {
                    items[i].BadgeTextColor = null;
                    _context.Entry(items[i]).State = EntityState.Modified;
                }

                _context.BadgeColors.Remove(badgeColor);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}
