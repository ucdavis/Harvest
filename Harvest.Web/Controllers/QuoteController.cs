using System.Linq;
using System.Threading.Tasks;
using Harvest.Core.Data;
using Harvest.Core.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Harvest.Web.Controllers
{
    // Must be field manager (or possibly supervisors can make but not submit quotes)
    [ApiController]
    [Route("[controller]")]
    [Authorize]
    public class QuoteController : ControllerBase
    {
        private readonly AppDbContext _dbContext;

        public QuoteController(AppDbContext dbContext)
        {
            this._dbContext = dbContext;
        }

        // Get info on the project as well as outstanding quotes (in case we want to edit an in-progess quote)
        [HttpGet("{id?}")]
        public async Task<ActionResult> Get(int id)
        {
            var project = await _dbContext.Projects.SingleAsync(p => p.Id == id);
            var quotes = await _dbContext.Quotes.Where(q => q.ProjectId == id && q.ApprovedOn == null).ToArrayAsync();

            var model = new QuoteModel { Project = project, Quotes = quotes };

            return Ok(model);
        }
    }

    public class QuoteModel
    {
        public Project Project { get; set; }
        public Quote[] Quotes { get; set; }
    }
}
