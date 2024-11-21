using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net;
using WS_CMVC_Demo.Data;
using WS_CMVC_Demo.Services;

namespace WS_CMVC_Demo.Controllers
{
    /// <summary>
    /// Контроллер для отчетов о доставке смс
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class SmsCallbackController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger _logger;

        /// <summary>
        /// Список ip адресов мегафона с которых разрешены отправки отчетов
        /// </summary>
        private static readonly string[] _whiteList = { "::1", "127.0.0.1", "193.232.168.64", "193.232.168.65", "193.232.168.66", "193.232.168.67", "193.232.168.68", "193.232.168.69", "193.232.168.70", "193.232.168.71", "193.232.168.72", "193.232.168.73", "193.232.168.74", "193.232.168.75", "193.232.168.76", "193.232.168.77", "193.232.168.78", "193.232.168.79", "193.232.168.80", "193.232.168.81", "193.232.168.82", "193.232.168.83", "193.232.168.84", "193.232.168.85", "193.232.168.86", "193.232.168.87", "193.232.168.88", "193.232.168.89", "193.232.168.90", "193.232.168.91", "193.232.168.92", "193.232.168.93", "193.232.168.94", "193.232.168.95", "193.232.168.176", "193.232.168.177", "193.232.168.178", "193.232.168.179", "193.232.168.180", "193.232.168.181", "193.232.168.182", "193.232.168.183", "193.232.168.184", "193.232.168.185", "193.232.168.186", "193.232.168.187", "193.232.168.188", "193.232.168.189", "193.232.168.190", "193.232.168.191", "193.232.168.218", "193.232.168.219" };

        public SmsCallbackController(ApplicationDbContext context, ILoggerFactory loggerFactory)
        {
            _context = context;
            _logger = loggerFactory.CreateLogger<AccountController>();
        }

    }
}
