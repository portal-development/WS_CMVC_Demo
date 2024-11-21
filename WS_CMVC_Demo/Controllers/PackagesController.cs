using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WS_CMVC_Demo.Data;
using WS_CMVC_Demo.Models;
using WS_CMVC_Demo.Models.PackagesViewModels;
using WS_CMVC_Demo.Models.Service;

namespace WS_CMVC_Demo.Controllers
{
    [Authorize(Roles = "administrator")]
    public class PackagesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger _logger;

        public PackagesController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, ILoggerFactory loggerFactory)
        {
            _context = context;
            _userManager = userManager;
            _logger = loggerFactory.CreateLogger<PackagesController>();
        }

        // GET: PackagesController
        public async Task<ActionResult> Index()
        {
            //Берем мероприятия которые еще не кончились
            var datenow = DateTime.Now;
            var Events = await _context.Events.Where(ev => ev.DateEnd > datenow).ToListAsync();
            return View(Events);
        }

        // GET: UserPackagesController/List/5
        public async Task<IActionResult> List(int? eventid)
        {
            if (eventid == null)
            {
                return NotFound();
            }
            var packages = await _context.Packages
                .OrderBy(r => r.Name)
                .Include(res => res.PackageServices.OrderBy(ps => ps.Service.ServiceType.Order))
                .ThenInclude(res => res.Service)
                .Include(res => res.UserSubcategoryEventPackages)
                .ThenInclude(res => res.UserSubcategoryEvent.UserSubcategoryCategory)
                .ThenInclude(res => res.Category)
                .ToListAsync();
            ViewBag.EventId = eventid;
            return View(packages);
        }


        // GET: PackagesController/Details/5
        public async Task<ActionResult> Details(int? eventid, int? id)
        {
            if (eventid == null || id == null)
            {
                return NotFound();
            }
            var model = await _context.PackageServices.Where(ser => ser.PackageId == id).Include(res => res.Service.ServiceType).OrderBy(res => res.Service.ServiceType.Order).ToListAsync();

            ViewBag.PackageName = await _context.Packages.Where(p => p.Id == id).Select(res => res.Name).FirstOrDefaultAsync();
            ViewBag.PackageId = id;
            ViewBag.EventId = eventid;
            return View(model);
        }

        // GET: PackagesController/Create
        public async Task<ActionResult> Create(int? eventid)
        {
            if (eventid == null)
            {
                return NotFound();
            }

            var userSubcategories = await _context.UserSubcategories
                .OrderBy(sc => sc.CategoryId)
                .ThenBy(sc => sc.Title)
                .Select(sc => new { sc.Id, Title = sc.Category.Title + " - " + sc.Title })
                .ToListAsync();

            ViewBag.UserSubcategories = new MultiSelectList(userSubcategories, "Id", "Title");

            return View();
        }

        // POST: PackagesController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(int eventid, [Bind("Name, Description, CheckedUserSubcategories")] PackageViewModel model)
        {
            if (ModelState.IsValid)
            {
                var package = new Package
                {
                    Name = model.Name,
                    Description = model.Description
                };
                _context.Packages.Add(package);
                await _context.SaveChangesAsync();

                foreach (var ch in model.CheckedUserSubcategories)
                {
                    var use = _context.UserSubcategoryEvents.Where(usep => usep.EventId == eventid && usep.UserSubcategoryId == ch).FirstOrDefault();
                    var usep = new UserSubcategoryEventPackage
                    {
                        UserSubcategoryEventId = use.Id,
                        PackageId = package.Id
                    };
                    _context.Add(usep);
                }
                await _context.SaveChangesAsync();
                return RedirectToAction("Details", "Packages", new { id = package.Id, eventid });
            }
            return NotFound();
        }

        // GET: PackagesController/CreatePackageService
        public async Task<ActionResult> CreatePackageService(int? id, int? eventid)
        {
            if (id == null || eventid == null)
            {
                return NotFound();
            }
            int selectedIndex = 0;
            var firsttype = await _context.ServiceTypes.FirstAsync();
            var firsttypeid = firsttype.Id;
            var showdates = await _context.ServiceTypes.Where(r => r.ShowDates == true).Select(res => res.Id).ToListAsync();
            string showdateids = "";
            foreach (var show in showdates)
            {
                showdateids += show + ",";
            }

            SelectList types = new SelectList(_context.ServiceTypes, "Id", "Name", selectedIndex);
            ViewBag.ServiceTypes = types;
            SelectList services = new SelectList(_context.Services.Where(r => r.ServiceTypeId == firsttypeid), "Id", "Name", selectedIndex);
            ViewBag.Services = services;
            var firstserviceid = Convert.ToInt32(services.Select(s => s.Value).First());
            ViewBag.PackageId = id;
            ViewBag.PackageName = await _context.Packages.Where(res => res.Id == id).Select(res => res.Name).FirstOrDefaultAsync();
            ViewBag.EventId = eventid;
            ViewBag.Quotas = await _context.Quotas.Where(q => q.ServiceId == firstserviceid).Select(r => new { r.StartDate, r.FinishDate, Num = r.AvailableNum }).ToListAsync();
            ViewBag.ShowDates = showdateids;
            return View();
        }

        public async Task<ActionResult> GetServices(int id)
        {
            var options = await _context.Services.Where(c => c.ServiceTypeId == id)?.Include(r => r.Quotas).OrderBy(res => res.Name).ToListAsync();
            if (!options.Any())
            {
                ViewBag.Quotas = null;
            }
            else
            {
                var firstserviceid = options.Select(s => s.Id).First();
                ViewBag.Quotas = await _context.Quotas.Where(q => q.ServiceId == firstserviceid)?.Select(r => new { r.StartDate, r.FinishDate, Num = r.AvailableNum }).ToListAsync();
            }
            return PartialView(options);
        }

        public async Task<ActionResult> GetQuotasForService(int id)
        {
            var options = await _context.Quotas.Where(c => c.ServiceId == id).ToListAsync();
            return PartialView(options);
        }

        // POST: PackagesController/CreatePackageService
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> CreatePackageService(int packageid, int eventid, [Bind("ServiceId,StartDate,FinishDate,MinimalDaysCount")] PackageService packageservice)
        {
            packageservice.PackageId = packageid;
            var serviceid = packageservice.ServiceId;
            if (ModelState.IsValid)
            {
                if (!await _context.PackageServices.Where(r => r.ServiceId == serviceid && r.PackageId == packageid).AnyAsync())
                {
                    _context.Add(packageservice);
                    await _context.SaveChangesAsync();
                    return RedirectToAction("Details", "Packages", new { id = packageid, eventid });
                }

            }
            return RedirectToAction("Details", "Packages", new { id = packageid, eventid });
        }



        // GET: PackagesController/EditPackageService/5
        public async Task<ActionResult> EditPackageService(int? id, int? packageid, int? eventid)
        {
            if (id == null)
            {
                return NotFound();
            }
            var model = await _context.PackageServices.Where(p => p.Id == id).Include(res => res.Service.ServiceType).Include(res => res.Service.Quotas).FirstOrDefaultAsync();
            ViewBag.PackageId = packageid;
            ViewBag.EventId = eventid;
            return View(model);
        }

        // POST: PackagesController/EditPackageService/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> EditPackageService(int id, int packageid, int eventid, [Bind("Id,PackageId,ServiceId,StartDate,FinishDate,MinimalDaysCount")] PackageService packageService)
        {
            if (id != packageService.Id)
            {
                return NotFound();
            }
            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(packageService);
                    await _context.SaveChangesAsync();
                }
                catch
                {
                    return NotFound();
                }
                return RedirectToAction("Details", "Packages", new { id = packageid, eventid });
            }
            return NotFound();
        }

        /// <summary>
        /// После изменения услуги в конструкторе пакета насильно обновляет услугу у всех, кто ее выбрал.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="packageid"></param>
        /// <param name="eventid"></param>
        /// <returns></returns>
        // GET: PackagesController/TracePackageService/5
        public async Task<ActionResult> TracePackageService(int? id, int? packageid, int? eventid)
        {
            if (id == null || packageid == null)
            {
                return NotFound();
            }
            //Все, кто бронировал данный пакет
            var allusers = await _context.UserPackageServices.Where(ups => ups.PackageService.PackageId == packageid).Select(res => res.UserId).Distinct().ToListAsync();
            var thispackageservice = await _context.PackageServices.Where(ps => ps.Id == id).FirstOrDefaultAsync();
            foreach (var userid in allusers)
            {
                //Услуга есть в хаброненом пакете, обновляем даты
                if (await _context.UserPackageServices.Where(ups => ups.UserId == userid && ups.PackageServiceId == id && ups.EventId == eventid).AnyAsync())
                {
                    var UPS = await _context.UserPackageServices.Where(ups => ups.UserId == userid && ups.PackageServiceId == id && ups.EventId == eventid).FirstOrDefaultAsync();
                    UPS.StartDate = thispackageservice.StartDate;
                    UPS.FinishDate = thispackageservice.FinishDate;
                    UPS.CreateDate = DateTime.Now;
                    await _context.SaveChangesAsync();
                }
                //Услуги нет в заброненном пакете, добавляем ее
                else
                {
                    //любая услуга из его пакета
                    var firstups = await _context.UserPackageServices.Where(ups => ups.UserId == userid && ups.EventId == eventid).FirstOrDefaultAsync();

                    var newuserps = new UserPackageService
                    {
                        User = await _context.Users.Where(us => us.Id == userid).FirstOrDefaultAsync(),
                        UserId = userid,
                        Event = await _context.Events.Where(ev => ev.Id == eventid).FirstOrDefaultAsync(),
                        PackageService = thispackageservice,
                        StartDate = thispackageservice.StartDate,
                        FinishDate = thispackageservice.FinishDate,
                        Status = firstups.Status,
                        Creator = firstups.Creator
                    };
                    await _context.UserPackageServices.AddAsync(newuserps);
                    await _context.SaveChangesAsync();
                }
            }

            return RedirectToAction("Details", new { eventid, id = packageid });
        }


        // GET: PackagesController/Delete/5
        public async Task<ActionResult> Delete(int? id, int? eventid)
        {
            if (id == null)
            {
                return NotFound();
            }
            ViewBag.EventId = eventid;
            var model = await _context.Packages.Where(r => r.Id == id).FirstOrDefaultAsync();
            return View(model);
        }

        // POST: PackagesController/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Delete(int id, int eventid)
        {
            try
            {
                var package = await _context.Packages.Where(ho => ho.Id == id).FirstOrDefaultAsync();
                _context.Remove(package);
                await _context.SaveChangesAsync();
                return RedirectToAction("List", "Packages", new { eventid });
            }
            catch
            {
                return View();
            }

        }

        // GET: PackagesController/DeletePackageService/5
        public async Task<ActionResult> DeletePackageService(int? id, int? packageid, int? eventid)
        {
            if (id == null)
            {
                return NotFound();
            }
            ViewBag.PackageId = packageid;
            ViewBag.EventId = eventid;
            var model = await _context.PackageServices.Where(x => x.Id == id).Include(res => res.Service.ServiceType).Include(res => res.Package).FirstOrDefaultAsync();
            return View(model);
        }

        // POST: PackagesController/DeletePackageService/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeletePackageService(int id, int packageid, int eventid)
        {
            try
            {
                var packageservice = await _context.PackageServices.Where(ho => ho.Id == id).FirstOrDefaultAsync();
                _context.Remove(packageservice);
                await _context.SaveChangesAsync();
                return RedirectToAction("Details", "Packages", new { id = packageid, eventid });
            }
            catch
            {
                return View();
            }
        }
    }
}
