using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WS_CMVC_Demo.Controllers
{
    [Authorize(Roles = "administrator,contracter,moderator")]
    public class AdminPanelController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }
    }
}
