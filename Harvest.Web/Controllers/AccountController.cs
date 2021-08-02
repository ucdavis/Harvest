using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Harvest.Web.Models.Settings;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

namespace Harvest.Web.Controllers
{
    [ApiExplorerSettings(IgnoreApi = true)]
    public class AccountController : Controller
    {
        private readonly AuthSettings _authenticationSettings;

        public AccountController(IOptions<AuthSettings> authenticationSettings)
        {
            _authenticationSettings = authenticationSettings.Value;
        }
        public IActionResult AccessDenied()
        {
            return View();
        }

        public async Task<IActionResult> Logout() 
        {
            await HttpContext.SignOutAsync();
            return Redirect($"{_authenticationSettings.Authority}/logout"); //This clears out all the sessions....
        }
        public async Task<IActionResult> EndEmulate() 
        {

            await HttpContext.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }
        
    }
}
