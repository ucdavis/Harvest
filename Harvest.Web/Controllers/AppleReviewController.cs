using Harvest.Core.Data;
using Harvest.Core.Domain;
using Harvest.Web.Models.Settings;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Harvest.Web.Controllers
{
    public class AppleReviewController : Controller
    {
        private readonly AppDbContext _dbContext;
        private readonly AuthSettings _authSettings;

        public AppleReviewController(AppDbContext dbContext, IOptions<AuthSettings> authSettings)
        {
            _dbContext = dbContext;
            _authSettings = authSettings.Value;
        }

        /// <summary>
        /// Provides a login link for the iPhone app review process without needing CAS or DUO
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public async Task<IActionResult> LoginIPhone(string key)
        {
            if (string.IsNullOrEmpty(_authSettings.IPhoneKey) || _authSettings.IPhoneKey == "[External]")
            {
                throw new InvalidOperationException("IPhone configuration is required");
            }

            if (string.IsNullOrWhiteSpace(key))
            {
                return BadRequest();
            }

            if(key != _authSettings.IPhoneKey)
            {
                return Unauthorized();
            }

            var user = await _dbContext.Users.Where(a => a.Kerberos == "xxxx").SingleOrDefaultAsync();
            if (user == null)
            {
                return NotFound();
            }
            var validRoles = new List<string> { Role.Codes.Worker, Role.Codes.FieldManager, Role.Codes.Supervisor };


            var permission = await _dbContext.Permissions
                .Where(p => p.UserId == user.Id && p.Team.Slug == "caes" && validRoles.Contains(p.Role.Name))
                .SingleOrDefaultAsync();
            if (permission == null)
            {
                return NotFound();
            }

            var baseUrl = $"{Request.Scheme}://{Request.Host}";
            //Check base url is for test only: https://harvest-test.azurewebsites.net
            if(!baseUrl.Equals("https://harvest-test.azurewebsites.net", StringComparison.OrdinalIgnoreCase) )
            {
                return BadRequest("This endpoint can only be used in test environment");
            }

            permission.Token = Guid.NewGuid();
            permission.TokenExpires = DateTime.UtcNow.AddMinutes(5);

            await _dbContext.SaveChangesAsync();

            
            var appLink = $"harvestmobile://applink?code={permission.Token}&baseUrl={Uri.EscapeDataString(baseUrl)}";
            return Redirect(appLink);
        }
    }
}
