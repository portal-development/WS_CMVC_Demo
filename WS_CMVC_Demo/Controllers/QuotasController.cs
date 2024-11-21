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
    public class QuotasController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger _logger;
        private readonly UserManager<ApplicationUser> _userManager;

        public QuotasController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, ILoggerFactory loggerFactory)
        {
            _context = context;
            _userManager = userManager;
            _logger = loggerFactory.CreateLogger<QuotasController>();
        }

        // GET: QuotasController/Create
        public async Task<ActionResult> Create(int id)
        {
            ViewBag.ServiceId = id;
            ViewBag.ServiceName = await _context.Services.Where(r => r.Id == id).Select(res => res.Name).FirstOrDefaultAsync();
            return View();
        }

        // POST: QuotasController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(int id, [Bind("StartDate,FinishDate,AvailableNum,Cost,FixedCost")] Quota quota)
        {
            quota.ServiceId = id;
            if (ModelState.IsValid)
            {
                _context.Add(quota);
                await _context.SaveChangesAsync();
                return RedirectToAction("Details", "Service", new { id });
            }
            return View(quota);
        }

        // GET: QuotasController/Edit/5
        public async Task<ActionResult> Edit(int? id, int? serviceid)
        {
            ViewBag.ServiceId = serviceid;
            if (id == null)
            {
                return NotFound();
            }
            var quota = await _context.Quotas.Where(ho => ho.Id == id).Include(re => re.Service).FirstOrDefaultAsync();
            return View(quota);
        }

        // POST: QuotasController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(int id, int serviceid, [Bind("Id,ServiceId,StartDate,FinishDate,AvailableNum,Cost,FixedCost")] Quota quota)
        {
            if (id != quota.Id || serviceid != quota.ServiceId)
            {
                return NotFound();
            }
            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(quota);
                    await _context.SaveChangesAsync();
                }
                catch
                {
                    return NotFound();
                }
                return RedirectToAction("Details", "Service", new { id = serviceid });
            }
            return NotFound();
        }

        // GET: QuotasController/Delete/5
        public async Task<ActionResult> Delete(int? id, int? serviceid)
        {
            ViewBag.ServiceId = serviceid;
            if (id == null)
            {
                return NotFound();
            }
            var quota = await _context.Quotas.Where(ho => ho.Id == id).Include(r => r.Service).FirstOrDefaultAsync();
            return View(quota);
        }

        // POST: QuotasController/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Delete(int id, int serviceid)
        {
            try
            {
                var quota = await _context.Quotas.Where(ho => ho.Id == id).FirstOrDefaultAsync();
                _context.Quotas.Remove(quota);
                await _context.SaveChangesAsync();
                return RedirectToAction("Details", "Service", new { id = serviceid });
            }
            catch
            {
                return NotFound();
            }
        }
    }
}
