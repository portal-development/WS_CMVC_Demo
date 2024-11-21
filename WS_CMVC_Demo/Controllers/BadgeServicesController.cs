using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WS_CMVC_Demo.Data;
using WS_CMVC_Demo.Models.Badge;

namespace WS_CMVC_Demo.Controllers
{
    [Authorize(Roles = "administrator")]
    public class BadgeServicesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public BadgeServicesController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            return View(await _context.BadgeServices
                .Include(bs => bs.Roles)
                .ThenInclude(r => r.Role)
                .OrderBy(bs => bs.Order)
                .ToListAsync());
        }

        public IActionResult Create()
        {
            ViewData["Title"] = "Добавление";
            ViewBag.Roles = new MultiSelectList(_context.Roles, "Id", "Name");
            return View("Edit", new BadgeService());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("IcoUrl,PeriodType,PeriodTime,Title,Order,RecommendedStartTime,RecommendedEndTime,CheckByPackage")] BadgeService badgeService, IEnumerable<Guid> roles)
        {
            if (ModelState.IsValid)
            {
                if (badgeService.PeriodType == BadgeServicePeriodType.Periodic && !badgeService.PeriodTime.HasValue)
                {
                    ModelState.AddModelError(nameof(BadgeService.PeriodTime), "Для периодической услуги необходимо указать период.");
                }
                else
                {
                    badgeService.Roles = new List<BadgeServiceApplicationRole>();
                    foreach (var item in roles)
                    {
                        badgeService.Roles.Add(new BadgeServiceApplicationRole { RoleId = item });
                    }
                    _context.Add(badgeService);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
            }
            ViewData["Title"] = "Добавление";
            ViewBag.Roles = new MultiSelectList(_context.Roles, "Id", "Name");
            return View("Edit", badgeService);
        }

        // GET: BadgeServices/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.BadgeServices == null)
            {
                return NotFound();
            }

            var badgeService = await _context.BadgeServices
                .Include(bs => bs.Roles)
                .ThenInclude(r => r.Role)
                .FirstOrDefaultAsync(bs => bs.Id == id);

            if (badgeService == null)
            {
                return NotFound();
            }
            ViewData["Title"] = "Редактирование";
            ViewBag.Roles = new MultiSelectList(_context.Roles, "Id", "Name", badgeService.Roles.Select(r => r.RoleId));
            return View(badgeService);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, IEnumerable<Guid> roles)
        {
            var badgeService = await _context.BadgeServices.FindAsync(id);
            if (badgeService == null)
            {
                return NotFound();
            }

            await TryUpdateModelAsync(badgeService);

            if (ModelState.IsValid)
            {
                if (badgeService.PeriodType != BadgeServicePeriodType.Periodic)
                {
                    badgeService.PeriodTime = null;
                }

                if (badgeService.PeriodType == BadgeServicePeriodType.Periodic && !badgeService.PeriodTime.HasValue)
                {
                    ModelState.AddModelError(nameof(BadgeService.PeriodTime), "Для периодической услуги необходимо указать период.");
                }
                else
                {
                    await _context.Entry(badgeService)
                     .Collection(b => b.Roles)
                     .LoadAsync();

                    foreach (var item in badgeService.Roles)
                    {
                        if (!roles.Contains(item.RoleId))
                        {
                            _context.Entry(item).State = EntityState.Deleted;
                        }
                    }

                    foreach (var item in roles)
                    {
                        if (!badgeService.Roles.Select(br => br.RoleId).Contains(item))
                        {
                            badgeService.Roles.Add(new BadgeServiceApplicationRole { RoleId = item });
                        }
                    }

                    _context.Update(badgeService);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
            }
            return View(badgeService);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.BadgeServices == null)
            {
                return NotFound();
            }

            var badgeService = await _context.BadgeServices
                .FirstOrDefaultAsync(m => m.Id == id);
            if (badgeService == null)
            {
                return NotFound();
            }

            return View(badgeService);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.BadgeServices == null)
            {
                return Problem("Entity set 'ApplicationDbContext.BadgeServices'  is null.");
            }
            var badgeService = await _context.BadgeServices.FindAsync(id);
            if (badgeService != null)
            {
                _context.BadgeServices.Remove(badgeService);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}
