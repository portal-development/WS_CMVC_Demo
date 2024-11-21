using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System.Data;
using WS_CMVC_Demo;
using WS_CMVC_Demo.Data;
using WS_CMVC_Demo.Models;
using WS_CMVC_Demo.Models.Service;
using WS_CMVC_Demo.Models.UsersViewModels;
using WS_CMVC_Demo.Services;
using X.PagedList;

namespace WS_CMVC_Demo.Controllers
{
    [Authorize(Roles = "administrator")]
    public class UsersController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly IHubContext<EmailSenderHub, ITypedHubClient> _chatHubContext;
        //private readonly IEmailSender _emailSender;
        //private readonly ISmsSender _smsSender;
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private IWebHostEnvironment _environment;
        private readonly ILogger _logger;

        public UsersController(ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            RoleManager<ApplicationRole> roleManager,
            IHubContext<EmailSenderHub, ITypedHubClient> chatHubContext,
            //IEmailSender emailSender,
            //ISmsSender smsSender,
            IServiceScopeFactory serviceScopeFactory,
            IWebHostEnvironment environment,
            ILoggerFactory loggerFactory)
        {
            _context = context;
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _chatHubContext = chatHubContext;
            //_emailSender = emailSender;
            //_smsSender = smsSender;
            _serviceScopeFactory = serviceScopeFactory;
            _environment = environment;
            _logger = loggerFactory.CreateLogger<AccountController>();
        }

        // GET: Users
        public async Task<ActionResult> Index(int page = 1)
        {
            var search = new UserManagerViewModel { Roles = await _context.Roles.ToListAsync() };
            await TryUpdateModelAsync(search);
            IQueryable<ApplicationUser> query = _context.Users.Include(u => u.Roles);

            if (search.UserId.HasValue)
            {
                query = query.Where(u => u.Id == search.UserId.Value);
            }
            else
            {
                if (!string.IsNullOrEmpty(search.Email))
                {
                    query = query.Where(u => u.Email.Contains(search.Email));
                }

                if (!string.IsNullOrEmpty(search.FIO))
                {
                    var searchstr = search.FIO.RemoveWhitespace();
                    query = query.Where(user => (user.SecondName + user.Name + user.MiddleName).Contains(searchstr));
                }

                if (search.RoleId.HasValue)
                {
                    query = query.Where(u => u.Roles.Any(ur => ur.RoleId == search.RoleId));
                }
            }

            search.Users = new PagedList<ApplicationUser>(query, page, (int)search.PageSize);

            return View(search);
        }

        public async Task<ActionResult> UserDetails(string Id)
        {
            var user = await _userManager.FindByIdAsync(Id);

            if (user == null)
                return NotFound();

            return View(user);
        }

        public async Task<ActionResult> EditUser(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return BadRequest();
            }

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            var model = new UserWithRolesViewModel()
            {
                User = new UserForEdit
                {
                    SecondName = user.SecondName,
                    Name = user.Name,
                    MiddleName = user.MiddleName
                },
                AvailableRoles = await GetUserRolesAsync()
            };

            var _userRoles = await _userManager.GetRolesAsync(user);

            foreach (var item in model.AvailableRoles)
            {
                item.Selected = _userRoles.Contains(item.Value);
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> EditUser(string id, UserWithRolesViewModel model)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            model.AvailableRoles = await GetUserRolesAsync();

            foreach (var item in model.AvailableRoles)
            {
                item.Selected = model.SelectedRoles.Contains(item.Value);
            }

            try
            {
                if (ModelState.IsValid)
                {
                    user.SecondName = model.User.SecondName;
                    user.Name = model.User.Name;
                    user.MiddleName = model.User.MiddleName;

                    var _userEditResult = await _userManager.UpdateAsync(user);
                    if (_userEditResult.Succeeded)
                    {
                        var userRoles = await _userManager.GetRolesAsync(user);

                        var includeRoles = model.AvailableRoles
                            .Where(ur => ur.Selected && !userRoles.Contains(ur.Value))
                            .Select(ur => ur.Value);

                        if (includeRoles.Any())
                            await _userManager.AddToRolesAsync(user, includeRoles.ToArray());


                        var excludeRoles = model.AvailableRoles
                            .Where(ur => !ur.Selected && userRoles.Contains(ur.Value))
                            .Select(ur => ur.Value);

                        if (excludeRoles.Any())
                            await _userManager.RemoveFromRolesAsync(user, excludeRoles.ToArray());
                    }
                    else
                    {
                        foreach (var errorText in _userEditResult.Errors)
                        {
                            ModelState.AddModelError(errorText.Code, errorText.Description);
                        }

                        throw new Exception("Изменения не были сохранены.");
                    }
                }

                return RedirectToAction("UserDetails", new { user.Id });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
            }

            return View(model);
        }

