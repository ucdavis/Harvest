using System.Linq;
using System.Threading.Tasks;
using Harvest.Core.Data;
using Harvest.Core.Models;
using Harvest.Core.Services;
using Harvest.Email.Services;
using Harvest.Email.Views.Emails;
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
        private readonly IEmailBodyService _emailBodyService;

        public HomeController(IUserService userService, INotificationService notificationService, IEmailBodyService emailBodyService)
        {
            _userService = userService;
            _notificationService = notificationService;
            _emailBodyService = emailBodyService;
        }
        public ActionResult Index()
        {
            return View();
        }


        public async Task<IActionResult> TestBody()
        {
            var xxx = await _emailBodyService.RenderBody("/Views/Emails/TestEmail.cshtml", new TestEmailModel());

            return Content(xxx);
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
