using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Harvest.Web.Controllers
{
    [Authorize]
    public class HomeController : SuperController
    {
        public ActionResult Index()
        {
            System.Console.WriteLine(HttpContext.Request.Protocol);
            return View("React");
        }
    }
}
