using System;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Harvest.Core.Data;
using Harvest.Core.Domain;
using Harvest.Core.Models;
using Harvest.Core.Services;
using Harvest.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Harvest.Web.Controllers
{
    [Authorize(Policy = AccessCodes.FieldManagerAccess)]
    public class QuoteController : Controller
    {
        private readonly AppDbContext _dbContext;
        private readonly IUserService _userService;

        public QuoteController(AppDbContext dbContext, IUserService userService)
        {
            this._dbContext = dbContext;
            this._userService = userService;
        }

        // Get info on the project as well as in-progess quote if it exists
        [HttpGet]
        public async Task<ActionResult> Get(int id)
        {
            var project = await _dbContext.Projects.Include(p => p.PrincipalInvestigator).Include(p => p.CreatedBy).SingleAsync(p => p.Id == id);
            var openQuote = await _dbContext.Quotes.Where(q => q.ProjectId == id && q.ApprovedOn == null).Select(q => QuoteDetail.Deserialize(q.Text)).SingleOrDefaultAsync();

            var model = new QuoteModel { Project = project, Quote = openQuote };

            return Ok(model);
        }

        // Create a quote for project ID
        [HttpGet]
        public ActionResult Create(int id)
        {
            return View("React");
        }

        [HttpPost]
        public async Task<ActionResult> Save(int id, [FromBody] QuoteDetail quoteDetail)
        {
            // Use existing quote if it exists, otherwise create new one
            var quote = await _dbContext.Quotes.Where(q => q.ProjectId == id && q.ApprovedOn == null).SingleOrDefaultAsync();

            if (quote == null)
            {
                quote = new Quote();

                // newly created quote, populate defaults
                var currentUser = await this._userService.GetCurrentUser();
                quote.InitiatedById = currentUser.Id;
                quote.Status = "New"; // TODO: definte status progression
                quote.CreatedDate = DateTime.UtcNow;
                quote.ProjectId = id;

                await _dbContext.Quotes.AddAsync(quote);
            }

            quote.Total = (decimal)Math.Round(quoteDetail.GrandTotal, 2);
            quote.Text = QuoteDetail.Serialize(quoteDetail);

            await _dbContext.SaveChangesAsync();

            return Ok(quote);
        }
    }
}
