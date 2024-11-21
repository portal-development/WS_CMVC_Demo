using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Text;
using WS_CMVC_Demo;
using WS_CMVC_Demo.Data;
using WS_CMVC_Demo.Models;
using WS_CMVC_Demo.Models.Badge;
using WS_CMVC_Demo.Models.Service;
using WS_CMVC_Demo.Models.UserPackagesViewModels;
using X.PagedList;
using OrderBy = WS_CMVC_Demo.Models.UserPackagesViewModels.OrderBy;
using UserPackageService = WS_CMVC_Demo.Models.UserPackagesViewModels.UserPackageService;

namespace WS_CMVC_Demo.Controllers
{
    [Authorize]
    public class UserPackagesController : Controller
    {

        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger _logger;

        public UserPackagesController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, ILoggerFactory loggerFactory)
        {
            _context = context;
            _userManager = userManager;
            _logger = loggerFactory.CreateLogger<UserPackagesController>();
        }

        // GET: UserPackagesController
        public async Task<ActionResult> Index(Guid? userid)
        {
            if (userid == null)
            {
                return NotFound();
            }
            var user = await _context.Users.FindAsync(userid);
            if (user == null)
            {
                return NotFound();
            }
            if (!await CheckDelegationPermission(userid))
            {
                return Forbid();
            }
            ViewData["userid"] = userid;
            var model = new List<EventsWithRequestsViewModel>();
            //Берем мероприятия которые еще не кончились
            var datenow = DateTime.Now;
            var Events = await _context.Events.Where(ev => ev.DateEnd > datenow).ToListAsync();
            foreach (var Event in Events)
            {
                var requests = _context.UserPackageServices.Where(ups => ups.Event == Event && ups.UserId == userid)?.Include(r => r.PackageService.Package).ToList();
                var subcategoryid = user.UserSubcategoryId;
                //Проверка, есть ли у пользователя ИМЕННО ТАКОЙ EventUser
                var hassub = await _context.EventUsers.Where(eu => eu.UserSubcategoryEvent.Event == Event && eu.User == user && eu.UserSubcategoryEvent.UserSubcategoryId == subcategoryid).AnyAsync();
                var eventwr = new EventsWithRequestsViewModel
                {
                    EventId = Event.Id,
                    EventName = Event.Title,
                    StartDate = Event.DateStart,
                    EndDate = Event.DateEnd,
                    HasRequest = requests.Any(),
                    PackageName = requests.Any() ? requests.Select(re => re.PackageService.Package.Name).FirstOrDefault().ToString() : null,
                    PackageId = requests.Any() ? requests.Select(re => re.PackageService.Package.Id).FirstOrDefault() : 0,
                    Status = requests.Any() ? requests.Select(re => re.Status).First() : UserPackageServiceStatus.draft,
                    HasUserSubCategory = hassub
                };
                model.Add(eventwr);
            }
            return View(model);
        }


        // GET: UserPackagesController/Details/5
        public async Task<ActionResult> Details(int? id, Guid? userid, int? eventid)
        {
            if (id == null || eventid == null || userid == null)
            {
                return NotFound();
            }

            System.Security.Claims.ClaimsPrincipal currentUser = User;
            var isadmin = currentUser.IsInRole("administrator");
            var iscontracter = currentUser.IsInRole("contracter");
            var ismoderator = currentUser.IsInRole("moderator");
            var isaccreditator = currentUser.IsInRole("accreditator");
            ViewBag.IsAdmin = isadmin;
            ViewBag.IsModerator = ismoderator;
            ViewBag.IsContracter = iscontracter;
            ViewBag.IsAccreditator = isaccreditator;

            if (!await CheckDelegationPermission(userid) && !isadmin && !ismoderator && !iscontracter)
            {
                return Forbid();
            }
            ViewBag.UserId = userid;
            ViewBag.eventid = eventid;
            ViewBag.id = id;
            var model = await _context.UserPackageServices.Where(ups => ups.UserId == userid && ups.PackageService.Package.Id == id && ups.Event.Id == eventid).Include(res => res.PackageService.Package).Include(res => res.PackageService.Service.ServiceType).OrderBy(ps => ps.PackageService.Service.ServiceType.Order).ToListAsync();
            var cost = await GetPackageCost(model);
            ViewBag.Cost = Math.Round(cost, 2, MidpointRounding.AwayFromZero);
            return View(model);
        }

        //Высчитываем стоимость пакета
        private async Task<double> GetPackageCost(List<Models.Service.UserPackageService> upses)
        {
            double cost = 0;
            foreach (var ups in upses)
            {
                var startdate = ups.StartDate;
                var enddate = ups.FinishDate;
                var serviceid = ups.PackageService.ServiceId;
                var quota = await _context.Quotas.Where(q => q.ServiceId == serviceid && q.StartDate <= startdate && q.FinishDate >= enddate).FirstOrDefaultAsync();
                if (quota != null)
                {
                    if (quota.FixedCost)
                    {
                        cost += quota.Cost;
                    }
                    else
                    {
                        //x+1, округляем до полных суток
                        var period = (enddate - startdate).Days + 1;
                        cost += quota.Cost * period;
                    }
                }
            }
            //НДС
            cost *= 1.2;
            return cost;
        }

        // GET: UserPackagesController/List/5
        public async Task<IActionResult> List(Guid? userid, int? eventid)
        {
            if (userid == null || eventid == null)
            {
                return NotFound();
            }
            var user = await _context.Users.FindAsync(userid);
            if (user == null)
            {
                return NotFound();
            }
            if (!await CheckDelegationPermission(userid))
            {
                return Forbid();
            }
            var reqid = await HasUserRequest((Guid)userid, (int)eventid);
            if (reqid != 0)
            {
                return RedirectToAction("Details", new { reqid, userid, eventid });
            }

            ViewBag.UserId = userid;
            ViewBag.eventid = eventid;
            //Находим все пакеты, которые доступны пользователю с его подкатегорией и eventid, на мероприятия которые еще не кончились.
            var subcat = await _context.EventUsers.Where(eu => eu.UserSubcategoryEvent.EventId == eventid && eu.UserId == userid)?.Select(eur => eur.UserSubcategoryEvent.UserSubcategoryCategory).FirstOrDefaultAsync();
            var datenow = DateTime.Now;
            var packages = await _context.UserSubcategoryEventPackages.Where(re => re.UserSubcategoryEvent.UserSubcategoryCategory == subcat && re.UserSubcategoryEvent.EventId == eventid && re.UserSubcategoryEvent.Event.DateEnd >= datenow).Select(r => r.Package).OrderBy(r => r.Name).ToListAsync();

            var model = new List<UserPackageViewModel>();

            foreach (var package in packages)
            {
                var pack = await GetFullPackage(package, userid, (int)eventid);
                if (pack != null)
                {
                    model.Add(pack);
                }
            }

            return View(model);
        }