        private async Task<IList<SelectListItem>> GetUserRolesAsync()
        {
            var _availableRoles = await _roleManager.Roles
                .OrderBy(role => role.Name)
                .Select(role => new SelectListItem
                {
                    Text = role.Name,
                    Value = role.Name,
                    Selected = false
                })
                .ToListAsync();

            return _availableRoles;
        }

        public ActionResult CreateRole()
        {
            ViewBag.Title = "Добавление роли";
            return View("EditRole");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> CreateRole(string Name, string Description)
        {
            var role = new ApplicationRole() { Name = Name, Description = Description };
            var result = await _roleManager.CreateAsync(role);

            if (result == IdentityResult.Success)
            {
                return RedirectToAction("Index");
            }

            foreach (var item in result.Errors)
            {
                ModelState.AddModelError(item.Code, item.Description);
            }

            return View("EditRole", role);
        }

        public async Task<ActionResult> EditRole(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return BadRequest();
            }

            var role = await _roleManager.FindByIdAsync(id);
            if (role == null)
            {
                return NotFound();
            }

            ViewBag.Title = "Редактирование роли";
            return View(role);
        }

        [HttpPost, ActionName("EditRole")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> EditRolePost(string id)
        {
            var role = await _roleManager.FindByIdAsync(id);

            if (await TryUpdateModelAsync(role, "", r => r.Name, r => r.Description))
            {
                var result = await _roleManager.UpdateAsync(role);
                if (result == IdentityResult.Success)
                {
                    return RedirectToAction("Index");
                }
                foreach (var item in result.Errors)
                {
                    ModelState.AddModelError(item.Code, item.Description);
                }
            }

            ViewBag.Title = "Редактирование роли";
            return View(role);
        }

        [HttpPost]
        public async Task<ActionResult> DeleteRole(string roleId)
        {
            var role = await _roleManager.FindByIdAsync(roleId);

            if (role != null)
            {
                await _roleManager.DeleteAsync(role);
            }

            return RedirectToAction("Index");
        }

        public async Task<ActionResult> LoginAsUser(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return BadRequest();
            }

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            await _signInManager.SignInAsync(user, false);

            return RedirectToAction("Index", "Home");
        }

        public ActionResult ResetPassword(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return BadRequest();
            }
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> ResetPassword(string id, UserManagerResetPasswordViewModel model)
        {
            if (string.IsNullOrEmpty(id))
            {
                return BadRequest();
            }
            var user = await _userManager.FindByIdAsync(id);

            await _userManager.RemovePasswordAsync(user);
            var result = await _userManager.AddPasswordAsync(user, model.Password);

            if (result == IdentityResult.Success)
            {
                return RedirectToAction("ResetPasswordConfirm");
            }
            foreach (var item in result.Errors)
            {
                ModelState.AddModelError(item.Code, item.Description);
            }

            return View(model);
        }

        public ActionResult ResetPasswordConfirm()
        {
            return View();
        }

        [Authorize(Roles = "emailsender")]
        public ActionResult EmailSender()
        {
            return View();
        }

