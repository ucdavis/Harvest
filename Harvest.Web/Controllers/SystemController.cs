using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Harvest.Core.Data;
using Harvest.Core.Models;
using Harvest.Web.Models;
using Harvest.Web.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace Harvest.Web.Controllers
{
    [Authorize(Policy = AccessCodes.SystemAccess)]
    public class SystemController : SuperController
    {
        private readonly AppDbContext _dbContext;
        private readonly IIdentityService _identityService;
        public const string IamIdClaimType = "ucdPersonIAMID";

        public SystemController(AppDbContext dbContext, IIdentityService identityService)
        {
            _dbContext = dbContext;
            _identityService = identityService;
        }

        [HttpGet]
        public IActionResult Emulate()
        {
            return View();
        }

        [HttpPost]
        [Authorize()]
        public async Task<IActionResult> Emulate(EmulateUserViewModel model)
        {
            Log.Information($"Emulation attempted for {model.UserEmail} by {User.Identity.Name}");
            var lookupVal = model.UserEmail.Trim();

            var user = await  _dbContext.Users.SingleOrDefaultAsync(u => u.Email == lookupVal || u.Kerberos == lookupVal);
            if (user == null)
            {
                user = model.UserEmail.Contains("@")
                    ? await _identityService.GetByEmail(model.UserEmail)
                    : await _identityService.GetByKerberos(model.UserEmail);
            }
            if (user == null)
            {
                return View(model);
            }
            

            var identity = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Kerberos),
                new Claim(ClaimTypes.Name, user.Kerberos),
                new Claim(ClaimTypes.GivenName, user.FirstName),
                new Claim(ClaimTypes.Surname, user.LastName),
                new Claim("name", user.Name),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(IamIdClaimType, user.Iam),
            }, CookieAuthenticationDefaults.AuthenticationScheme);

            // kill old login
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            // create new login
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity));

            return RedirectToAction("Index", "Home");
        }
    }
}