        [Authorize(Roles = "administrator,contracter,moderator")]
        public async Task<ActionResult> RequestListAsync(int? eventid, int page = 1)
        {
            if (eventid == null)
            {
                return NotFound();
            }
            ViewBag.eventid = eventid;

            var search = new SortedRequestViewModel();
            await TryUpdateModelAsync(search);

            var query = _context.UserPackageServices.Where(ups => ups.EventId == eventid);

            if (search.Status.HasValue)
            {
                query = query.Where(ups => ups.Status == search.Status);
            }

            if (!string.IsNullOrEmpty(search.Searchstring))
            {
                var searchstr = search.Searchstring.RemoveWhitespace();
                query = query.Where(ups => (ups.User.SecondName + ups.User.Name + ups.User.MiddleName).Contains(searchstr));
            }

            if (search.UserId.HasValue)
            {
                query = query.Where(ups => ups.UserId == search.UserId);
            }

            var res = query
                .GroupBy(ups => new { ups.UserId, ups.PackageService.PackageId })
                .Select(gr => gr.First().Id)
                .Join(_context.UserPackageServices, t1 => t1, ups => ups.Id, (t1, ups) => new RequestViewModel
                {
                    FIO = ups.User.SecondName + " " + ups.User.Name + " " + ups.User.MiddleName,
                    DelegationFIO = ups.User.RegisteredUser.SecondName + " " + ups.User.RegisteredUser.Name + " " + ups.User.RegisteredUser.MiddleName,
                    PackageName = ups.PackageService.Package.Name,
                    PackageId = ups.PackageService.PackageId,
                    CreateDate = ups.CreateDate,
                    Status = ups.Status,
                    UserId = ups.UserId,
                    DelegationUserId = ups.User.RegisteredUserId,
                    RightPackage = _context.UserSubcategoryEventPackages.Where(r => r.PackageId == ups.PackageService.PackageId).Select(r => r.UserSubcategoryEvent.UserSubcategoryCategory).Contains(ups.User.UserSubcategory) ? true : false
                });

            //Для аккредитатора добавляем в результат пользователей без пакетов
            var isaccreditator = User.IsInRole("accreditator");
            ViewBag.IsAccreditator = isaccreditator;

            if (!res.Any() && !string.IsNullOrEmpty(search.Searchstring) && isaccreditator)
            {
                var searstr = search.Searchstring.RemoveWhitespace();
                res = _context.Users.Where(ups => (ups.SecondName + ups.Name + ups.MiddleName).Contains(searstr)).Select(ups => new RequestViewModel
                {
                    FIO = ups.SecondName + " " + ups.Name + " " + ups.MiddleName,
                    DelegationFIO = ups.RegisteredUser.SecondName + " " + ups.RegisteredUser.Name + " " + ups.RegisteredUser.MiddleName,
                    PackageName = "Нет брони",
                    PackageId = 0,
                    CreateDate = DateTime.Now,
                    Status = UserPackageServiceStatus.draft,
                    UserId = ups.Id,
                    DelegationUserId = ups.RegisteredUserId,
                    RightPackage = true
                });
            }

            var result = search.Order switch
            {
                Models.UserPackagesViewModels.OrderBy.Date => res.OrderBy(u => u.CreateDate),
                OrderBy.DateDesc => res.OrderByDescending(u => u.CreateDate),
                OrderBy.Delagate => res.OrderBy(u => u.CreateDate),
                _ => res.OrderBy(u => u.DelegationFIO)
            };

            search.Requests = new PagedList<RequestViewModel>(result, page, (int)search.PageSize);

            return View(search);
        }

        // GET: UserPackagesController/Edit/5
        public async Task<ActionResult> Edit(int? id, Guid? userid, int? eventid)
        {
            if (id == null || eventid == null || userid == null)
            {
                return NotFound();
            }
            System.Security.Claims.ClaimsPrincipal currentUser = User;
            var isadmin = currentUser.IsInRole("administrator");

            if ((!await CheckDelegationPermission(userid) || !await CheckPackagePermission((int)eventid, (Guid)userid, (int)id)) && !isadmin)
            {
                return Forbid();
            }
            ViewBag.UserId = userid;
            ViewBag.eventid = eventid;
            var package = await _context.Packages.Where(pa => pa.Id == id).FirstOrDefaultAsync();

            if (package != null)
            {
                var model = await GetFullPackage(package, userid, (int)eventid);
                return View(model);
            }
            else
            {
                return NotFound();
            }
        }

