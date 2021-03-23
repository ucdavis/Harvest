using System.Linq;
using System.Threading.Tasks;
using Harvest.Core.Data;
using Harvest.Core.Models;
using Harvest.Core.Services;
using Harvest.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Harvest.Web.Controllers
{
    [Authorize]
    public class HomeController : SuperController
    {
        private readonly IUserService _userService;
        private readonly INotificationService _notificationService;

        public HomeController(IUserService userService, INotificationService notificationService)
        {
            _userService = userService;
            _notificationService = notificationService;
        }
        public ActionResult Index()
        {
            return View();
        }
        [Authorize(Policy = AccessCodes.SystemAccess)]
        public async Task<IActionResult> TestEmail()
        {
            var user = await _userService.GetCurrentUser();

            await _notificationService.SendSampleNotificationMessage(user.Email);
            return Content("Done. Maybe. Well, possibly. If you don't get it, check the settings.");
        }

        public ActionResult Spa() {
            return View();
        }

        public ActionResult Map() {
            return View();
        }
    }
}
