using System.Linq;
using System.Threading.Tasks;
using Harvest.Core.Data;
using Harvest.Core.Domain;
using Harvest.Core.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Harvest.Web.Controllers
{
    [Authorize(Policy = AccessCodes.FieldManagerAccess)]
    public class RequestController : SuperController
    {
        private readonly AppDbContext _dbContext;

        public RequestController(AppDbContext dbContext)
        {
            this._dbContext = dbContext;
        }

        // Get info on the project as well as outstanding quotes (in case we want to edit an in-progess quote)
        public ActionResult Get(int id)
        {
            return Ok();
        }

        // Create a quote for project ID
        [HttpGet]
        public ActionResult Create(int id)
        {
            return View();
        }
    }
}