        // POST: UserPackagesController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, int eventid, Guid userid, UserPackageViewModel model)
        {
            System.Security.Claims.ClaimsPrincipal currentUser = User;
            var isadmin = currentUser.IsInRole("administrator");

            if ((!await CheckDelegationPermission(userid) || !await CheckPackagePermission(eventid, userid, id)) && !isadmin)
            {
                return Forbid();
            }
            var user = await _context.Users.FindAsync(userid);
            if (ModelState.IsValid)
            {
                try
                {
                    //Бронировал ли пользователь вообще услуги на данное мероприятие
                    var bookedpacks = await _context.UserPackageServices.Where(ups => ups.UserId == userid && ups.Event.Id == eventid).ToListAsync();
                    if (bookedpacks.Any())
                    {
                        //Есть ли среди услуг поданные или утвержденные заявки
                        if (bookedpacks.Where(bp => bp.Status == UserPackageServiceStatus.book || bp.Status == UserPackageServiceStatus.accepted || bp.Status == UserPackageServiceStatus.contracted || bp.Status == UserPackageServiceStatus.completed).Any())
                        {
                            ViewBag.StatusMessage = "Вы не можете поменять условия брони, так как заявка уже отправлена.";
                        }
                        //Только черновики или отклоненные
                        else
                        {
                            if (await CheckBooking(id, eventid, user, model))
                            {
                                foreach (var bookedpack in bookedpacks)
                                {
                                    _context.UserPackageServices.Remove(bookedpack);
                                }
                                await _context.SaveChangesAsync();
                                await BookPackage(id, eventid, user, model);
                                return RedirectToAction("Details", new { id, eventid, userid });
                            }
                            return View(model);
                        }
                    }
                    //Пользователь бронирует в первый раз
                    else
                    {
                        if (await CheckBooking(id, eventid, user, model))
                        {
                            await BookPackage(id, eventid, user, model);
                            return RedirectToAction("Details", new { id, eventid, userid });
                        }
                        else
                        {
                            return View(model);
                        }
                    }
                }
                catch
                {
                    return View(model);
                }
            }
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Delete(int? id, Guid? userid, int? eventid)
        {
            if (id == null || eventid == null || userid == null)
            {
                return NotFound();
            }

            if (!await CheckDelegationPermission(userid))
            {
                return Forbid();
            }
            var bookedpacks = await _context.UserPackageServices.Where(ups => ups.UserId == userid && ups.Event.Id == eventid).ToListAsync();
            foreach (var bookedpack in bookedpacks)
            {
                _context.UserPackageServices.Remove(bookedpack);
            }

            //За одно удаляем подкатегорию пользователя на случай если она первоначально была присвоена неверно
            //Такой ход вместо точки входа в EditSub, некуда ее приткнуть
            var eventuser = await _context.EventUsers.Where(eu => eu.UserId == userid && eu.UserSubcategoryEvent.EventId == eventid).FirstOrDefaultAsync();
            _context.EventUsers.Remove(eventuser);

            await _context.SaveChangesAsync();
            return RedirectToAction("Index", new { id, eventid, userid });
        }

        // GET: UserPackagesController/Book/5
        public async Task<ActionResult> Book(int? id, Guid? userid, int? eventid)
        {
            if (id == null || eventid == null || userid == null)
            {
                return NotFound();
            }

            if (!await CheckDelegationPermission(userid) || !await CheckPackagePermission((int)eventid, (Guid)userid, (int)id))
            {
                return Forbid();
            }
            var bookedpacks = await _context.UserPackageServices.Where(ups => ups.UserId == userid && ups.Event.Id == eventid && ups.PackageService.Package.Id == id).ToListAsync();
            foreach (var bookedpack in bookedpacks)
            {
                bookedpack.Status = UserPackageServiceStatus.book;
            }
            await _context.SaveChangesAsync();
            return RedirectToAction("Details", new { id, eventid, userid });
        }

        [Authorize(Roles = "administrator,moderator")]
        // GET: UserPackagesController/Accept/5
        public async Task<ActionResult> Accept(int? id, Guid? userid, int? eventid)
        {
            if (id == null || eventid == null || userid == null)
            {
                return NotFound();
            }
            var bookedpacks = await _context.UserPackageServices.Where(ups => ups.UserId == userid && ups.Event.Id == eventid && ups.PackageService.Package.Id == id).ToListAsync();
            foreach (var bookedpack in bookedpacks)
            {
                bookedpack.Status = UserPackageServiceStatus.accepted;
            }
            await _context.SaveChangesAsync();
            return RedirectToAction("Details", new { id, eventid, userid });
        }

        [Authorize(Roles = "administrator,moderator")]
        // GET: UserPackagesController/Decline/5
        public async Task<ActionResult> Decline(int? id, Guid? userid, int? eventid)
        {
            if (id == null || eventid == null || userid == null)
            {
                return NotFound();
            }
            var bookedpacks = await _context.UserPackageServices.Where(ups => ups.UserId == userid && ups.Event.Id == eventid && ups.PackageService.Package.Id == id).ToListAsync();
            foreach (var bookedpack in bookedpacks)
            {
                bookedpack.Status = UserPackageServiceStatus.declined;
            }
            await _context.SaveChangesAsync();
            return RedirectToAction("Details", new { id, eventid, userid });
        }

        [Authorize(Roles = "administrator,contracter")]
        // GET: UserPackagesController/Contract/5
        public async Task<ActionResult> Contract(int? id, Guid? userid, int? eventid)
        {
            if (id == null || eventid == null || userid == null)
            {
                return NotFound();
            }
            var bookedpacks = await _context.UserPackageServices.Where(ups => ups.UserId == userid && ups.Event.Id == eventid && ups.PackageService.Package.Id == id).ToListAsync();
            foreach (var bookedpack in bookedpacks)
            {
                bookedpack.Status = UserPackageServiceStatus.contracted;
            }
            await _context.SaveChangesAsync();
            return RedirectToAction("Details", new { id, eventid, userid });
        }

        [Authorize(Roles = "administrator,contracter")]
        // GET: UserPackagesController/Complete/5
        public async Task<ActionResult> Complete(int? id, Guid? userid, int? eventid)
        {
            if (id == null || eventid == null || userid == null)
            {
                return NotFound();
            }
            var bookedpacks = await _context.UserPackageServices.Where(ups => ups.UserId == userid && ups.Event.Id == eventid && ups.PackageService.Package.Id == id).ToListAsync();
            foreach (var bookedpack in bookedpacks)
            {
                bookedpack.Status = UserPackageServiceStatus.completed;
            }
            await _context.SaveChangesAsync();
            return RedirectToAction("Details", new { id, eventid, userid });
        }

        // GET: UserPackagesController/EditSub/5
        public async Task<ActionResult> EditSub(int? id, int? eventid, Guid? userid)
        {
            if (eventid == null || userid == null)
            {
                return NotFound();
            }

            if (!await CheckDelegationPermission(userid))
            {
                return Forbid();
            }
            eventid = (int)eventid;
            ViewBag.UserId = userid;
            ViewBag.eventid = eventid;

            //Категория пользователя, который бронирует пакет (за себя или члена делегации)
            //Отсеивает подкатегории, которые в других категориях
            var thisuserid = HttpContext.User.GetId();
            var categoryid = await _context.Users.Where(us => us.Id == thisuserid).Select(res => res.UserCategoryId).FirstOrDefaultAsync();

            //Все подкатегории
            var subcat = await _context.UserSubcategoryEvents.Where(use => use.Event.Id == eventid && use.UserSubcategoryCategory.Category.Id == categoryid).Select(res => new { res.Id, res.UserSubcategoryCategory.Title }).ToListAsync();

            //Подкатегория, которая уже есть у участника
            //var usercategory = await _context.EventUsers.Where(eu => eu.UserId == userid).ToListAsync();
            //var hasusercategory = usercategory.Any();
            //var usercatid = usercategory?.Select(res => res.UserSubcategoryEventId).FirstOrDefault();
            var listsubcat = new List<SelectListItem>();

            //if (hasusercategory) 
            //{ 
            //foreach (var item in subcat)
            //{
            //    var isitusercategory = item.Id == usercatid;
            //    var sub = new SelectListItem
            //    {
            //        Value = item.Id.ToString(),
            //        Text = item.Title,
            //        Selected = isitusercategory
            //    };
            //    listsubcat.Add(sub);
            //} 
            //}
            //else
            //{
            foreach (var item in subcat)
            {
                var sub = new SelectListItem
                {
                    Value = item.Id.ToString(),
                    Text = item.Title
                };
                listsubcat.Add(sub);
            }
            //}

            var model = new UserSubCategoryEventViewModel
            {
                EventTitle = await _context.Events.Where(ev => ev.Id == eventid).Select(res => res.Title).FirstOrDefaultAsync(),
                UserSubCategoryEventId = await _context.EventUsers.Where(eu => eu.UserId == userid && eu.UserSubcategoryEvent.EventId == eventid)?.Select(res => res.UserSubcategoryEventId).FirstOrDefaultAsync(),
                SubCategories = listsubcat
            };

            return View(model);
        }

        // POST: UserPackagesController/EditSub/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditSub(int id, int eventid, Guid userid, UserSubCategoryEventViewModel model)
        {
            if (!await CheckDelegationPermission(userid))
            {
                return Forbid();
            }
            var thisuser = await _userManager.GetUserAsync(HttpContext.User);
            if (ModelState.IsValid)
            {
                try
                {
                    var usceid = model.UserSubCategoryEventId;
                    var eventuser = new EventUser
                    {
                        UserId = userid,
                        UserSubcategoryEventId = usceid,
                        Creator = thisuser
                    };
                    _context.EventUsers.Add(eventuser);
                    await _context.SaveChangesAsync();

                    var reqid = await HasUserRequest(userid, eventid);
                    if (reqid == 0)
                    {
                        return RedirectToAction("List", new { userid, eventid });
                    }
                    else
                    {
                        return RedirectToAction("Details", new { id = reqid, userid, eventid });
                    }

                }
                catch
                {
                    return View(model);
                }
            }
            return View(model);
        }

        // GET: UserPackagesController/SetSubcategory/5
        /// <summary>
        /// Создает EventUser пользователю из ApplicationUser.UserSubCategoryId
        /// </summary>
        /// <param name="id"></param>
        /// <param name="eventid"></param>
        /// <param name="userid"></param>
        /// <returns></returns>
        public async Task<IActionResult> SetSubcategory(int? id, int? eventid, Guid? userid)
        {
            if (eventid == null || userid == null)
            {
                return NotFound();
            }
            if (!await CheckDelegationPermission(userid))
            {
                return Forbid();
            }
            var user = await _context.Users.FindAsync(userid);
            var thisuser = await _userManager.GetUserAsync(HttpContext.User);

            //Удалим все старые EventUsers на случай если человеку его создали а потом поменяли подкатегорию
            var eventusers = await _context.EventUsers.Where(eu => eu.UserSubcategoryEvent.EventId == eventid && eu.User == user).ToListAsync();
            foreach (var eur in eventusers)
            {
                _context.EventUsers.Remove(eur);
            }

            var usersubcategoryid = user.UserSubcategoryId;

            if (usersubcategoryid != null)
            {
                if (ModelState.IsValid)
                {
                    try
                    {
                        var subcategoryid = await _context.UserSubcategoryEvents.Where(use => use.UserSubcategoryId == usersubcategoryid && use.EventId == eventid).Select(res => res.Id).FirstOrDefaultAsync();
                        var eventuser = new EventUser
                        {
                            UserId = (Guid)userid,
                            UserSubcategoryEventId = subcategoryid,
                            Creator = thisuser
                        };
                        _context.EventUsers.Add(eventuser);
                        await _context.SaveChangesAsync();

                        var reqid = await HasUserRequest((Guid)userid, (int)eventid);
                        if (reqid == 0)
                        {
                            return RedirectToAction("List", new { userid, eventid });
                        }
                        else
                        {
                            return RedirectToAction("Details", new { id = reqid, userid, eventid });
                        }

                    }
                    catch
                    {

                    }
                }

            }
            return RedirectToAction("EditSub", new { id, eventid, userid });
        }

        /// <summary>
        /// Бронирует услуги в пакете
        /// </summary>
        /// <param name="id"></param>
        /// <param name="eventid"></param>
        /// <param name="user"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task BookPackage(int id, int eventid, ApplicationUser user, UserPackageViewModel model)
        {
            var packageid = model.PackageId;
            foreach (var service in model.Services)
            {
                var ServiceId = service.ServiceId;
                //Радиокнопка выбрана
                if (service.RadioChecked)
                {
                    var StartDate = service.StartDate;
                    var EndDate = service.EndDate;
                    //Для отеля устанавливаем время заезда и выезда
                    if (service.IsHotel)
                    {
                        StartDate = StartDate.AddHours(15);
                        EndDate = EndDate.AddHours(12);
                    }
                    else
                    {
                        EndDate = EndDate.AddHours(23).AddMinutes(59);
                    }
                    var packageservice = await _context.PackageServices.Where(ps => ps.Id == ServiceId).FirstOrDefaultAsync();
                    var mindayscount = packageservice.MinimalDaysCount;

                    //проверка квот и минимального количества дней
                    var newuserps = new Models.Service.UserPackageService
                    {
                        User = user,
                        UserId = user.Id,
                        Event = await _context.Events.Where(ev => ev.Id == eventid).FirstOrDefaultAsync(),
                        PackageService = await _context.PackageServices.Where(ps => ps.Id == ServiceId).FirstOrDefaultAsync(),
                        StartDate = StartDate,
                        FinishDate = EndDate,
                        Status = UserPackageServiceStatus.draft,
                        Creator = user
                    };
                    await _context.UserPackageServices.AddAsync(newuserps);
                    await _context.SaveChangesAsync();
                }
            }
        }

        /// <summary>
        /// Проверяет все услуги в пакете перед бронированием
        /// </summary>
        /// <param name="id"></param>
        /// <param name="eventid"></param>
        /// <param name="user"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task<bool> CheckBooking(int id, int eventid, ApplicationUser user, UserPackageViewModel model)
        {
            var packageid = model.PackageId;

            for (int i = 0; i < model.Services.Count; i++)
            {
                var service = model.Services[i];
                var PackageServiceId = service.ServiceId;
                var ServiceId = await _context.PackageServices.Where(ps => ps.Id == PackageServiceId).Select(res => res.Service.Id).FirstOrDefaultAsync();
                //Радиокнопка выбрана
                if (service.RadioChecked)
                {
                    var StartDate = service.StartDate;
                    var EndDate = service.EndDate;
                    //Для отеля устанавливаем время заезда и выезда
                    if (service.IsHotel)
                    {
                        StartDate = StartDate.AddHours(15);
                        EndDate = EndDate.AddHours(12);
                    }
                    var packageservice = await _context.PackageServices.Where(ps => ps.Id == PackageServiceId).FirstOrDefaultAsync();
                    //x-1 потому что всегда получается что x дней - это x-1 полных суток
                    var mindayscount = packageservice.MinimalDaysCount - 1;

                    //Проверяем что даты не выходят за зашитые в пакете
                    if (StartDate < service.ServiceStartDate || EndDate > service.ServiceEndDate)
                    {
                        string key = "Services[" + i + "].StartDate";
                        ModelState.AddModelError(key, "Пожалуйста, выберите даты в указанном выше промежутке.");
                        return false;
                    }

                    //Проверяем что срок не меньше минимального
                    if (!CheckMinimalDaysCount(mindayscount, StartDate, EndDate))
                    {
                        string key = "Services[" + i + "].StartDate";
                        ModelState.AddModelError(key, "Ваша бронь не соответствует минимальному сроку.");
                        return false;
                    }

                    //Проверяем квоты
                    var fp = await GetFreePlacesForService(ServiceId, StartDate, EndDate);
                    if (fp <= 0)
                    {
                        string key = "Services[" + i + "].StartDate";
                        ModelState.AddModelError(key, "К сожалению, на указанные даты недостаточно мест.");
                        return false;
                    }
                    else
                    {
                        //Красиво обновляет цифру при смене дат
                        model.Services[i].FreePlaces = fp;
                    }
                }
                else
                {
                    //Кнопка не выбрана, надо бы проверить, есть ли вообще выбранная услуга из этой группы
                    var group = service.RadioGroupName;
                    if (!model.Services.Where(ser => ser.RadioGroupName == group && ser.RadioChecked == true).Any())
                    {
                        ViewBag.StatusMessage = "Необходимо выбрать хотябы одну услугу в каждой группе";
                        return false;
                    }
                }
            }
            return true;
        }

        /// <summary>
        /// Проверяет срок на соответствие минимальному
        /// </summary>
        /// <param name="mindayscount"></param>
        /// <param name="StartDate"></param>
        /// <param name="EndDate"></param>
        /// <returns></returns>
        public bool CheckMinimalDaysCount(int mindayscount, DateTime StartDate, DateTime EndDate)
        {
            var dayscount = (EndDate.Date - StartDate.Date).Days;
            return dayscount >= mindayscount;
        }

        /// <summary>
        /// Возвращает количество свободных мест для бронирования конкретной услуги на данный период
        /// </summary>
        /// <param name="serviceid"></param>
        /// <param name="StartDate"></param>
        /// <param name="EndDate"></param>
        /// <returns></returns>
        public async Task<int> GetFreePlacesForService(int serviceid, DateTime StartDate, DateTime EndDate)
        {
            var dayscount = (EndDate.Date - StartDate.Date).Days;
            var minfreepaces = 9000;
            //перебираем каждый день срока брони услуги
            for (int i = 0; i <= dayscount; i++)
            {
                //первый период действует со StartDate до начала следующего дня,
                //периоды по 1 сутки
                //последний период действует с начала последнего дня до EndDate

                var StartPeriod = i == 0 ? StartDate : StartDate.AddDays(i).Date;
                var EndPeriod = i == dayscount ? EndDate : StartDate.AddDays(i + 1).Date;

                //ищем квоту этой услуги, в которую попадает период
                var quotaforperiod = await _context.Quotas.Where(q => q.StartDate <= StartPeriod && q.FinishDate >= EndPeriod && q.Service.Id == serviceid).ToListAsync();
                if (!quotaforperiod.Any())
                {
                    return 0;
                }
                var numquotaforperiod = quotaforperiod.Select(res => res.AvailableNum).FirstOrDefault();

                //Находим все брони данной услуги(во всех пакетах), пересекющиеся с этим периодом кроме как в одной точке и имеющие статус book или accepted
                var reserved = await _context.UserPackageServices.Where(ups => (ups.StartDate > StartPeriod && ups.StartDate < EndPeriod
                || ups.FinishDate > StartPeriod && ups.FinishDate < EndPeriod
                || ups.StartDate == StartPeriod
                || ups.FinishDate == EndPeriod)
                && (ups.Status == UserPackageServiceStatus.book || ups.Status == UserPackageServiceStatus.accepted || ups.Status == UserPackageServiceStatus.contracted || ups.Status == UserPackageServiceStatus.completed)
                && ups.PackageService.Service.Id == serviceid).CountAsync();
                //Свободно мест на этот период
                var freeplaces = numquotaforperiod - reserved;

                //Сравниваем с минимальным на весь срок количеством свободных мест
                minfreepaces = freeplaces < minfreepaces ? freeplaces : minfreepaces;
            }
            return minfreepaces;
        }

        /// <summary>
        /// Возвращает id пакета если пользователь уже бронировал его для данного мероприятия
        /// </summary>
        /// <param name="userid"></param>
        /// <param name="eventid"></param>
        /// <returns></returns>
        public async Task<int?> HasUserRequest(Guid userid, int eventid)
        {
            return await _context.UserPackageServices.Where(ups => ups.Event.Id == eventid && ups.UserId == userid)?.Select(res => res.PackageService.Package.Id).FirstOrDefaultAsync();
        }

        /// <summary>
        /// Собирает модель для List и Edit
        /// </summary>
        /// <param name="package"></param>
        /// <param name="userid"></param>
        /// <param name="eventid"></param>
        /// <returns></returns>
        public async Task<UserPackageViewModel> GetFullPackage(Package package, Guid? userid, int eventid)
        {
            var UserPackageServices = new List<UserPackageService>();
            var PackageServices = await _context.PackageServices.Where(ps => ps.Package == package).Include(ps => ps.Service.ServiceType).OrderBy(ps => ps.Service.ServiceType.Order).ToListAsync();
            foreach (var packageService in PackageServices)
            {
                //Проверяем, была ли уже выбрана ранее эта услуга.
                var alreadyChoosed = await _context.UserPackageServices.Where(ups => ups.UserId == userid && ups.PackageService == packageService).FirstOrDefaultAsync();
                var StartDate = new DateTime();
                var EndDate = new DateTime();
                bool boolchecked = false;
                //Если была выбрана, то назначем выбранные параметры в модель
                if (alreadyChoosed != null)
                {
                    StartDate = alreadyChoosed.StartDate;
                    EndDate = alreadyChoosed.FinishDate;
                    boolchecked = true;
                }
                else
                {
                    StartDate = packageService.StartDate;
                    EndDate = packageService.FinishDate;
                }

                //Возможность изменять даты
                //Если временные рамки услуги больше чем минимальное доступное количество дней бронирования
                //То даты изменять можно. Иначе нельзя.
                var countofdaysinpackage = (packageService.FinishDate.Date - packageService.StartDate.Date).Days;
                var canchangedates = countofdaysinpackage > packageService.MinimalDaysCount;

                //Берем тип услуги чтобы понять, есть ли выбор в этом типе услуги внутри пакета
                var servicetype = packageService.Service.ServiceType;

                //Запрашиваем количество свободных мест для услуги
                var servid = packageService.Service.Id;
                var freeplaces = await GetFreePlacesForService(servid, StartDate, EndDate);

                var userPackageService = new UserPackageService
                {
                    ServiceId = packageService.Id,
                    ServiceName = packageService.Service.Name,
                    ServiceDescription = packageService.Service.Description,
                    StartDate = StartDate,
                    EndDate = EndDate,
                    ServiceStartDate = packageService.StartDate,
                    ServiceEndDate = packageService.FinishDate,
                    RadioEnabled = _context.PackageServices.Where(ps => ps.Package == package && ps.Service.ServiceType == servicetype).Count() > 1,
                    RadioChecked = _context.PackageServices.Where(ps => ps.Package == package && ps.Service.ServiceType == servicetype).Count() > 1 ? boolchecked : true,
                    RadioGroupName = servicetype.Name,
                    CanChangeDates = canchangedates,
                    MinimalDaysCount = packageService.MinimalDaysCount,
                    IsHotel = packageService.Service.ServiceType.Id == 1,
                    ShowDates = packageService.Service.ServiceType.ShowDates,
                    FreePlaces = freeplaces
                };
                UserPackageServices.Add(userPackageService);
            }
            //Находим максимальное количество свободных мест в каждой группе услуг и выбираем их них минимальное
            //Получается количество свободных мест для брони пакета
            var freeplacesforpackage = (from ups in UserPackageServices
                                        group ups by ups.RadioGroupName into res
                                        select new
                                        {
                                            name = res.Key,
                                            max = res.Max(x => x.FreePlaces)
                                        }).Select(result => result.max).Min();

            if (freeplacesforpackage > 0)
            {
                //Выбран ли пакет
                bool packagechoosed = await _context.UserPackageServices.Where(ups => ups.PackageService.Package == package && ups.UserId == userid).AnyAsync();
                //Находим самую позднюю дату начала и самую раннюю дату окончания действия услуг в пакете: это временные рамки пакета.
                var PackageStartDate = await _context.PackageServices.Where(ps => ps.Package == package).Select(pa => pa.StartDate).MinAsync();
                var PackageEndDate = await _context.PackageServices.Where(ps => ps.Package == package).Select(pa => pa.FinishDate).MaxAsync();
                var pack = new UserPackageViewModel
                {
                    PackageName = package.Name,
                    PackageDescription = package.Description,
                    PackageId = package.Id,
                    Fixed = !_context.PackageServices.Where(ps => ps.Package == package).Include(p => p.Service.ServiceType).AsEnumerable().GroupBy(p => p.Service.ServiceType).Select(res => new
                    {
                        ServiceType = res.Key,
                        ServiceCount = res.Count()
                    }).Where(pr => pr.ServiceCount > 1).Any(),
                    Choosed = packagechoosed,
                    Services = UserPackageServices,
                    StartDate = PackageStartDate,
                    EndDate = PackageEndDate,
                    EventId = eventid,
                    FreePackagesCount = freeplacesforpackage
                };
                return pack;
            }
            return null;
        }

        /// <summary>
        /// Проверка, может ли пользователь выбирать пакет за данного пользователя
        /// </summary>
        /// <param name="userid">Id пользователя, за которого выбирают пакет</param>
        /// <returns></returns>
        public async Task<bool> CheckDelegationPermission(Guid? userid)
        {
            var checkeduser = await _context.Users.FindAsync(userid);
            //Проверка на аккредитатора
            var isaccreditator = User.IsInRole("accreditator");
            return checkeduser.CanEdit(HttpContext.User.GetId()) || isaccreditator;
        }

        /// <summary>
        /// Проверка, можно ли забронировать данный пакет для данного пользователя
        /// </summary>
        /// <param name="eventid"></param>
        /// <param name="userid"></param>
        /// <param name="packageid"></param>
        /// <returns></returns>
        public async Task<bool> CheckPackagePermission(int eventid, Guid userid, int packageid)
        {
            var subcat = await _context.EventUsers
                .Where(eu => eu.UserSubcategoryEvent.EventId == eventid && eu.UserId == userid)
                .Select(eur => eur.UserSubcategoryEvent.UserSubcategoryCategory)
                .FirstOrDefaultAsync();

            return await _context.UserSubcategoryEventPackages
                .Where(re => re.UserSubcategoryEvent.UserSubcategoryCategory == subcat && re.UserSubcategoryEvent.EventId == eventid)
                .Select(r => r.Package)
                .Where(res => res.Id == packageid)
                .AnyAsync();
        }

        private readonly string[] columns = new string[] { "Фамилия", "Имя", "Отчество", "Серия и номер паспорта", "Телефон", "Электронная почта", "Страна", "Регион", "Компания", "Категория", "Подкатегория", "Компетенция", "Дата регистрации", "Дата прибытия", "Номер самолета/поезда", "Дата отъезда", "Номер самолета/поезда", "Реквизиты", "Пакет", "Статус пакета", "Стоимость пакета", "Аккредитация" };

        [Authorize(Roles = "administrator,moderator")]
        // GET: UserPackagesController/DownloadRequests
        public ActionResult<DownloadRequestViewModel> DownloadRequests()
        {
            var model = new DownloadRequestViewModel
            {
                Columns = columns.ToList(),
                EndDate = DateTime.Now.AddDays(1).Date,
                StartDate = DateTime.Now.AddDays(-2).Date
            };
            return View(model);

        }


        // POST: UserPackagesController/DownloadRequests/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DownloadRequests([Bind("StartDate, EndDate")] DownloadRequestViewModel model, string submitButton)
        {
            if (ModelState.IsValid)
            {
                var StartDate = model.StartDate;
                var EndDate = model.EndDate;

                //Определяем, что за кнопка нажата
                return submitButton switch
                {
                    "Сохранить выгрузку(заявки)" => await DownloadOnlyRequest(StartDate, EndDate),
                    "Сохранить выгрузку(пользователи)" => await DownloadUsers(StartDate, EndDate),
                    "Сохранить выгрузку(питание)" => await DownloadFood(StartDate, EndDate),
                    _ => View(model)
                };

            }
            return View(model);
        }

        //Сохранение выгрузки заявок
        public async Task<IActionResult> DownloadOnlyRequest(DateTime StartDate, DateTime EndDate)
        {
            var users = await _context.UserPackageServices.Where(r => r.CreateDate >= StartDate && r.CreateDate <= EndDate).Include(us => us.User.Country).Include(us => us.User.Competence).Include(us => us.User.RussiaSubject).Include(us => us.User.UserCategory).Include(us => us.User.UserSubcategory).Include(us => us.User.RegisteredUser).Include(us => us.User.BadgeServiceCheckups).Select(res => res.User).Distinct().ToListAsync();
            var bytes = await DownloadFile(users, StartDate, EndDate);
            if (bytes == null || bytes.Length == 0)
            {
                return NotFound();
            }

            return File(
                fileContents: bytes,
                contentType: "application/vnd.ms-excel",
                fileDownloadName: "report" + DateTime.Now + ".xls"
            );
        }

        //Сохранение выгрузки пользователей
        public async Task<IActionResult> DownloadUsers(DateTime StartDate, DateTime EndDate)
        {
            var users = await _context.Users.Where(r => r.RegisterDate >= StartDate && r.RegisterDate <= EndDate).Include(us => us.PackageServices).Include(us => us.Country).Include(us => us.Competence).Include(us => us.RussiaSubject).Include(us => us.UserCategory).Include(us => us.UserSubcategory).Include(us => us.RegisteredUser).Include(us => us.BadgeServiceCheckups).ToListAsync();
            var bytes = await DownloadFile(users, StartDate, EndDate);
            if (bytes == null || bytes.Length == 0)
            {
                return NotFound();
            }

            return File(
                fileContents: bytes,
                contentType: "application/vnd.ms-excel",
                fileDownloadName: "usersreport" + DateTime.Now + ".xls"
            );
        }

        //Метод формирования файла выгрузки
        public async Task<byte[]> DownloadFile(List<ApplicationUser> users, DateTime StartDate, DateTime EndDate)
        {
            using (StringWriter sw = new())
            {
                sw.WriteLine("<?xml version=\"1.0\"?>");
                sw.WriteLine("<?mso-application progid=\"Excel.Sheet\"?>");
                sw.WriteLine("<Workbook xmlns=\"urn:schemas-microsoft-com:office:spreadsheet\"");
                sw.WriteLine(" xmlns:o=\"urn:schemas-microsoft-com:office:office\"");
                sw.WriteLine(" xmlns:x=\"urn:schemas-microsoft-com:office:excel\"");
                sw.WriteLine(" xmlns:ss=\"urn:schemas-microsoft-com:office:spreadsheet\"");
                sw.WriteLine(" xmlns:html=\"http://www.w3.org/TR/REC-html40\">");
                sw.WriteLine(" <DocumentProperties xmlns=\"urn:schemas-microsoft-com:office:office\">");
                sw.WriteLine("  <Version>16.00</Version>");
                sw.WriteLine(" </DocumentProperties>");
                sw.WriteLine(" <OfficeDocumentSettings xmlns=\"urn:schemas-microsoft-com:office:office\">");
                sw.WriteLine("  <AllowPNG/>");
                sw.WriteLine(" </OfficeDocumentSettings>");
                sw.WriteLine(" <ExcelWorkbook xmlns=\"urn:schemas-microsoft-com:office:excel\">");
                sw.WriteLine("  <WindowHeight>12330</WindowHeight>");
                sw.WriteLine("  <WindowWidth>28800</WindowWidth>");
                sw.WriteLine("  <WindowTopX>0</WindowTopX>");
                sw.WriteLine("  <WindowTopY>0</WindowTopY>");
                sw.WriteLine("  <ProtectStructure>False</ProtectStructure>");
                sw.WriteLine("  <ProtectWindows>False</ProtectWindows>");
                sw.WriteLine(" </ExcelWorkbook>");
                sw.WriteLine(" <Styles>");
                sw.WriteLine("  <Style ss:ID=\"Default\" ss:Name=\"Normal\">");
                sw.WriteLine("   <Alignment ss:Vertical=\"Bottom\"/>");
                sw.WriteLine("   <Borders/>");
                sw.WriteLine("   <Font ss:FontName=\"Calibri\" x:CharSet=\"204\" x:Family=\"Swiss\" ss:Size=\"11\"");
                sw.WriteLine("    ss:Color=\"#000000\"/>");
                sw.WriteLine("   <Interior/>");
                sw.WriteLine("   <NumberFormat/>");
                sw.WriteLine("   <Protection/>");
                sw.WriteLine("  </Style>");
                sw.WriteLine("  <Style ss:ID=\"s62\">");
                sw.WriteLine("   <Alignment ss:Vertical=\"Bottom\"/>");
                sw.WriteLine("   <Borders/>");
                sw.WriteLine("   <Font ss:FontName=\"Calibri\" x:CharSet=\"204\" x:Family=\"Swiss\" ss:Size=\"11\"");
                sw.WriteLine("    ss:Color=\"#000000\" ss:Bold=\"1\"/>");
                sw.WriteLine("   <Interior/>");
                sw.WriteLine("   <NumberFormat/>");
                sw.WriteLine("   <Protection/>");
                sw.WriteLine("  </Style>");
                sw.WriteLine("  <Style ss:ID=\"s63\">");
                sw.WriteLine("   <NumberFormat/>");
                sw.WriteLine("  </Style>");
                sw.WriteLine(" </Styles>");
                sw.WriteLine(" <Worksheet ss:Name=\"Лист1\">");
                sw.WriteLine("  <Table>");

                sw.Write("<Row>");
                foreach (var col in columns)
                    sw.Write($"<Cell ss:StyleID=\"s62\"><Data ss:Type=\"String\">{col}</Data></Cell>");
                sw.WriteLine("</Row>");

                var res = new List<RequestForExcel>();
                foreach (var user in users)
                {
                    string packages = "";
                    string statuses = "";
                    double cost = 0;
                    //Представим, что пользователь может забронировать больше одного пакета (напр. на разные мероприятия)
                    var userpackages = await _context.UserPackageServices.Where(ups => ups.User == user && (ups.Status == UserPackageServiceStatus.accepted || ups.Status == UserPackageServiceStatus.contracted || ups.Status == UserPackageServiceStatus.completed)).Select(p => p.PackageService.Package).Distinct().ToListAsync();
                    foreach (var userpackage in userpackages)
                    {
                        packages += userpackage.Name + " ";
                        var upses = await _context.UserPackageServices.Where(ups => ups.User == user && ups.PackageService.Package == userpackage).Include(res => res.PackageService.Service).ToListAsync();
                        var status = upses.First().Status switch
                        {
                            UserPackageServiceStatus.accepted => "Бронь подтверждена",
                            UserPackageServiceStatus.contracted => "Отправлен договор",
                            UserPackageServiceStatus.completed => "Оплачено",
                            _ => ""
                        };
                        statuses += status + " ";
                        cost += Math.Round(await GetPackageCost(upses), 2, MidpointRounding.AwayFromZero);
                    }

                    var rfe = new RequestForExcel
                    {
                        SecondName = user.SecondName,
                        Name = user.Name,
                        MiddleName = user.MiddleName,
                        PassportNum = user.PassportNumber,
                        PhoneNumber = user.PhoneNumber,
                        Email = user.Email,
                        Country = user.Country != null ? user.Country.Title : null,
                        Region = user.RussiaSubject != null ? user.RussiaSubject.Title : null,
                        Company = user.CompanyName,
                        Category = user.UserCategory != null ? user.UserCategory.Title : null,
                        SubCategory = user.UserSubcategory != null ? user.UserSubcategory.Title : null,
                        Competence = user.Competence != null ? user.Competence.Title : null,
                        RegisterDate = user.RegisterDate,
                        ArrivalDateTime = userpackages.Any() ? user.ArrivalDateTime : null,
                        ArrivalDetails = userpackages.Any() ? user.ArrivalDetails : null,
                        DepartureDateTime = userpackages.Any() ? user.DepartureDateTime : null,
                        DepartureDetails = userpackages.Any() ? user.DepartureDetails : null,
                        BankDetails = userpackages.Any() ? user.BankDetails != null ? user.BankDetails : user.RegisteredUser != null && user.RegisteredUser.BankDetails != null && user.RegisteredUser.IsGeneralBankDetails ? user.RegisteredUser.BankDetails : null : null,
                        CurrentPackage = packages,
                        CurrentPackageStatus = statuses,
                        PackageCost = userpackages.Any() ? cost.ToString() : null,
                        AccredStatus = user.BadgeServiceCheckups.Where(bsc => bsc.BadgeServiceId == 11 && bsc.Type == 0).Any() ? "БЕЙДЖ ВРУЧЕН" : null

                    };
                    res.Add(rfe);
                }

                foreach (var row in res)
                {
                    sw.Write("<Row>");
                    InsertCell(sw, row.SecondName, false);
                    InsertCell(sw, row.Name, false);
                    InsertCell(sw, row.MiddleName, false);
                    InsertCell(sw, row.PassportNum, false);
                    InsertCell(sw, row.PhoneNumber, false);
                    InsertCell(sw, row.Email, false);
                    InsertCell(sw, row.Country, false);
                    InsertCell(sw, row.Region, false);
                    InsertCell(sw, row.Company, false);
                    InsertCell(sw, row.Category, false);
                    InsertCell(sw, row.SubCategory, false);
                    InsertCell(sw, row.Competence, false);
                    InsertCell(sw, row.RegisterDate.ToShortDateString(), false);
                    InsertCell(sw, row.ArrivalDateTime.ToString(), false);
                    InsertCell(sw, row.ArrivalDetails, false);
                    InsertCell(sw, row.DepartureDateTime.ToString(), false);
                    InsertCell(sw, row.DepartureDetails, false);
                    InsertCell(sw, row.BankDetails, false);
                    InsertCell(sw, row.CurrentPackage, false);
                    InsertCell(sw, row.CurrentPackageStatus, false);
                    InsertCell(sw, row.PackageCost, false);
                    InsertCell(sw, row.AccredStatus, false);

                    sw.WriteLine("</Row>");
                }

                sw.WriteLine("  </Table>");
                sw.WriteLine("  <WorksheetOptions xmlns=\"urn:schemas-microsoft-com:office:excel\">");
                sw.WriteLine("   <PageSetup>");
                sw.WriteLine("    <Header x:Margin=\"0.3\"/>");
                sw.WriteLine("    <Footer x:Margin=\"0.3\"/>");
                sw.WriteLine("    <PageMargins x:Bottom=\"0.75\" x:Left=\"0.7\" x:Right=\"0.7\" x:Top=\"0.75\"/>");
                sw.WriteLine("   </PageSetup>");
                sw.WriteLine("   <Print>");
                sw.WriteLine("    <ValidPrinterInfo/>");
                sw.WriteLine("    <PaperSizeIndex>9</PaperSizeIndex>");
                sw.WriteLine("    <HorizontalResolution>600</HorizontalResolution>");
                sw.WriteLine("    <VerticalResolution>600</VerticalResolution>");
                sw.WriteLine("   </Print>");
                sw.WriteLine("   <Selected/>");
                sw.WriteLine("   <ProtectObjects>False</ProtectObjects>");
                sw.WriteLine("   <ProtectScenarios>False</ProtectScenarios>");
                sw.WriteLine("  </WorksheetOptions>");
                sw.WriteLine(" </Worksheet>");
                sw.WriteLine("</Workbook>");

                string outp = sw.ToString();
                byte[] bytes = Encoding.UTF8.GetBytes(outp);
                return bytes;
            }
        }

        public async Task<IActionResult> DownloadFood(DateTime StartDate, DateTime EndDate)
        {
            //Формируем объекты
            var FoodUsers = new List<UserFood>();
            var users = await _context.BadgeServiceCheckups.Where(bsc => (bsc.BadgeService.Title.Contains("ЗАВТРАК") || bsc.BadgeService.Title.Contains("ОБЕД") || bsc.BadgeService.Title.Contains("УЖИН")) && bsc.Type == ServiceCheckupType.Approve && bsc.CreateDate >= StartDate && bsc.CreateDate <= EndDate).Select(res => res.User).Distinct().Include(re => re.BadgeServiceCheckups).ThenInclude(re => re.BadgeService).Include(re => re.Country).Include(re => re.UserSubcategory).Include(re => re.RussiaSubject).OrderBy(r => r.UserSubcategoryId).ThenBy(r => r.RussiaSubject.Title).ThenBy(r => r.Country.Title).ThenBy(r => r.SecondName).ToListAsync();
            foreach (var user in users)
            {
                var FD = EndDate.Date;
                var checkdate = StartDate.Date;
                var newdays = new List<FoodDay>();
                while (checkdate < FD)
                {
                    var newbreakfast = user.BadgeServiceCheckups.Count(bsc => bsc.CreateDate.Date == checkdate && bsc.BadgeService.Title.Contains("ЗАВТРАК") && bsc.Type == ServiceCheckupType.Approve);
                    var newlunch = user.BadgeServiceCheckups.Count(bsc => bsc.CreateDate.Date == checkdate && bsc.BadgeService.Title.Contains("ОБЕД") && bsc.Type == ServiceCheckupType.Approve);
                    var newdinner = user.BadgeServiceCheckups.Count(bsc => bsc.CreateDate.Date == checkdate && bsc.BadgeService.Title.Contains("УЖИН") && bsc.Type == ServiceCheckupType.Approve);
                    var newFoodDay = new FoodDay
                    {
                        fooddate = checkdate,
                        Breakfast = newbreakfast == 0 ? "" : newbreakfast.ToString(),
                        Lunch = newlunch == 0 ? "" : newlunch.ToString(),
                        Dinner = newdinner == 0 ? "" : newdinner.ToString()
                    };
                    newdays.Add(newFoodDay);
                    checkdate = checkdate.AddDays(1);
                }
                var NewUserFood = new UserFood
                {
                    SubCategory = user.UserSubcategory.Title,
                    Region = user.Country == null ? user.RussiaSubject == null ? "" : user.RussiaSubject.Title : user.Country.Title,
                    SecondName = user.SecondName,
                    Name = user.Name,
                    MiddleName = user.MiddleName,
                    Days = newdays
                };
                FoodUsers.Add(NewUserFood);
            }

            //Строим файл.
            byte[] bytes = null;
            using (StringWriter sw = new())
            {
                sw.WriteLine("<?xml version=\"1.0\"?>");
                sw.WriteLine("<?mso-application progid=\"Excel.Sheet\"?>");
                sw.WriteLine("<Workbook xmlns=\"urn:schemas-microsoft-com:office:spreadsheet\"");
                sw.WriteLine(" xmlns:o=\"urn:schemas-microsoft-com:office:office\"");
                sw.WriteLine(" xmlns:x=\"urn:schemas-microsoft-com:office:excel\"");
                sw.WriteLine(" xmlns:ss=\"urn:schemas-microsoft-com:office:spreadsheet\"");
                sw.WriteLine(" xmlns:html=\"http://www.w3.org/TR/REC-html40\">");
                sw.WriteLine(" <DocumentProperties xmlns=\"urn:schemas-microsoft-com:office:office\">");
                sw.WriteLine("  <Version>16.00</Version>");
                sw.WriteLine(" </DocumentProperties>");
                sw.WriteLine(" <OfficeDocumentSettings xmlns=\"urn:schemas-microsoft-com:office:office\">");
                sw.WriteLine("  <AllowPNG/>");
                sw.WriteLine(" </OfficeDocumentSettings>");
                sw.WriteLine(" <ExcelWorkbook xmlns=\"urn:schemas-microsoft-com:office:excel\">");
                sw.WriteLine("  <WindowHeight>12330</WindowHeight>");
                sw.WriteLine("  <WindowWidth>28800</WindowWidth>");
                sw.WriteLine("  <WindowTopX>0</WindowTopX>");
                sw.WriteLine("  <WindowTopY>0</WindowTopY>");
                sw.WriteLine("  <ProtectStructure>False</ProtectStructure>");
                sw.WriteLine("  <ProtectWindows>False</ProtectWindows>");
                sw.WriteLine(" </ExcelWorkbook>");
                sw.WriteLine(" <Styles>");
                sw.WriteLine("  <Style ss:ID=\"Default\" ss:Name=\"Normal\">");
                sw.WriteLine("   <Alignment ss:Vertical=\"Bottom\"/>");
                sw.WriteLine("   <Borders/>");
                sw.WriteLine("   <Font ss:FontName=\"Calibri\" x:CharSet=\"204\" x:Family=\"Swiss\" ss:Size=\"11\"");
                sw.WriteLine("    ss:Color=\"#000000\"/>");
                sw.WriteLine("   <Interior/>");
                sw.WriteLine("   <NumberFormat/>");
                sw.WriteLine("   <Protection/>");
                sw.WriteLine("  </Style>");
                sw.WriteLine("  <Style ss:ID=\"s62\">");
                sw.WriteLine("   <Alignment ss:Vertical=\"Bottom\"/>");
                sw.WriteLine("   <Borders/>");
                sw.WriteLine("   <Font ss:FontName=\"Calibri\" x:CharSet=\"204\" x:Family=\"Swiss\" ss:Size=\"11\"");
                sw.WriteLine("    ss:Color=\"#000000\" ss:Bold=\"1\"/>");
                sw.WriteLine("   <Interior/>");
                sw.WriteLine("   <NumberFormat/>");
                sw.WriteLine("   <Protection/>");
                sw.WriteLine("  </Style>");
                sw.WriteLine("  <Style ss:ID=\"s63\">");
                sw.WriteLine("   <NumberFormat/>");
                sw.WriteLine("  </Style>");
                sw.WriteLine(" </Styles>");
                sw.WriteLine(" <Worksheet ss:Name=\"Лист1\">");
                sw.WriteLine("  <Table>");

                //Шапка
                sw.Write("<Row>");
                sw.Write($"<Cell ss:StyleID=\"s62\"><Data ss:Type=\"String\">Подкатегория</Data></Cell>");
                sw.Write($"<Cell ss:StyleID=\"s62\"><Data ss:Type=\"String\">Регион</Data></Cell>");
                sw.Write($"<Cell ss:StyleID=\"s62\"><Data ss:Type=\"String\">Фамилия</Data></Cell>");
                sw.Write($"<Cell ss:StyleID=\"s62\"><Data ss:Type=\"String\">Имя</Data></Cell>");
                sw.Write($"<Cell ss:StyleID=\"s62\"><Data ss:Type=\"String\">Отчество</Data></Cell>");
                var checkdate = StartDate.Date;
                var FD = EndDate.Date;
                while (checkdate < FD)
                {
                    var date = checkdate.ToString().Substring(0, 5);
                    sw.Write($"<Cell ss:StyleID=\"s62\"><Data ss:Type=\"String\">{date} - З</Data></Cell>");
                    sw.Write($"<Cell ss:StyleID=\"s62\"><Data ss:Type=\"String\">{date} - О</Data></Cell>");
                    sw.Write($"<Cell ss:StyleID=\"s62\"><Data ss:Type=\"String\">{date} - У</Data></Cell>");
                    checkdate = checkdate.AddDays(1);
                }
                sw.WriteLine("</Row>");

                int period = (EndDate - StartDate).Days;
                var subcat = "";
                foreach (var row in FoodUsers)
                {
                    if (row.SubCategory == subcat)
                    {
                        sw.Write("<Row>");
                        InsertCell(sw, row.SubCategory, false);
                        InsertCell(sw, row.Region, false);
                        InsertCell(sw, row.SecondName, false);
                        InsertCell(sw, row.Name, false);
                        InsertCell(sw, row.MiddleName, false);
                        for (int j = 0; j < period; j++)
                        {
                            InsertCell(sw, row.Days[j].Breakfast, row.Days[j].Breakfast == "1" ? true : false);
                            InsertCell(sw, row.Days[j].Lunch, row.Days[j].Lunch == "1" ? true : false);
                            InsertCell(sw, row.Days[j].Dinner, row.Days[j].Dinner == "1" ? true : false);
                        }
                        sw.WriteLine("</Row>");
                        subcat = row.SubCategory;
                    }
                    else
                    {
                        sw.Write("<Row>");
                        for (int j = 0; j < period * 3 + 4; j++)
                        {
                            InsertCell(sw, null, true);
                        }
                        sw.WriteLine("</Row>");

                        sw.Write("<Row>");
                        InsertCell(sw, row.SubCategory, false);
                        InsertCell(sw, row.Region, false);
                        InsertCell(sw, row.SecondName, false);
                        InsertCell(sw, row.Name, false);
                        InsertCell(sw, row.MiddleName, false);
                        for (int j = 0; j < period; j++)
                        {
                            InsertCell(sw, row.Days[j].Breakfast, row.Days[j].Breakfast == "1" ? true : false);
                            InsertCell(sw, row.Days[j].Lunch, row.Days[j].Lunch == "1" ? true : false);
                            InsertCell(sw, row.Days[j].Dinner, row.Days[j].Dinner == "1" ? true : false);
                        }
                        sw.WriteLine("</Row>");
                        subcat = row.SubCategory;
                    }
                }

                sw.WriteLine("  </Table>");
                sw.WriteLine("  <WorksheetOptions xmlns=\"urn:schemas-microsoft-com:office:excel\">");
                sw.WriteLine("   <PageSetup>");
                sw.WriteLine("    <Header x:Margin=\"0.3\"/>");
                sw.WriteLine("    <Footer x:Margin=\"0.3\"/>");
                sw.WriteLine("    <PageMargins x:Bottom=\"0.75\" x:Left=\"0.7\" x:Right=\"0.7\" x:Top=\"0.75\"/>");
                sw.WriteLine("   </PageSetup>");
                sw.WriteLine("   <Print>");
                sw.WriteLine("    <ValidPrinterInfo/>");
                sw.WriteLine("    <PaperSizeIndex>9</PaperSizeIndex>");
                sw.WriteLine("    <HorizontalResolution>600</HorizontalResolution>");
                sw.WriteLine("    <VerticalResolution>600</VerticalResolution>");
                sw.WriteLine("   </Print>");
                sw.WriteLine("   <Selected/>");
                sw.WriteLine("   <ProtectObjects>False</ProtectObjects>");
                sw.WriteLine("   <ProtectScenarios>False</ProtectScenarios>");
                sw.WriteLine("  </WorksheetOptions>");
                sw.WriteLine(" </Worksheet>");
                sw.WriteLine("</Workbook>");

                string outp = sw.ToString();
                bytes = Encoding.UTF8.GetBytes(outp);
            }

            if (bytes == null || bytes.Length == 0)
            {
                return NotFound();
            }

            return File(
                fileContents: bytes,
                contentType: "application/vnd.ms-excel",
                fileDownloadName: "foodreport" + DateTime.Now + ".xls"
            );
        }

        public void InsertCell(StringWriter sw, string value, bool isItInt)
        {
            if (value != null)
            {
                sw.Write(isItInt ? "<Cell><Data ss:Type=\"Number\">" : "<Cell><Data ss:Type=\"String\">");

                foreach (var c in value.ToString())
                {
                    switch (c)
                    {
                        case '<':
                            sw.Write("&lt;");
                            break;
                        case '&':
                            sw.Write("&amp;");
                            break;
                        case '>':
                            sw.Write("&gt;");
                            break;
                        default:
                            sw.Write(c);
                            break;
                    }
                }
                sw.Write("</Data></Cell>");
            }
            else
            {
                sw.Write("<Cell><Data ss:Type=\"String\"></Data></Cell>");
            }
        }


        public class RequestForExcel
        {
            public string SecondName { get; set; }
            public string Name { get; set; }
            public string MiddleName { get; set; }
            public string PassportNum { get; set; }
            public string PhoneNumber { get; set; }
            public string Email { get; set; }
            public string Country { get; set; }
            public string Region { get; set; }
            public string Company { get; set; }
            public string Category { get; set; }
            public string SubCategory { get; set; }
            public string Competence { get; set; }
            public DateTime RegisterDate { get; set; }
            public DateTime? ArrivalDateTime { get; set; }
            public string ArrivalDetails { get; set; }
            public DateTime? DepartureDateTime { get; set; }
            public string DepartureDetails { get; set; }
            public string BankDetails { get; set; }
            public string CurrentPackage { get; set; }
            public string CurrentPackageStatus { get; set; }
            public string PackageCost { get; set; }
            public string AccredStatus { get; set; }
        }

        public class UserFood
        {
            public string SubCategory { get; set; }
            public string Region { get; set; }
            public string SecondName { get; set; }
            public string Name { get; set; }
            public string MiddleName { get; set; }
            public List<FoodDay> Days { get; set; }
        }

        public class FoodDay
        {
            public DateTime fooddate { get; set; }
            public string Breakfast { get; set; }
            public string Lunch { get; set; }
            public string Dinner { get; set; }
        }
    }
}
