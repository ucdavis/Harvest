using System.Linq;
using Harvest.Core.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Harvest.Web.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Spa() {
            return View();
        }

        public ActionResult Map() {
            return View();
        }
    }
}
