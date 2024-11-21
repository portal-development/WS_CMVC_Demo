using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WS_CMVC_Demo;
using WS_CMVC_Demo.Data;
using WS_CMVC_Demo.Models;
using WS_CMVC_Demo.Models.AccountViewModels;

namespace WS_CMVC_Demo.Controllers
{
    [Authorize(Roles = "accreditator")]
    public class QuickRegistrationController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger _logger;
        private readonly UserManager<ApplicationUser> _userManager;

        public QuickRegistrationController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, ILoggerFactory loggerFactory)
        {
            _context = context;
            _userManager = userManager;
            _logger = loggerFactory.CreateLogger<HotelsController>();
        }

        // GET: QuickRegistrationController
        public ActionResult Index()
        {
            ViewBag.Category = new SelectList(_context.UserCategories, "Id", "Title");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Index(string categoryddl)
        {
            var categoryid = categoryddl;
            if (categoryid != null)
            {
                return RedirectToAction("Create", new { id = categoryid });
            }
            ViewBag.Category = new SelectList(_context.UserCategories, "Id", "Title");
            return View();
        }

        // GET: QuickRegistrationController/Create/5
        public async Task<IActionResult> Create(int id)
        {
            ViewData["Title"] = "Быстрая регистрация";
            var cat = await _context.UserCategories.Where(c => c.Id == id).FirstOrDefaultAsync();
            ViewData["UserCategory"] = cat.Title;
            ViewData["UserCategoryId"] = id;
            ViewData["UserSubcategoryId"] = new SelectList(_context.UserSubcategories.Where(uc => uc.CategoryId == cat.Id), "Id", "Title");
            ViewData["CountryId"] = new SelectList(_context.UserCountries.OrderBy(c => c.Order), "Id", "Title");
            ViewData["RussiaSubjectId"] = new SelectList(_context.UserRussiaSubjects.OrderBy(c => c.Order), "Id", "Title");
            ViewData["CompetenceId"] = new SelectList(_context.UserCompetences.OrderBy(c => c.Order), "Id", "Title");
            ViewData["ExcludeProp"] = _context.UserSubcategories.FirstOrDefault(s => s.Category == cat)?.ExcludeProperties?.ToList() ?? new List<string>();
            ViewData["ExcPropArray"] = Newtonsoft.Json.JsonConvert.SerializeObject(await _context.UserSubcategories.Select(sc => new { sc.Id, sc.ExcludeProperties }).ToListAsync());
            return View("Edit", new GoldMemberViewModel());
        }

        // POST: QuickRegistrationController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(int id, GoldMemberViewModel model)
        {
            ViewData["Title"] = "Быстрая регистрация";
            var userId = HttpContext.User.GetId();
            var cat = await _context.UserCategories.Where(c => c.Id == id).FirstOrDefaultAsync();
            if (ModelState.IsValid)
            {
                var rnd = new Random();
                var newusername = rnd.Next(100000, 999999);
                //В базе нет ограничений, говорил Максим.
                var newphone = rnd.Next(1000000, 9999999);
                var user = new ApplicationUser()
                {
                    UserName = newusername.ToString() + "@quickregistration.ru",
                    Email = newusername.ToString() + "@quickregistration.ru",
                    SecondName = model.SecondName,
                    Name = model.Name,
                    MiddleName = model.MiddleName,
                    PassportNumber = "-",
                    PhoneNumber = "7000" + newphone.ToString(),
                    UserCategoryId = cat.Id,
                    UserSubcategoryId = model.UserSubcategoryId,
                    RegisteredHimself = false,
                    RegisteredUserId = userId,
                    CountryId = model.CountryId,
                    RussiaSubjectId = model.RussiaSubjectId,
                    CompetenceId = model.CompetenceId,
                    CompanyName = model.CompanyName,
                    Agreement = false
                };

                var errors = await user.CheckQuickUserAsync(_context);
                if (!errors.Any())
                {
                    var result = await _userManager.CreateAsync(user);
                    if (result.Succeeded)
                    {
                        await Extensions.AutoPackage(user, _context);
                        _logger.LogInformation(3, "User created a new quick account without password.");
                        var userid = user.Id;
                        return RedirectToAction("AccrEdit", "Delegation", new { id = userid });
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
            ViewData["UserCategoryId"] = id;
            return View("Edit", model);
        }

    }
}
