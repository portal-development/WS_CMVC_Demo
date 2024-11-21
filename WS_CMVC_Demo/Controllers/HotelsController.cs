using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WS_CMVC_Demo.Data;
using WS_CMVC_Demo.Models;
using WS_CMVC_Demo.Models.Service;

namespace WS_CMVC_Demo.Controllers
{
    [Authorize(Roles = "administrator")]
    public class HotelsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger _logger;
        private readonly UserManager<ApplicationUser> _userManager;

        public HotelsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, ILoggerFactory loggerFactory)
        {
            _context = context;
            _userManager = userManager;
            _logger = loggerFactory.CreateLogger<HotelsController>();
        }


        // GET: Hotels
        public async Task<ActionResult> Index()
        {
            var hotels = await _context.Hotels.Include(r => r.HotelOptions).ToListAsync();
            return View(hotels);
        }

        // GET: HotelsController/Details/5
        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            ViewBag.HotelName = await _context.Hotels.Where(ho => ho.Id == id).Select(res => res.Name).FirstOrDefaultAsync();
            ViewBag.HotelId = id;
            var hoteloptions = await _context.HotelOptions.Where(hop => hop.Hotel.Id == id).ToListAsync();
            return View(hoteloptions);
        }

        // GET: Hotels/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Hotels/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind("Name,CountOfStars,Address")] Hotel hotel)
        {
            if (ModelState.IsValid)
            {
                _context.Add(hotel);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(hotel);
        }

        // GET: Hotels/Edit/5
        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var hotel = await _context.Hotels.Where(ho => ho.Id == id).FirstOrDefaultAsync();
            return View(hotel);
        }

        // POST: Hotels/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(int id, [Bind("Id,Name,CountOfStars,Address")] Hotel hotel)
        {
            if (id != hotel.Id)
            {
                return NotFound();
            }
            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(hotel);
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

        // GET: Hotels/Delete/5
        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var hotel = await _context.Hotels.Where(ho => ho.Id == id).FirstOrDefaultAsync();
            return View(hotel);
        }

        // POST: Hotels/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Delete(int id)
        {

            try
            {
                var hotel = await _context.Hotels.Where(ho => ho.Id == id).FirstOrDefaultAsync();
                _context.Hotels.Remove(hotel);
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
