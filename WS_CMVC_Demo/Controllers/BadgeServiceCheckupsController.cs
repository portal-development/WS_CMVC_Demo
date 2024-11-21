using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WS_CMVC_Demo;
using WS_CMVC_Demo.Data;
using WS_CMVC_Demo.Models.Badge;
using WS_CMVC_Demo.Models.Service;
using X.PagedList;

namespace WS_CMVC_Demo.Controllers
{
    [Authorize(Roles = "administrator,moderator,volunteer,accreditator")]
    public class BadgeServiceCheckupsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public BadgeServiceCheckupsController(ApplicationDbContext context)
        {
            _context = context;
        }

        [Authorize(Roles = "administrator,moderator")]
        public async Task<IActionResult> Index(int page = 1)
        {
            var search = new SearchRequestBadgeServiceCheckupViewModel();
            await TryUpdateModelAsync(search);

            var query = _context.BadgeServiceCheckups
                .Include(u => u.BadgeService)
                .Include(u => u.User)
                .Include(u => u.Creator)
                .AsQueryable();

            if (search.BadgeServiceId.HasValue)
            {
                query = query.Where(q => q.BadgeServiceId == search.BadgeServiceId);
            }

            if (search.UserSubcategoryId.Any())
            {
                query = query.Where(q => search.UserSubcategoryId.Contains(q.User.UserSubcategoryId.Value));
            }

            if (search.RussiaSubjectId.Any())
            {
                query = query.Where(q => search.RussiaSubjectId.Contains(q.User.RussiaSubjectId.Value));
            }

            if (search.CountryId.Any())
            {
                if (search.CountryId.Contains(141))
                {
                    query = query.Where(q => q.User.CountryId == null || search.CountryId.Contains(q.User.CountryId.Value));
                }
                else
                {
                    query = query.Where(q => search.CountryId.Contains(q.User.CountryId.Value));
                }
            }

            if (search.CompetenceId.Any())
            {
                query = query.Where(q => search.CompetenceId.Contains(q.User.CompetenceId.Value));
            }

            if (search.Type.HasValue)
            {
                query = query.Where(q => q.Type == search.Type);
            }

            if (!string.IsNullOrEmpty(search.Searchstring))
            {
                var searchstr = search.Searchstring.RemoveWhitespace();
                query = query.Where(q => (q.Creator.SecondName + q.Creator.Name + q.Creator.MiddleName).Contains(searchstr));
            }

            if (search.DateStart.HasValue)
            {
                query = query.Where(q => q.CreateDate > search.DateStart);
            }

            if (search.DateEnd.HasValue)
            {
                query = query.Where(q => q.CreateDate < search.DateEnd);
            }

            query = search.Order switch
            {
                OrderBy.Date => query.OrderBy(u => u.CreateDate),
                OrderBy.DateDesc => query.OrderByDescending(u => u.CreateDate),
                _ => query.OrderByDescending(u => u.CreateDate),
            };

            search.Requests = await query.ToPagedListAsync(page, (int)search.PageSize);

            ViewBag.BadgeServiceId = new SelectList(_context.BadgeServices.OrderBy(x => x.Order), "Id", "Title");
            ViewBag.UserSubcategoryId = new SelectList(_context.UserSubcategories.OrderBy(x => x.Category.Id).ThenBy(x => x.Title).Select(x => new { x.Id, Title = x.Category.Title + " - " + x.Title }), "Id", "Title");
            ViewBag.CountryId = new SelectList(_context.UserCountries.OrderBy(x => x.Order), "Id", "Title");
            ViewBag.RussiaSubjectId = new SelectList(_context.UserRussiaSubjects.OrderBy(x => x.Order), "Id", "Title");
            ViewBag.CompetenceId = new SelectList(_context.UserCompetences.OrderBy(x => x.Order), "Id", "Title");

            return View(search);
        }

        public IActionResult Start()
        {
            return View(PermittableServices.OrderBy(bs => bs.Order));
        }

        public async Task<IActionResult> Create(int Id, int t = 1)
        {
            var bs = await PermittableServices.FirstOrDefaultAsync(bs => bs.Id == Id);

            if (bs == null)
            {
                return Forbid();
            }

            ViewBag.Type = t;
            ViewBag.Id = Id;
            return View(bs);
        }

