using Harvest.Core.Data;
using Harvest.Core.Domain;
using Harvest.Core.Models;
using Harvest.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace Harvest.Web.Controllers.Api
{
    public class MobileController : Controller
    {
        private readonly AppDbContext _dbContext;
        private readonly IApiKeyService _apiKeyService;
        public MobileController(AppDbContext appDbContext, IApiKeyService apiKeyService)
        {
            _apiKeyService = apiKeyService;
            _dbContext = appDbContext;
        }
        public async Task<IActionResult> Projects()
        {
            var permission = await ValidateApiKeyFromHeader();
            if (permission == null)
            {
                return Unauthorized();
            }

            permission = await _dbContext.Permissions.Include(a => a.Team).Include(a => a.User).Where(a => a.Id == permission.Id).SingleOrDefaultAsync();

            if (permission.Team == null)
            {
                return Unauthorized();
            }

            var projects = await _dbContext.Projects
                .Where(p => p.TeamId == permission.TeamId && p.IsActive && p.Status == Project.Statuses.Active)
                .Select(p => new
                {
                    p.Id,
                    p.Name,                    
                })
                .ToListAsync();
            return Ok(projects);
        }


        private async Task<Permission> ValidateApiKeyFromHeader()
        {
            var apiKey = GetApiKeyFromHeader();

            if (string.IsNullOrEmpty(apiKey))
            {
                return null;
            }

            return await _apiKeyService.ValidateApiKeyAsync(apiKey);
        }

        private string GetApiKeyFromHeader()
        {
            if (!Request.Headers.TryGetValue("Authorization", out var authHeader))
            {
                return null;
            }

            var authValue = authHeader.ToString();

            // Support both "Bearer {key}" and just "{key}" formats
            if (authValue.StartsWith("Bearer ", System.StringComparison.OrdinalIgnoreCase))
            {
                return authValue.Substring(7); // Remove "Bearer " prefix
            }

            return authValue;
        }
    }
}
