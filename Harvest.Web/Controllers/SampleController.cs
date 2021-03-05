using System.Linq;
using Harvest.Core.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Harvest.Web.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [Authorize]
    public class SampleController : ControllerBase
    {
        private readonly AppDbContext dbContext;

        public SampleController(AppDbContext dbContext)
        {
            this.dbContext = dbContext;
        }
        public ActionResult List()
        {
            return Ok(new { Me = User.Identity.Name, Users = dbContext.Users.ToArray() });
        }
    }
}
