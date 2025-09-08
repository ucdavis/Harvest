using Harvest.Core.Data;
using Harvest.Core.Domain;
using Harvest.Core.Models;
using Harvest.Core.Services;
using Harvest.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Harvest.Web.Controllers.Api
{
    [Authorize]
    public class LinkController : SuperController
    {
        private readonly AppDbContext _dbContext;
        private readonly IUserService _userService;
        private readonly IApiKeyService _apiKeyService;
        public LinkController(AppDbContext dbContext, IUserService userService, IApiKeyService apiKeyService)
        {
            _dbContext = dbContext;
            _userService = userService;
            _apiKeyService = apiKeyService;
        }

        [HttpGet]
        [Authorize(Policy = AccessCodes.WorkerAccess)]
        [Route("api/{team}/link")]
        public async Task<IActionResult> Get()
        {
            var user = await _userService.GetCurrentUser();


            var permission = await _dbContext.Permissions
                .Where(p => p.UserId == user.Id && p.Team.Slug == TeamSlug && p.Role.Name == Role.Codes.Worker)
                .SingleOrDefaultAsync();
            if (permission == null)
            {
                return NotFound();
            }
            permission.Token = Guid.NewGuid();
            permission.TokenExpires = DateTime.UtcNow.AddMinutes(5);

            await _dbContext.SaveChangesAsync();

            return Ok(permission.Token);
        }

        [HttpGet]
        [AllowAnonymous]
        [Route("api/getapi/{id}")]
        public async Task<IActionResult> GetApi(Guid id)
        {
            var permission = await _dbContext.Permissions
                .Include(p => p.Team)
                .Where(p => p.Token == id && p.TokenExpires > DateTime.UtcNow)
                .SingleOrDefaultAsync();
            if (permission == null)
                {
                return NotFound();
            }
            var apiKey = await _apiKeyService.GenerateApiKeyAsync(permission.Id);
            permission.Token = null;
            permission.TokenExpires = null;
            await _dbContext.SaveChangesAsync();
            return Ok(new { apiKey, team = permission.Team.Slug });
        }

        //Can use this for testing
        //[HttpGet]
        //[AllowAnonymous]
        //[Route("api/validate/{id}")]
        //public async Task<IActionResult> Validate(string id)
        //{
        //    var permission = await _apiKeyService.ValidateApiKeyAsync(id);
        //    if (permission == null)
        //    {
        //        return NotFound();
        //    }
        //    return Ok(new { valid = true });
        //}

    }
}
