using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WS_CMVC_Demo.Models.UsersViewModels;
using WS_CMVC_Demo;
using WS_CMVC_Demo.Models.Badge;
using WS_CMVC_Demo.Data;
using WS_CMVC_Demo.Models.ManageViewModels;
using WS_CMVC_Demo.Models.Service;
using WS_CMVC_Demo.Models.AccountViewModels;
using WS_CMVC_Demo.Models;

namespace WS_CMVC_Demo.Controllers
{
    [Authorize]
    public class DelegationController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger _logger;
        private IWebHostEnvironment Env { get; }

        public object Environment { get; private set; }

        public DelegationController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, ILoggerFactory loggerFactory, IWebHostEnvironment env)
        {
            _context = context;
            _userManager = userManager;
            _logger = loggerFactory.CreateLogger<AccountController>();
            Env = env;
        }

        // GET: Delegetion
        public async Task<IActionResult> Index()
        {
            //Проверка на аккредитатора
            ViewBag.IsAccreditator = User.IsInRole("accreditator");

            var userId = HttpContext.User.GetId();
            var user = await _context.Users.FindAsync(userId);
            var cat = await _context.Users.Where(u => u.Id == userId).Select(u => u.UserCategory).FirstOrDefaultAsync();
            ViewData["UserCategory"] = cat.Title;
            ViewData["UserId"] = userId;
            var users = _context.Users
                .Where(u => u.RegisteredUserId == userId || u.Id == userId || u.RegisteredUserId == user.RegisteredUserId && user.RegisteredUserId != null || u.Id == user.RegisteredUserId)
                .Include(us => us.PackageServices)
                .Include(u => u.UserSubcategory)
                .AsNoTracking();
            return View(await users.OrderBy(u => u.RegisterDate).ToListAsync());
        }

        // GET: Delegetion/5
        [Authorize(Roles = "accreditator")]
        public async Task<IActionResult> AccrEdit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            //Проверка на аккредитатора
            ViewBag.IsAccreditator = User.IsInRole("accreditator");

            var cat = await _context.Users.Where(u => u.Id == id).Select(u => u.UserCategory).FirstOrDefaultAsync();
            ViewData["UserCategory"] = cat.Title;
            ViewData["UserId"] = id;
            var users = _context.Users
                .Where(u => u.Id == id)
                .Include(us => us.PackageServices)
                .Include(u => u.UserSubcategory)
                .AsNoTracking();
            return View("Index", await users.ToListAsync());
        }

        // GET: Details/5
        [Authorize(Roles = "administrator,contracter,moderator")]
        public async Task<ActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var users = _context.Users.Where(u => u.RegisteredUserId == id || u.Id == id).Include(us => us.PackageServices).Include(u => u.UserSubcategory).Include(u => u.UserCategory);
            ViewData["UserCategory"] = users.First().UserCategory.Title;
            return View(await users.ToListAsync());
        }

        [Authorize(Roles = "administrator,contracter,moderator")]
        public async Task<ActionResult> UserDetails(Guid Id)
        {
            var user = await _context.Users
                        .Include(u => u.UserCategory)
                        .Include(u => u.UserSubcategory)
                        .Include(u => u.Country)
                        .Include(u => u.RussiaSubject)
                        .Include(u => u.Competence)
                        .FirstOrDefaultAsync(u => u.Id == Id);

            if (user == null)
                return NotFound();

            ViewBag.UserCheckups = await _context.BadgeServiceCheckups
                .Include(c => c.BadgeService)
                .Include(c => c.Creator)
                .Where(c => c.UserId == Id)
                .OrderByDescending(c => c.Id)
                .ToListAsync();

            ViewBag.thisUrl = Request.GetEncodedUrl();

            return View(user);
        }

        public async Task<ActionResult> ArrivalEditAsync(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == id);
            if (user == null)
            {
                return NotFound();
            }

            if (!user.CanEdit(HttpContext.User.GetId()))
            {
                return Forbid();
            }

            var model = new UserArrivalViewModel
            {
                ArrivalDateTime = user.ArrivalDateTime,
                ArrivalDetails = user.ArrivalDetails,
                DepartureDateTime = user.DepartureDateTime,
                DepartureDetails = user.DepartureDetails
            };

            ViewData["id"] = id;
            ViewData["UserFIO"] = $"{user.SecondName} {user.Name} {user.MiddleName}";

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ArrivalEdit(Guid id, [Bind] UserArrivalViewModel model)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == id);
            if (user == null)
            {
                return NotFound();
            }

            if (!user.CanEdit(HttpContext.User.GetId()))
            {
                return Forbid();
            }

            if (ModelState.IsValid)
            {
                if (model.ArrivalDateTime >= model.DepartureDateTime)
                {
                    ModelState.AddModelError(nameof(model.DepartureDateTime), "Дата время отъезда должна быть позднее даты времени прибытия");
                }
                else if (await TryUpdateModelAsync(
                user,
                "",
                u => u.ArrivalDateTime,
                u => u.ArrivalDetails,
                u => u.DepartureDateTime,
                u => u.DepartureDetails))
                {
                    var result = await _userManager.UpdateAsync(user);
                    if (result.Succeeded)
                    {
                        return RedirectToAction("Index");
                    }
                    this.AddErrors(result);
                }
            }

            ViewData["id"] = id;
            ViewData["UserFIO"] = $"{user.SecondName} {user.Name} {user.MiddleName}";

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ArrivalDelete(Guid id)
        {
            var user = await _context.Users.FindAsync(id);
            if (!user.CanEdit(HttpContext.User.GetId()))
            {
                return Forbid();
            }
            user.ArrivalDateTime = null;
            user.ArrivalDetails = null;
            user.DepartureDateTime = null;
            user.DepartureDetails = null;
            await _userManager.UpdateAsync(user);
            return RedirectToAction(nameof(Index));
        }

        public async Task<ActionResult> BankDetailsEditAsync(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == id);
            if (user == null)
            {
                return NotFound();
            }

            if (!user.CanEdit(HttpContext.User.GetId()))
            {
                return Forbid();
            }

            var model = new UserBankDetailsViewModel
            {
                IsGeneralBankDetails = user.IsGeneralBankDetails,
                BankDetails = user.BankDetails
            };

            ViewData["id"] = id;
            ViewData["UserFIO"] = $"{user.SecondName} {user.Name} {user.MiddleName}";

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BankDetailsEdit(Guid id, [Bind] UserBankDetailsViewModel model)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == id);
            if (user == null)
            {
                return NotFound();
            }

            if (!user.CanEdit(HttpContext.User.GetId()))
            {
                return Forbid();
            }

            var generalItems = await _context.Users.Where(u => u.RegisteredUserId == id || u.Id == id || u.RegisteredUserId == user.RegisteredUserId && user.RegisteredUserId != null || u.Id == user.RegisteredUserId)
                .Where(u => u.IsGeneralBankDetails && u.Id != id)
                .ToListAsync();

            foreach (var item in generalItems)
            {
                item.IsGeneralBankDetails = false;
                _context.Entry(item).State = EntityState.Modified;
            }

            if (ModelState.IsValid && await TryUpdateModelAsync(
                            user,
                            "",
                            u => u.IsGeneralBankDetails,
                            u => u.BankDetails))
            {
                _context.Entry(user).State = EntityState.Modified;
                await _context.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            ViewData["id"] = id;
            ViewData["UserFIO"] = $"{user.SecondName} {user.Name} {user.MiddleName}";

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BankDetailsDelete(Guid id)
        {
            var user = await _context.Users.FindAsync(id);
            if (!user.CanEdit(HttpContext.User.GetId()))
            {
                return Forbid();
            }
            user.IsGeneralBankDetails = false;
            user.BankDetails = null;
            await _userManager.UpdateAsync(user);
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> CreateAsync()
        {
            ViewData["Title"] = "Добавление члена делегации";

            var userId = HttpContext.User.GetId();
            var cat = await _context.Users.Where(u => u.Id == userId).Select(u => u.UserCategory).FirstOrDefaultAsync();
            ViewData["UserCategory"] = cat.Title;
            ViewData["UserSubcategoryId"] = new SelectList(_context.UserSubcategories.Where(uc => uc.CategoryId == cat.Id), "Id", "Title");
            ViewData["CountryId"] = new SelectList(_context.UserCountries.OrderBy(c => c.Order), "Id", "Title");
            ViewData["RussiaSubjectId"] = new SelectList(_context.UserRussiaSubjects.OrderBy(c => c.Order), "Id", "Title");
            ViewData["CompetenceId"] = new SelectList(_context.UserCompetences.OrderBy(c => c.Order), "Id", "Title");
            ViewData["ExcludeProp"] = _context.UserSubcategories.FirstOrDefault(s => s.Category == cat)?.ExcludeProperties?.ToList() ?? new List<string>();
            ViewData["ExcPropArray"] = Newtonsoft.Json.JsonConvert.SerializeObject(await _context.UserSubcategories.Select(sc => new { sc.Id, sc.ExcludeProperties }).ToListAsync());
            ViewBag.IsAccreditator = false;
            return View("Edit", new MemberViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(MemberViewModel model)
        {
            ViewData["Title"] = "Добавление члена делегации";
            var userId = HttpContext.User.GetId();
            var cat = await _context.Users.Where(u => u.Id == userId).Select(u => u.UserCategory).FirstOrDefaultAsync();
            if (ModelState.IsValid)
            {
                var user = new ApplicationUser()
                {
                    UserName = model.Email,
                    Email = model.Email,
                    SecondName = model.SecondName,
                    Name = model.Name,
                    MiddleName = model.MiddleName,
                    PassportNumber = model.PassportNumber,
                    PhoneNumber = Extensions.ConvertPhoneNumber(model.PhoneNumber),
                    UserCategoryId = cat.Id,
                    UserSubcategoryId = model.UserSubcategoryId,
                    RegisteredHimself = false,
                    RegisteredUserId = userId,
                    CountryId = model.CountryId,
                    RussiaSubjectId = model.RussiaSubjectId,
                    CompetenceId = model.CompetenceId,
                    CompanyName = model.CompanyName,
                    Agreement = model.Agreement
                };

                //добавление существующего пользователя и его делегацию в эту делегацию
                var existsUser = await _context.Users.FirstOrDefaultAsync(u => u.PhoneNumber == user.PhoneNumber && u.PassportNumber == user.PassportNumber);
                if (existsUser != null)
                {
                    existsUser.RegisteredUserId = userId;
                    _context.Entry(existsUser).State = EntityState.Modified;

                    foreach (var item in await _context.Users.Where(u => u.RegisteredUserId == existsUser.Id).ToListAsync())
                    {
                        item.RegisteredUserId = userId;
                        _context.Entry(item).State = EntityState.Modified;
                    }

                    await _context.SaveChangesAsync();
                    return RedirectToAction("Index");
                }

                var errors = await user.CheckUserAsync(_context);
                if (!errors.Any())
                {
                    var result = await _userManager.CreateAsync(user);
                    if (result.Succeeded)
                    {
                        await Extensions.AutoPackage(user, _context);
                        _logger.LogInformation(3, "User created a new account without password.");
                        return RedirectToAction("Index");
                    }
                    this.AddErrors(result);
                }
                this.AddErrors(errors);
            }

            ViewData["UserCategory"] = cat.Title;
            ViewData["UserSubcategoryId"] = new SelectList(_context.UserSubcategories.Where(uc => uc.CategoryId == cat.Id), "Id", "Title", model.UserSubcategoryId);
            ViewData["CountryId"] = new SelectList(_context.UserCountries.OrderBy(c => c.Order), "Id", "Title");
            ViewData["RussiaSubjectId"] = new SelectList(_context.UserRussiaSubjects.OrderBy(c => c.Order), "Id", "Title");
            ViewData["CompetenceId"] = new SelectList(_context.UserCompetences.OrderBy(c => c.Order), "Id", "Title");
            ViewData["ExcludeProp"] = _context.UserSubcategories.Find(model.UserSubcategoryId)?.ExcludeProperties?.ToList() ?? new List<string>();
            ViewData["ExcPropArray"] = Newtonsoft.Json.JsonConvert.SerializeObject(await _context.UserSubcategories.Select(sc => new { sc.Id, sc.ExcludeProperties }).ToListAsync());
            ViewBag.IsAccreditator = false;
            return View("Edit", model);
        }

        public async Task<IActionResult> Edit(Guid? id)
        {
            ViewData["Title"] = "Редактирование члена делегации";
            if (id == null)
            {
                return NotFound();
            }

            var user = await _context.Users.Include(u => u.UserCategory).Include(u => u.UserSubcategory).FirstOrDefaultAsync(u => u.Id == id);
            if (user == null)
            {
                return NotFound();
            }
            //Проверка на аккредитатора
            var isaccreditator = User.IsInRole("accreditator");

            if (!user.CanEdit(HttpContext.User.GetId()) && !isaccreditator)
            {
                return Forbid();
            }

            var model = new MemberViewModel
            {
                SecondName = user.SecondName,
                Name = user.Name,
                MiddleName = user.MiddleName,
                PassportNumber = user.PassportNumber,
                PhoneNumber = user.PhoneNumber,
                Email = user.Email,
                UserSubcategoryId = user.UserSubcategoryId ?? 0,
                CountryId = user.CountryId,
                RussiaSubjectId = user.RussiaSubjectId,
                CompetenceId = user.CompetenceId,
                CompanyName = user.CompanyName,
                Agreement = user.Agreement
            };

            ViewData["UserCategory"] = user.UserCategory.Title;
            ViewData["UserSubcategoryId"] = new SelectList(_context.UserSubcategories.Where(uc => uc.CategoryId == user.UserCategoryId), "Id", "Title", user.UserSubcategoryId);
            ViewData["CountryId"] = new SelectList(_context.UserCountries.OrderBy(c => c.Order), "Id", "Title");
            ViewData["RussiaSubjectId"] = new SelectList(_context.UserRussiaSubjects.OrderBy(c => c.Order), "Id", "Title");
            ViewData["CompetenceId"] = new SelectList(_context.UserCompetences.OrderBy(c => c.Order), "Id", "Title");
            ViewData["ExcludeProp"] = user.UserSubcategory?.ExcludeProperties?.ToList() ?? new List<string>();
            ViewData["ExcPropArray"] = Newtonsoft.Json.JsonConvert.SerializeObject(await _context.UserSubcategories.Select(sc => new { sc.Id, sc.ExcludeProperties }).ToListAsync());
            ViewData["UserId"] = user.Id;
            ViewBag.IsAccreditator = isaccreditator;
            return View("Edit", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind("UserSubcategoryId,CountryId,RussiaSubjectId,CompetenceId,CompanyName,Email,PhoneNumber,SecondName,Name,MiddleName,PassportNumber,Agreement")] MemberViewModel model)
        {
            ViewData["Title"] = "Редактирование члена делегации";

            var user = await _context.Users.Include(u => u.UserCategory).Include(u => u.UserSubcategory).FirstOrDefaultAsync(u => u.Id == id);
            if (user == null)
            {
                return NotFound();
            }
            //Проверка на аккредитатора
            var isaccreditator = User.IsInRole("accreditator");

            if (!user.CanEdit(HttpContext.User.GetId()) && !isaccreditator)
            {
                return Forbid();
            }

            if (await TryUpdateModelAsync(
                user,
                "",
                u => u.SecondName,
                u => u.Name,
                u => u.MiddleName,
                u => u.PassportNumber,
                u => u.UserSubcategoryId,
                u => u.CountryId,
                u => u.RussiaSubjectId,
                u => u.CompetenceId,
                u => u.CompanyName,
                u => u.Agreement))
            {
                using var tran = _context.Database.BeginTransaction();
                var errors = await user.CheckUserAsync(_context, user.Id);
                if (!errors.Any())
                {
                    var result = await _userManager.UpdateAsync(user);
                    if (result.Succeeded && user.Email != model.Email)
                    {
                        result = await _userManager.SetEmailAsync(user, model.Email);
                        if (result.Succeeded)
                        {
                            result = await _userManager.SetUserNameAsync(user, model.Email);
                        }
                    }

                    var phone = Extensions.ConvertPhoneNumber(model.PhoneNumber);
                    if (result.Succeeded && user.PhoneNumber != phone)
                    {
                        if (await _context.Users.AnyAsync(u => u.PhoneNumber == phone))
                        {
                            errors.Add(nameof(model.PhoneNumber), "В системе уже зарегистрирован пользователь с таким телефонным номером.");
                        }
                        else
                        {
                            result = await _userManager.SetPhoneNumberAsync(user, phone);
                        }
                    }

                    if (result.Succeeded && !errors.Any())
                    {
                        await tran.CommitAsync();
                        //Аккредитатор выходит на свою страницу
                        return isaccreditator ? RedirectToAction("AccrEdit", new { id }) : RedirectToAction("Index");
                    }

                    this.AddErrors(result);
                }
                this.AddErrors(errors);
                await tran.RollbackAsync();
            }

            ViewData["UserCategory"] = user.UserCategory.Title;
            ViewData["UserSubcategoryId"] = new SelectList(_context.UserSubcategories.Where(uc => uc.CategoryId == user.UserCategoryId), "Id", "Title", model.UserSubcategoryId);
            ViewData["CountryId"] = new SelectList(_context.UserCountries.OrderBy(c => c.Order), "Id", "Title");
            ViewData["RussiaSubjectId"] = new SelectList(_context.UserRussiaSubjects.OrderBy(c => c.Order), "Id", "Title");
            ViewData["CompetenceId"] = new SelectList(_context.UserCompetences.OrderBy(c => c.Order), "Id", "Title");
            ViewData["ExcludeProp"] = user.UserSubcategory?.ExcludeProperties?.ToList() ?? new List<string>();
            ViewData["ExcPropArray"] = Newtonsoft.Json.JsonConvert.SerializeObject(await _context.UserSubcategories.Select(sc => new { sc.Id, sc.ExcludeProperties }).ToListAsync());
            ViewData["UserId"] = user.Id;
            ViewBag.IsAccreditator = isaccreditator;

            return View(model);
        }

        // GET: QuickRegistrationController
        [Authorize(Roles = "administrator,accreditator")]
        public ActionResult UserCategoryEdit(Guid? id)
        {
            ViewBag.Category = new SelectList(_context.UserCategories, "Id", "Title");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "administrator,accreditator")]
        public async Task<IActionResult> UserCategoryEdit(Guid? id, string categoryddl)
        {
            int categoryid;
            if (int.TryParse(categoryddl, out categoryid))
            {
                var cat = await _context.UserCategories.FirstOrDefaultAsync(c => c.Id == categoryid);
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == id);
                if (user != null)
                {
                    user.UserCategoryId = categoryid;
                    var result = await _userManager.UpdateAsync(user);
                    if (result.Succeeded)
                    {
                        return RedirectToAction("Edit", new { id });
                    }
                }

            }
            ViewBag.Category = new SelectList(_context.UserCategories, "Id", "Title");
            return View();
        }

        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _context.Users
                .FirstOrDefaultAsync(m => m.Id == id);
            if (user == null)
            {
                return NotFound();
            }

            if (!user.CanDelete(HttpContext.User.GetId()))
            {
                return Forbid();
            }

            return View(user);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var user = await _context.Users.FindAsync(id);
            if (!user.CanDelete(HttpContext.User.GetId()))
            {
                return Forbid();
            }
            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // GET: DownloadManual
        public FileResult DownloadManual()
        {
            return null;
        }

        [Authorize(Roles = "administrator,contracter,moderator")]
        public async Task<IActionResult> UserBadge(Guid? id, string t = "0")
        {
            IQueryable<ApplicationUser> users = _context.Users;
            ViewBag.SingleBadge = false;

            if (id.HasValue)
            {
                users = users.Where(u => u.Id == id);
            }
            else if (t == "1")
            {
                users = _context.UserSubcategories.Select(usc => _context.Users
                .Where(u => u.UserSubcategoryId == usc.Id)
                .Where(u => u.PackageServices.Any(cs => cs.Status == UserPackageServiceStatus.accepted || cs.Status == UserPackageServiceStatus.contracted || cs.Status == UserPackageServiceStatus.completed))
                .First())
                    .Where(u => u != null);

            }
            else if (t == "2")
            {
                var r = await _context.UserSubcategories.Select(usc => new BadgeViewModel
                {
                    BadgeColor = usc.BadgeColor.ColorGraphHex,
                    BadgeTextColor = usc.BadgeTextColor.ColorGraphHex,
                    Subcategory = usc.TitleForPrint == null ? usc.Title.ToUpper() : usc.TitleForPrint
                })
                    .Distinct()
                    .ToListAsync();
                ViewBag.SingleBadge = true;
                return View(r.OrderBy(x => x.BadgeColor)
                    .ThenBy(x => x.Subcategory).ToList());
            }
            else
            {
                users = users
                    .Where(u => !u.BadgeServiceCheckups.Any(sc => sc.BadgeServiceId == 9 && sc.Type == ServiceCheckupType.Approve))
                    .Where(u => u.PackageServices.Any(cs => cs.Status == UserPackageServiceStatus.accepted || cs.Status == UserPackageServiceStatus.contracted || cs.Status == UserPackageServiceStatus.completed));
            }

            var res = await users
                .OrderBy(u => u.UserCategory.Title)
                .ThenBy(u => u.Country.Title)
                .ThenBy(u => u.RussiaSubject.Title)
                .ThenBy(u => u.UserSubcategory.Title)
                .ThenBy(u => u.SecondName)
                .Select(u => new BadgeViewModel
                {
                    Id = u.Id,
                    BadgeColor = u.UserSubcategory.BadgeColor.ColorGraphHex,
                    BadgeTextColor = u.UserSubcategory.BadgeTextColor.ColorGraphHex,
                    SecondName = u.SecondName,
                    Name = u.Name,
                    MiddleName = u.MiddleName,
                    RussiaSubject = u.RussiaSubject.Title,
                    Country = u.Country.Title,
                    Competence = u.Competence.Title,
                    Subcategory = u.UserSubcategory.TitleForPrint == null ? u.UserSubcategory.Title.ToUpper() : u.UserSubcategory.TitleForPrint,
                    Pictograms = u.PackageServices
                    .Where(cs => cs.Status == UserPackageServiceStatus.accepted || cs.Status == UserPackageServiceStatus.contracted || cs.Status == UserPackageServiceStatus.completed)
                    .Select(ps => ps.PackageService.Service.BadgeService)
                    .Where(ico => ico != null)
                    .Select(ico => new Pictogram { IcoUrl = ico.IcoUrl, Order = ico.Order })
                    .Distinct()

                })
                .ToListAsync();

            return View(res);
        }

        /// <summary>
        /// Записывает чекапы для напечатанных бэйджей
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        [Authorize(Roles = "administrator,contracter,moderator")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequestFormLimits(ValueCountLimit = int.MaxValue)]
        public async Task<IActionResult> UserBadgeAsync(IEnumerable<Guid> ids)
        {
            var users = await _context.Users
                .Where(u => ids.Contains(u.Id))
                .ToListAsync();

            if (users.Count == 0)
            {
                return NotFound();
            }

            var date = DateTime.Now;
            _context.BadgeServiceCheckups.AddRange(users.Select(u => new BadgeServiceCheckup
            {
                User = u,
                BadgeServiceId = 9,
                Type = ServiceCheckupType.Approve,
                CreateUserId = User.GetId().Value,
                CreateDate = date
            }));

            await _context.SaveChangesAsync();

            if (users.Count == 1)
            {
                return RedirectToAction(nameof(UserDetails), new { id = users[0].Id });
            }

            return RedirectToAction("RequestList", "UserPackages", new { eventid = 1 });
        }
    }
}


