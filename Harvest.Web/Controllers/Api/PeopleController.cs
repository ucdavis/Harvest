using System.Threading.Tasks;
using Harvest.Core.Data;
using Harvest.Core.Domain;
using Harvest.Core.Models;
using Harvest.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Harvest.Web.Controllers.Api
{
    [Authorize()]
    public class PeopleController : Controller
    {
        private readonly AppDbContext _dbContext;
        private readonly IIdentityService _identityService;

        public PeopleController(AppDbContext dbContext, IIdentityService identityService)
        {
            this._dbContext = dbContext;
            this._identityService = identityService;
        }

        // Search people based on kerb or email
        [HttpGet("/people/search")]
        public async Task<ActionResult> Search(string query)
        {
            User user;

            if (query.Contains('@'))
            {
                user = await _identityService.GetByEmail(query);
            }
            else
            {
                user = await _identityService.GetByKerberos(query);
            }

            return Ok(user);
        }
    }
}