        [HttpPost]
        public async Task<PartialViewResult> Create(int Id, Guid userId)
        {
            var temp = await PermittableServices.FirstOrDefaultAsync(bs => bs.Id == Id);

            if (temp == null)
            {
                ViewBag.ErrorMessage = "Вам недоступен контроль данного типа. Попробуйде перезайти в систему.";
                return PartialView("_PartialBadgeServiceCheckupError");
            }

            var lastChekUp = await _context.BadgeServiceCheckups
                .Where(s => (byte)s.Type < 64) // только успешные проверки
                .OrderByDescending(s => s.CreateDate)
                .FirstOrDefaultAsync(s => s.BadgeServiceId == Id && s.UserId == userId);

            if (lastChekUp != null && lastChekUp.CreateDate.AddSeconds(5) > DateTime.Now)
            {
                ViewBag.ErrorMessage = "Повторное сканирование. Сканировать один и тот же бейдж можно не чаще одного раза в минуту.";
                return PartialView("_PartialBadgeServiceCheckupError");
            }

            var item = new BadgeServiceCheckup
            {
                BadgeServiceId = Id,
                UserId = userId,
                CreateUserId = User.GetId().Value,
                Type = ServiceCheckupType.Forbid
            };

            var bs = await _context.BadgeServices.FindAsync(Id);

            var permits = await _context.UserPackageServices
            .Include(ups => ups.PackageService.Service.BadgeService)
            .Where(ups => ups.UserId == userId && ups.PackageService.Service.BadgeServiceId == Id)
            .ToListAsync();

            if (!permits.Any() && bs.CheckByPackage)
            {
                if (!await _context.Users.AnyAsync(u => u.Id == userId))
                {
                    ViewBag.ErrorMessage = "Ошибка. Пользователь не найден.";
                    return PartialView("_PartialBadgeServiceCheckupError");
                }
                else if (!await _context.BadgeServices.AnyAsync(s => s.Id == Id))
                {
                    ViewBag.ErrorMessage = "Ошибка. Неизвстный тип контрольной точки.";
                    return PartialView("_PartialBadgeServiceCheckupError");
                }
            }
            else
            {
                // только оплаченные пакеты
                var per = permits.Where(ups => ups.Status == UserPackageServiceStatus.accepted
                                            || ups.Status == UserPackageServiceStatus.completed
                                            || ups.Status == UserPackageServiceStatus.contracted);

                if (!per.Any() && bs.CheckByPackage)
                {
                    item.Type = ServiceCheckupType.ForbidPay;
                }
                else if (!per.Where(ups => ups.StartDate < DateTime.Now && ups.FinishDate > DateTime.Now).Any() && bs.CheckByPackage)
                {
                    item.Type = ServiceCheckupType.ForbidTime;
                }
                else
                {
                    // только доступные на сейчас
                    per = per.Where(ups => ups.StartDate < DateTime.Now && ups.FinishDate > DateTime.Now);

                    item.Type = ServiceCheckupType.Approve;
                    switch (bs.PeriodType)
                    {
                        case BadgeServicePeriodType.Multiple:
                            break;

                        case BadgeServicePeriodType.Single:
                            if (lastChekUp != null)
                            {
                                item.Type = ServiceCheckupType.ForbidSingle;
                            }
                            break;

                        case BadgeServicePeriodType.Periodic:
                            if (lastChekUp != null)
                            {
                                if (lastChekUp.CreateDate.Add(bs.PeriodTime.Value) > DateTime.Now)
                                {
                                    item.Type = ServiceCheckupType.ForbidPeriodic;
                                }
                            }
                            break;

                        default:
                            break;
                    }
                }
            }

            _context.BadgeServiceCheckups.Add(item);
            await _context.SaveChangesAsync();
            await _context.Entry(item).Reference(bs => bs.User).LoadAsync();
            return PartialView("_PartialBadgeServiceCheckup", item);
        }

        [Authorize(Roles = "administrator,moderator")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id, string returnUrl = null)
        {
            if (_context.BadgeServiceCheckups == null)
            {
                return Problem("Entity set 'ApplicationDbContext.BadgeServiceCheckups'  is null.");
            }
            var badgeServiceCheckup = await _context.BadgeServiceCheckups.FindAsync(id);
            if (badgeServiceCheckup != null)
            {
                _context.BadgeServiceCheckups.Remove(badgeServiceCheckup);
            }

            await _context.SaveChangesAsync();
            if (!string.IsNullOrEmpty(returnUrl))
            {
                return Redirect(returnUrl);
            }
            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "administrator,moderator")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(int id, string returnUrl = null)
        {
            if (_context.BadgeServiceCheckups == null)
            {
                return Problem("Entity set 'ApplicationDbContext.BadgeServiceCheckups'  is null.");
            }
            var badgeServiceCheckup = await _context.BadgeServiceCheckups.FindAsync(id);
            if (badgeServiceCheckup != null)
            {
                if (badgeServiceCheckup.Type == ServiceCheckupType.Approve)
                {
                    badgeServiceCheckup.Type = ServiceCheckupType.Canceled;
                    _context.Entry(badgeServiceCheckup).State = EntityState.Modified;
                    await _context.SaveChangesAsync();
                }
            }

            if (!string.IsNullOrEmpty(returnUrl))
            {
                return Redirect(returnUrl);
            }
            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "administrator,moderator")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ReCancel(int id, string returnUrl = null)
        {
            if (_context.BadgeServiceCheckups == null)
            {
                return Problem("Entity set 'ApplicationDbContext.BadgeServiceCheckups'  is null.");
            }
            var badgeServiceCheckup = await _context.BadgeServiceCheckups.FindAsync(id);
            if (badgeServiceCheckup != null)
            {
                if (badgeServiceCheckup.Type == ServiceCheckupType.Canceled)
                {
                    badgeServiceCheckup.Type = ServiceCheckupType.Approve;
                    _context.Entry(badgeServiceCheckup).State = EntityState.Modified;
                    await _context.SaveChangesAsync();
                }
            }

            if (!string.IsNullOrEmpty(returnUrl))
            {
                return Redirect(returnUrl);
            }
            return RedirectToAction(nameof(Index));
        }

        private IQueryable<BadgeService> PermittableServices => (from t1 in _context.UserRoles
                                                                 join t2 in _context.BadgeServiceApplicationRoles on t1.RoleId equals t2.RoleId
                                                                 where t1.UserId == User.GetId()
                                                                 select t2.BadgeService).Distinct();
    }
}