        [HttpPost]
        [Authorize(Roles = "emailsender")]
        public async Task EmailSenderAsync(EmailSenderViewModel model)
        {
            var usersId = model.Users.Split('\n').Select(x => Guid.Parse(x));
            var us = _context.Users.Where(us => usersId.Contains(us.Id) && us.Email != null).ToListAsync();

            List<Task> tasks = new();
            foreach (var user in await us)
            {
                var t = TrySendEmail(user, model.Subject, model.Message);
                tasks.Add(t);
                if (tasks.Count > 19)
                {
                    var finishedTask = await Task.WhenAny(tasks);
                    tasks.Remove(finishedTask);
                }
            }

            await Task.WhenAll(tasks);
        }

        private async Task TrySendEmail(ApplicationUser user, string subject, string message)
        {
            try
            {
                using (var scope = _serviceScopeFactory.CreateScope())
                {
                    var _subcontext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                    //await _emailSender.SendEmailAsync(user, subject, message, _subcontext);
                }

                await _chatHubContext.Clients.All.Sended(user.Id.ToString(), user.Email, false, null);
            }
            catch (Exception e)
            {
                await _chatHubContext.Clients.All.Sended(user.Id.ToString(), user.Email, false, e.Message + e.InnerException != null ? "; " + e.InnerException?.Message : null);
            }
        }

        [Authorize(Roles = "administrator")]
        public IActionResult UploadUsers()
        {
            return View();
        }

