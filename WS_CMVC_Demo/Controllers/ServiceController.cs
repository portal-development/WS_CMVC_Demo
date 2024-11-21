using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WS_CMVC_Demo.Data;
using WS_CMVC_Demo.Models;
using WS_CMVC_Demo.Models.Service;

namespace WS_CMVC_Demo.Controllers
{
    [Authorize(Roles = "administrator")]
    public class ServiceController : Controller
    {

        private readonly ApplicationDbContext _context;
        private readonly ILogger _logger;
        private readonly UserManager<ApplicationUser> _userManager;

        public ServiceController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, ILoggerFactory loggerFactory)
        {
            _context = context;
            _userManager = userManager;
            _logger = loggerFactory.CreateLogger<ServiceController>();
        }


        // GET: ServiceController
        public async Task<ActionResult> Index()
        {
            var services = await _context.Services
                .Include(r => r.HotelOption.Hotel)
                .Include(r => r.ServiceType)
                .Include(r => r.Quotas)
                .Include(r => r.BadgeService)
                .OrderBy(res => res.ServiceTypeId)
                .ToListAsync();
            return View(services);
        }

        // GET: ServiceController/Details/5
        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            ViewBag.ServiceName = await _context.Services.Where(ho => ho.Id == id).Select(res => res.Name).FirstOrDefaultAsync();
            ViewBag.ServiceId = id;
            var quotas = await _context.Quotas.Where(hop => hop.ServiceId == id).Include(r => r.Service).ToListAsync();
            return View(quotas);
        }

        // GET: ServiceController/Create
        public async Task<ActionResult> Create()
        {
            ViewBag.ServiceTypes = new SelectList(_context.ServiceTypes, "Id", "Name");
            var hotels = await _context.Hotels.OrderBy(res => res.Name).ToListAsync();
            ViewBag.Hotels = new SelectList(hotels, "Id", "Name");
            var firstHotelId = hotels.FirstOrDefault()?.Id ?? 0;
            ViewBag.HotelOptions = new SelectList(_context.HotelOptions.Where(c => c.HotelId == firstHotelId).OrderBy(res => res.Name), "Id", "Name");
            ViewBag.LivingId = await _context.ServiceTypes.Where(re => re.Name == "Проживание").Select(res => res.Id).FirstOrDefaultAsync();
            ViewBag.BadgeServices = new SelectList(_context.BadgeServices.OrderBy(c => c.Order), "Id", "Title");
            return View();
        }

        public async Task<ActionResult> GetHotelOptions(int id)
        {
            var options = await _context.HotelOptions.Where(c => c.HotelId == id).OrderBy(res => res.Name).ToListAsync();
            return PartialView(options);
        }

        // POST: ServiceController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind("ServiceTypeId,Name,Description,HotelOptionId")] Service service)
        {
            if (ModelState.IsValid)
            {
                var livingid = await _context.ServiceTypes.Where(re => re.Name == "Проживание").Select(res => res.Id).FirstOrDefaultAsync();
                if (service.ServiceTypeId != livingid)
                {
                    service.HotelOptionId = null;
                }
                _context.Add(service);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewBag.ServiceTypes = new SelectList(_context.ServiceTypes, "Id", "Name", service.ServiceType);
            var hotels = await _context.Hotels.OrderBy(res => res.Name).ToListAsync();
            ViewBag.Hotels = new SelectList(hotels, "Id", "Name");
            var firstHotelId = hotels.FirstOrDefault()?.Id ?? 0;
            ViewBag.HotelOptions = new SelectList(_context.HotelOptions.Where(c => c.HotelId == firstHotelId).OrderBy(res => res.Name), "Id", "Name");
            ViewBag.LivingId = await _context.ServiceTypes.Where(re => re.Name == "Проживание").Select(res => res.Id).FirstOrDefaultAsync();
            ViewBag.BadgeServices = new SelectList(_context.BadgeServices.OrderBy(c => c.Order), "Id", "Title", service.BadgeServiceId);
            return View(service);
        }

        // GET: ServiceController/Edit/5
        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var service = await _context.Services.Where(ho => ho.Id == id).Include(r => r.HotelOption.Hotel).Include(r => r.ServiceType).FirstOrDefaultAsync();
            SelectList types = new SelectList(_context.ServiceTypes, "Id", "Name", service.ServiceTypeId);
            ViewBag.ServiceTypes = types;
            var ishotel = service.ServiceType.Name == "Проживание" ? true : false;
            ViewBag.IsHotel = ishotel;
            var hotelselectedindex = service.HotelOption?.HotelId;
            SelectList hotels = new SelectList(_context.Hotels.OrderBy(res => res.Name), "Id", "Name", hotelselectedindex);
            ViewBag.Hotels = hotels;
            SelectList hoteloptions = new SelectList(_context.HotelOptions.Where(c => c.HotelId == hotelselectedindex).OrderBy(res => res.Name), "Id", "Name", service.HotelOptionId);
            ViewBag.HotelOptions = hoteloptions;
            var LivingId = await _context.ServiceTypes.Where(re => re.Name == "Проживание").Select(res => res.Id).FirstOrDefaultAsync();
            ViewBag.LivingId = LivingId;
            ViewBag.BadgeServices = new SelectList(_context.BadgeServices.OrderBy(c => c.Order), "Id", "Title");
            return View(service);
        }

        // POST: ServiceController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(int id, [Bind("Id,ServiceTypeId,Name,Description,HotelOptionId,BadgeServiceId")] Service service)
        {
            if (id != service.Id)
            {
                return NotFound();
            }
            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(service);
                    await _context.SaveChangesAsync();
                }
                catch
                {
                    return NotFound();
                }
                return RedirectToAction(nameof(Index));
            }
            return NotFound();
        }

        // GET: ServiceController/Delete/5
        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var service = await _context.Services.Include(r => r.HotelOption).Include(r => r.ServiceType).FirstOrDefaultAsync(res => res.Id == id);
            return View(service);
        }

        // POST: ServiceController/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Delete(int id)
        {
            try
            {
                var service = await _context.Services.Where(ho => ho.Id == id).FirstOrDefaultAsync();
                _context.Services.Remove(service);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return NotFound();
            }
        }
    }
}
