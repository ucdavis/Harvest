using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace Harvest.Web.Controllers
{
    [Authorize]
    public class HelpController : SuperController
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
