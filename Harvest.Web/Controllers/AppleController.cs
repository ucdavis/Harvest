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
    public class AppleController : Controller
    {
        private readonly AppDbContext _dbContext;
        private readonly AuthSettings _authSettings;

        public AppleController(AppDbContext dbContext, IOptions<AuthSettings> authSettings)
        {
            _dbContext = dbContext;
            _authSettings = authSettings.Value;
            if (string.IsNullOrEmpty(_authSettings.IPhoneKey) || _authSettings.IPhoneKey == "[External]")
            {
                throw new InvalidOperationException("IPhone configuration is required");
            }
        }
        public async Task<IActionResult> LoginIPhone(string key)
        {           
            //TODO: Key Checks
            if(string.IsNullOrWhiteSpace(key))
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

            permission.Token = Guid.NewGuid();
            permission.TokenExpires = DateTime.UtcNow.AddMinutes(5);

            await _dbContext.SaveChangesAsync();

            var baseUrl = $"{Request.Scheme}://{Request.Host}";
            var appLink = $"harvestmobile://applink?code={permission.Token}&baseUrl={Uri.EscapeDataString(baseUrl)}";
            return Redirect(appLink);
        }
    }
}
