using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WS_CMVC_Demo.Data;
using WS_CMVC_Demo.Models;
using WS_CMVC_Demo.Models.AccountViewModels;
using WS_CMVC_Demo.Services;

namespace WS_CMVC_Demo.Controllers
{
    [Authorize]
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        //private readonly IEmailSender _emailSender;
        //private readonly ISmsSender _smsSender;
        private readonly ILogger _logger;
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _env;

        public AccountController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            //IEmailSender emailSender,
            //ISmsSender smsSender,
            ILoggerFactory loggerFactory,
            IWebHostEnvironment env)
        {
            _context = context;
            _userManager = userManager;
            _signInManager = signInManager;
            //_emailSender = emailSender;
            //_smsSender = smsSender;
            _logger = loggerFactory.CreateLogger<AccountController>();
            _env = env;
        }

        [AllowAnonymous]
        public ActionResult Login(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Login(LoginViewModel model, string returnUrl)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                var phone = Extensions.ConvertPhoneNumber(model.Email);
                user = await _userManager.Users.FirstOrDefaultAsync(u => u.PhoneNumber == phone);
            }
            if (!await _userManager.CheckPasswordAsync(user, model.Password))
            {
                ModelState.AddModelError(string.Empty, "Неудачная попытка входа.");
                return View(model);
            }
            else
            {
                if (!(user.PhoneNumberConfirmed || user.EmailConfirmed))
                {
                    return Confirmation(user, new { userid = user.Id, returnUrl });
                }
                user.LastSigInDate = DateTime.Now;
                await _userManager.UpdateAsync(user);
                await _signInManager.SignInAsync(user, model.RememberMe);
                _logger.LogInformation(1, "User logged in.");
                return this.RedirectToLocal(returnUrl);
            }
        }

        [AllowAnonymous]
        public async Task<ActionResult> RegisterAsync(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return View("RegisterForbidden");
            }

            var cat = new UserCategory() { IdForRegister = id };
            int catId = cat.Id;
            ViewData["CategoryTitle"] = await _context.UserCategories.Where(c => c.Id == catId).Select(u => u.Title).FirstOrDefaultAsync();
            ViewData["UserSubcategoryId"] = new SelectList(_context.UserSubcategories.Where(uc => uc.CategoryId == catId), "Id", "Title");
            ViewData["CountryId"] = new SelectList(_context.UserCountries.OrderBy(c => c.Order), "Id", "Title");
            ViewData["RussiaSubjectId"] = new SelectList(_context.UserRussiaSubjects.OrderBy(c => c.Order), "Id", "Title");
            ViewData["CompetenceId"] = new SelectList(_context.UserCompetences.OrderBy(c => c.Order), "Id", "Title");
            ViewData["ExcludeProp"] = _context.UserSubcategories.FirstOrDefault(s => s.Category == cat)?.ExcludeProperties?.ToList() ?? new List<string>();
            ViewData["ExcPropArray"] = Newtonsoft.Json.JsonConvert.SerializeObject(await _context.UserSubcategories.Select(sc => new { sc.Id, sc.ExcludeProperties }).ToListAsync());

            return View(new RegisterViewModel());
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Register(RegisterViewModel model, string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return View("RegisterForbidden");
            }

            var cat = new UserCategory() { IdForRegister = id };
            int catId = cat.Id;

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
                    UserCategoryId = catId,
                    UserSubcategoryId = model.UserSubcategoryId,
                    CountryId = model.CountryId,
                    RussiaSubjectId = model.RussiaSubjectId,
                    CompetenceId = model.CompetenceId,
                    CompanyName = model.CompanyName,
                    Agreement = model.Agreement,
                    //обход подтверждений
                    EmailConfirmed = true,
                    PhoneNumberConfirmed = true
                };

                var errors = await user.CheckUserAsync(_context);
                if (!errors.Any())
                {
                    var result = await _userManager.CreateAsync(user, model.Password);
                    if (result.Succeeded)
                    {
                        _logger.LogInformation(3, "User created a new account with password.");
                        await Extensions.AutoPackage(user, _context);

                        //Заглушка на регистрацию без подтверждений
                        user.LastSigInDate = DateTime.Now;
                        await _userManager.UpdateAsync(user);
                        await _signInManager.SignInAsync(user, false);

                        return RedirectToAction("Index", "Home");
                    }
                    this.AddErrors(result);
                }
                this.AddErrors(errors);
            }

            ViewData["CategoryTitle"] = await _context.UserCategories.Where(c => c.Id == catId).Select(u => u.Title).FirstOrDefaultAsync();
            ViewData["UserSubcategoryId"] = new SelectList(_context.UserSubcategories.Where(uc => uc.CategoryId == catId), "Id", "Title", model.UserSubcategoryId);
            ViewData["CountryId"] = new SelectList(_context.UserCountries.OrderBy(c => c.Order), "Id", "Title");
            ViewData["RussiaSubjectId"] = new SelectList(_context.UserRussiaSubjects.OrderBy(c => c.Order), "Id", "Title");
            ViewData["CompetenceId"] = new SelectList(_context.UserCompetences.OrderBy(c => c.Order), "Id", "Title");
            ViewData["ExcludeProp"] = _context.UserSubcategories.FirstOrDefault(s => s.Category == cat)?.ExcludeProperties?.ToList() ?? new List<string>();
            ViewData["ExcPropArray"] = Newtonsoft.Json.JsonConvert.SerializeObject(await _context.UserSubcategories.Select(sc => new { sc.Id, sc.ExcludeProperties }).ToListAsync());

            return View(model);
        }

        [AllowAnonymous]
        public ActionResult ConfirmPhoneNumber()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult ConfirmPhoneNumberAsync(ConfirmPhoneNumberViewModel model, Guid userid, string returnUrl, string act)
        {
            return this.RedirectToLocal(returnUrl);
        }

        [AllowAnonymous]
        public ActionResult ConfirmEmail()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult ConfirmEmailAsync(ConfirmPhoneNumberViewModel model, Guid userid, string returnUrl, string act)
        {
            return this.RedirectToLocal(returnUrl);
        }

        [AllowAnonymous]
        public ActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ForgotPasswordAsync(ForgotPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var phone = Extensions.ConvertPhoneNumber(model.PhoneNumber);

                var user = await _userManager.Users.FirstOrDefaultAsync(u => u.PhoneNumber == phone);
                if (user != null && user.PhoneNumber != null)
                {
                    return Confirmation(user, new { userid = user.Id, act = "pwdreset" });
                }
                return RedirectToAction(nameof(ConfirmPhoneNumber), new { act = "pwdreset" });
            }

            // Появление этого сообщения означает наличие ошибки; повторное отображение формы
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> LogOff(string returnUrl)
        {
            await _signInManager.SignOutAsync();
            _logger.LogInformation(4, "User logged out.");
            return this.RedirectToLocal(returnUrl);
        }

        [Authorize(Roles = "administrator")]
        public async Task<ActionResult> RegisterUrls()
        {
            var categories = await _context.UserCategories.ToListAsync();
            var catWithUrl = categories.Select(c => new { c.Title, url = $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host}/Account/Register?id=" + c.IdForRegister });
            return View(catWithUrl.ToDictionary(c => c.Title, c => c.url));
        }

        public ActionResult AccessDenied()
        {
            return View();
        }

        // GET: DownloadManual
        [AllowAnonymous]
        public FileResult DownloadManual()
        {
            var fileName = "2.jpg";
            string path = Path.Combine(_env.WebRootPath, "files/") + fileName;

            //Read the File data into Byte Array.
            byte[] bytes = System.IO.File.ReadAllBytes(path);

            //Send the File to Download.
            return File(bytes, "application/octet-stream", fileName);
        }

        #region Вспомогательные приложения
        // Используется для защиты от XSRF-атак при добавлении внешних имен входа
        private const string XsrfKey = "demo";

        
        private RedirectToActionResult Confirmation(ApplicationUser user, object routeValues = null)
        {
            return RedirectToAction(nameof(ConfirmPhoneNumber), routeValues);
        }

        #endregion
    }
}