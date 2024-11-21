using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WS_CMVC_Demo.Data;
using WS_CMVC_Demo.Models;

namespace WS_CMVC_Demo.Controllers
{
    [Authorize(Roles = "administrator")]
    public class UserSubcategoriesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public UserSubcategoriesController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.UserSubcategories
                .Include(u => u.Category)
                .Include(u => u.BadgeColor)
                .Include(u => u.BadgeTextColor)
                .OrderBy(u => u.CategoryId)
                .ThenBy(u => u.Title);
            return View(await applicationDbContext.ToListAsync());
        }

        public IActionResult Create()
        {
            ViewData["Title"] = "Добавление";
            ViewData["CategoryId"] = new SelectList(_context.UserCategories, "Id", "Title");
            var item = new UserSubcategory();
            return View("Edit", item);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("CategoryId,Title,IncludeProperties,NotFreePackage,BadgeColorId,BadgeServiceId,TitleForPrint")] UserSubcategory userSubcategory)
        {
            if (ModelState.IsValid)
            {
                _context.Add(userSubcategory);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["Title"] = "Добавление";
            ViewData["CategoryId"] = new SelectList(_context.UserCategories, "Id", "Title", userSubcategory.CategoryId);
            return View("Edit", userSubcategory);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.UserSubcategories == null)
            {
                return NotFound();
            }

            var userSubcategory = await _context.UserSubcategories.FindAsync(id);
            if (userSubcategory == null)
            {
                return NotFound();
            }

            ViewData["Title"] = "Редактирование";
            ViewData["CategoryId"] = new SelectList(_context.UserCategories, "Id", "Title", userSubcategory.CategoryId);
            ViewData["BadgeColorId"] = new SelectList(_context.BadgeColors, "Id", "ColorGraphHex", userSubcategory.BadgeColorId);

            return View(userSubcategory);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id)
        {
            var item = await _context.UserSubcategories.FindAsync(id);
            if (item == null)
            {
                return NotFound();
            }

            if (await TryUpdateModelAsync(item))
            {
                if (!HttpContext.Request.Form.Any(f => f.Key == nameof(UserSubcategory.IncludeProperties)))
                {
                    item.IncludeProperties = null;
                }
                _context.Update(item);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewData["Title"] = "Редактирование";
            ViewData["CategoryId"] = new SelectList(_context.UserCategories, "Id", "Title", item.CategoryId);

            return View(item);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.UserSubcategories == null)
            {
                return NotFound();
            }

            var userSubcategory = await _context.UserSubcategories
                .Include(u => u.Category)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (userSubcategory == null)
            {
                return NotFound();
            }

            return View(userSubcategory);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.UserSubcategories == null)
            {
                return Problem("Entity set 'ApplicationDbContext.UserSubcategories'  is null.");
            }
            var userSubcategory = await _context.UserSubcategories.FindAsync(id);
            if (userSubcategory != null)
            {
                _context.UserSubcategories.Remove(userSubcategory);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}