        [HttpPost]
        [Authorize(Roles = "administrator")]
        public async Task<IActionResult> UploadUsers(IFormFile postedFile)
        {
            if (postedFile != null)
            {
                string path = Path.Combine(_environment.WebRootPath, "Uploads");
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }

                string fileName = Path.GetFileName(postedFile.FileName);
                string filePath = Path.Combine(path, fileName);
                using (FileStream stream = new FileStream(filePath, FileMode.Create))
                {
                    postedFile.CopyTo(stream);
                }
                string csvData = System.IO.File.ReadAllText(filePath);
                var UploadedUsers = new List<UploadedUser>();

                //Набиваем лист строками csv
                foreach (string row in csvData.Split('\n'))
                {
                    if (!string.IsNullOrEmpty(row))
                    {
                        var rowcells = new List<string>();
                        foreach (string cell in row.Split(';'))
                        {
                            rowcells.Add(cell);
                        }
                        if (rowcells.Count == 5)
                        {
                            var uuser = new UploadedUser
                            {
                                FIO = rowcells[0],
                                Email = rowcells[1],
                                Phone = rowcells[2],
                                Company = rowcells[3],
                                Region = rowcells[4]
                            };
                            UploadedUsers.Add(uuser);
                        }
                    }
                }
                if (UploadedUsers.Count > 0)
                {
                    var model = new List<ApplicationUser>();
                    Random rnd = new Random();
                    var depoid = _context.UserCategories.Where(cat => cat.Title == "Деловая программа").Select(res => res.Id).FirstOrDefault();
                    var subdepoid = _context.UserSubcategories.Where(cat => cat.CategoryId == depoid).Select(res => res.Id).FirstOrDefault();
                    int i = 1;
                    foreach (UploadedUser uuser in UploadedUsers)
                    {
                        //Конструируем пользователя
                        var fios = uuser.FIO.Split(' ');
                        var region = uuser.Region.Trim().Replace("\r", "");
                        var regionid = region.Any() ? _context.UserRussiaSubjects.Where(reg => reg.Title.Contains(region)).Select(res => res.Id).FirstOrDefault() : 0;
                        var registratorid = _context.Users.Where(us => us.Email == "test1@gmail.com").Select(res => res.Id).FirstOrDefault();
                        if (fios.Length > 0)
                        {
                            var user = new ApplicationUser()
                            {
                                UserName = uuser.Email,
                                Email = uuser.Email,
                                SecondName = fios[0],
                                Name = fios.Length > 1 ? fios[1] : null,
                                MiddleName = fios.Length > 2 ? fios[2] : null,
                                PassportNumber = "0000" + rnd.Next(100000, 999999).ToString(),
                                PhoneNumber = uuser.Phone.Any() ? uuser.Phone : "000" + rnd.Next(1000000, 9999999).ToString(),
                                UserCategoryId = depoid == 0 ? null : depoid,
                                UserSubcategoryId = subdepoid == 0 ? null : subdepoid,
                                RegisteredHimself = false,
                                RussiaSubjectId = regionid == 0 ? null : regionid,
                                CompanyName = uuser.Company.Any() ? uuser.Company : null,
                                RegisteredUserId = registratorid,
                                ImportedUser = true
                            };

                            //Проверяем, не грузили ли пользователя в систему. Тогда не импортируем его.
                            //Отдельно от CheckUserAsync чтобы не записывал такую строку в ошибочную
                            var phone = user.PhoneNumber;
                            var exemail = user.Email;
                            if (!_context.Users.Where(us => us.Email == exemail && us.ImportedUser == true).Any())
                            {
                                var errors = await user.CheckUserAsync(_context);
                                if (!errors.Any())
                                {
                                    var result = await _userManager.CreateAsync(user);
                                    if (result.Succeeded)
                                    {
                                        //Автопакет
                                        await AutoPackage(user);
                                        _logger.LogInformation(3, "User created a new account without password.");
                                        model.Add(user);
                                    }
                                    else
                                    {
                                        ViewData["ErrorRows"] += i.ToString() + ",";
                                        this.AddErrors(result);
                                    }
                                }
                                else
                                {
                                    ViewData["ErrorRows"] += i.ToString() + ",";
                                    this.AddErrors(errors);
                                }
                            }
                        }
                        else
                        {
                            ViewData["ErrorRows"] += i.ToString() + ",";
                        }
                        i++;
                    }
                    return View(model);
                }
            }
            return View();
        }

        public class UploadedUser
        {
            public string FIO { get; set; }
            public string Email { get; set; }
            public string Phone { get; set; }
            public string Company { get; set; }
            public string Region { get; set; }
        }

        /// <summary>
        /// Автоматическая бронь первого доступного пакета для данной категории
        /// Пришлось дублировать, с ходу не сообразил как его вызвать
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        [AllowAnonymous]
        public async Task AutoPackage(ApplicationUser user)
        {
            try
            {
                if (user != null)
                {
                    var userid = user.Id;
                    var subcatid = user.UserSubcategoryId;
                    //Проверка, есть ли Автопакет
                    if (await _context.UserSubcategories.Where(us => us.Id == subcatid).Select(res => res.AutoPackage).FirstOrDefaultAsync())
                    {
                        var eventid = 1;
                        if (subcatid != null)
                        {
                            var datenow = DateTime.Now;
                            var package = await _context.UserSubcategoryEventPackages.Where(re => re.UserSubcategoryEvent.UserSubcategoryId == subcatid && re.UserSubcategoryEvent.EventId == eventid && re.UserSubcategoryEvent.Event.DateEnd >= datenow).Select(r => r.Package).FirstOrDefaultAsync();
                            var PackageServices = await _context.PackageServices.Where(ps => ps.Package == package).Include(ps => ps.Service.ServiceType).OrderBy(ps => ps.Service.ServiceType.Id).ToListAsync();
                            foreach (var packageService in PackageServices)
                            {
                                //Проверяем, была ли уже выбрана ранее эта услуга.
                                var alreadyChoosed = await _context.UserPackageServices.Where(ups => ups.UserId == userid && ups.PackageService == packageService).FirstOrDefaultAsync();
                                if (alreadyChoosed == null)
                                {
                                    var StartDate = packageService.StartDate;
                                    var EndDate = packageService.FinishDate;

                                    var ups = new UserPackageService
                                    {
                                        PackageServiceId = packageService.Id,
                                        StartDate = packageService.StartDate,
                                        FinishDate = packageService.FinishDate,
                                        CreateUserId = userid,
                                        EventId = eventid,
                                        UserId = userid,
                                        Status = UserPackageServiceStatus.accepted
                                    };
                                    await _context.UserPackageServices.AddAsync(ups);
                                }
                            }
                            await _context.SaveChangesAsync();
                        }
                    }
                }

            }
            catch { }
        }
    }
}