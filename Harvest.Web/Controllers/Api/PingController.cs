using Harvest.Core.Data;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Harvest.Web.Controllers.Api
{

    public class PingController : Controller
    {
        private readonly AppDbContext _dbContext;

        public PingController(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        [Route("api/ping")]
        [ResponseCache(Duration = 300)]
        public async Task<ActionResult> Index()
        {
            if (await _dbContext.Database.CanConnectAsync())
            {
                return Content("Ping");
            }

            return BadRequest();

        }
    }
}
