using System.Linq;
using System.Threading.Tasks;
using Harvest.Core.Data;
using Harvest.Core.Domain;
using Harvest.Core.Models;
using Harvest.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Harvest.Web.Controllers
{
    [Authorize(Policy = AccessCodes.FieldManagerAccess)]
    public class RequestController : Controller
    {
        private readonly AppDbContext _dbContext;

        public RequestController(AppDbContext dbContext)
        {
            this._dbContext = dbContext;
        }

        // Get info on the project as well as current proposed quote
        [HttpGet]
        public async Task<ActionResult> Get(int id)
        {
            return Ok();
        }

        // create a new request via react
        [HttpGet]
        public ActionResult Create()
        {
            return View("React");
        }
    }
}