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
    public class HotelOptionsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger _logger;
        private readonly UserManager<ApplicationUser> _userManager;

        public HotelOptionsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, ILoggerFactory loggerFactory)
        {
            _context = context;
            _userManager = userManager;
            _logger = loggerFactory.CreateLogger<HotelOptionsController>();
        }

        // GET: HotelOptionsController/Create/5
        public ActionResult Create(int id)
        {
            ViewBag.HotelId = id;
            return View();
        }

        // POST: HotelOptionsController/Create/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(int id, [Bind("Name,Description,NumberOfSeats")] HotelOption hoteloption)
        {
            hoteloption.HotelId = id;
            if (ModelState.IsValid)
            {
                _context.Add(hoteloption);
                await _context.SaveChangesAsync();
                return RedirectToAction("Details", "Hotels", new { id });
            }
            return View(hoteloption);
        }

        // GET: HotelOptionsController/Edit/5
        public async Task<ActionResult> Edit(int? id, int? hotelid)
        {
            ViewBag.HotelId = hotelid;
            if (id == null)
            {
                return NotFound();
            }
            var hoteloption = await _context.HotelOptions.Where(ho => ho.Id == id).FirstOrDefaultAsync();
            return View(hoteloption);
        }

        // POST: HotelOptionsController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(int id, [Bind("Id,HotelId,Name,Description,NumberOfSeats")] HotelOption hoteloption)
        {
            if (id != hoteloption.Id)
            {
                return NotFound();
            }
            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(hoteloption);
                    await _context.SaveChangesAsync();
                }
                catch
                {
                    return NotFound();
                }
                return RedirectToAction("Details", "Hotels", new { id = hoteloption.HotelId });
            }
            return NotFound();
        }

        // GET: HotelOptionsController/Delete/5
        public async Task<ActionResult> Delete(int? id, int? hotelid)
        {
            ViewBag.HotelId = hotelid;
            if (id == null)
            {
                return NotFound();
            }
            var hoteloption = await _context.HotelOptions.Where(ho => ho.Id == id).FirstOrDefaultAsync();
            return View(hoteloption);
        }

        // POST: HotelOptionsController/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Delete(int id, int hotelid)
        {
            try
            {
                var hoteloption = await _context.HotelOptions.Where(ho => ho.Id == id).FirstOrDefaultAsync();
                _context.HotelOptions.Remove(hoteloption);
                await _context.SaveChangesAsync();
                return RedirectToAction("Details", "Hotels", new { id = hotelid });
            }
            catch
            {
                return NotFound();
            }
        }
    }
}
