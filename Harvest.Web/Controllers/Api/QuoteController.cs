using System;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Harvest.Core.Data;
using Harvest.Core.Domain;
using Harvest.Core.Models;
using Harvest.Core.Services;
using Harvest.Web.Models;
using Harvest.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Harvest.Web.Controllers
{
    [Authorize(Policy = AccessCodes.PrincipalInvestigator)]
    public class QuoteController : Controller
    {
        private readonly AppDbContext _dbContext;
        private readonly IUserService _userService;
        private readonly IProjectHistoryService _historyService;
        private readonly IEmailService _emailService;

        public QuoteController(AppDbContext dbContext, IUserService userService, IProjectHistoryService historyService, IEmailService emailService)
        {
            _dbContext = dbContext;
            _userService = userService;
            _historyService = historyService;
            _emailService = emailService;
        }

        // Get info on the project as well as in-progess quote if it exists
        [HttpGet]
        public async Task<ActionResult> Get(int projectId)
        {
            var project = await _dbContext.Projects.Include(p => p.PrincipalInvestigator).Include(p => p.Accounts).Include(p => p.CreatedBy).SingleAsync(p => p.Id == projectId);
            var openQuote = await _dbContext.Quotes.Where(q => q.ProjectId == projectId && q.ApprovedOn == null).Select(q => QuoteDetail.Deserialize(q.Text)).SingleOrDefaultAsync();

            var model = new QuoteModel { Project = project, Quote = openQuote };

            return Ok(model);
        }

        // Create a quote for project ID
        [Authorize(Policy = AccessCodes.SupervisorAccess)]
        [HttpGet]
        public ActionResult Create(int projectId)
        {
            return View("React");
        }

        [Authorize(Policy = AccessCodes.SupervisorAccess)]
        [HttpPost]
        public async Task<ActionResult> Save(int projectId, bool submit, [FromBody] QuoteDetail quoteDetail)
        {
            // only FM is allowed to submit a quote, anyone with access can save
            if (submit && !await _userService.HasAccess(AccessCodes.FieldManagerAccess)) { return Unauthorized(); }

            // Use existing quote if it exists, otherwise create new one
            var quote = await _dbContext.Quotes.Where(q => q.ProjectId == projectId && q.ApprovedOn == null).SingleOrDefaultAsync();

            if (quote == null)
            {
                quote = new Quote();

                // newly created quote, populate defaults
                var currentUser = await this._userService.GetCurrentUser();
                quote.InitiatedById = currentUser.Id;
                quote.Status = "New"; // TODO: definte status progression
                quote.CreatedDate = DateTime.UtcNow;
                quote.ProjectId = projectId;

                await _dbContext.Quotes.AddAsync(quote);
            }

            quote.Total = (decimal)Math.Round(quoteDetail.GrandTotal, 2);
            quote.Text = QuoteDetail.Serialize(quoteDetail);

            if (submit)
            {
                quote.Status = Quote.Statuses.Proposed;

                var project = await _dbContext.Projects.SingleAsync(p => p.Id == projectId);
                project.Status = Project.Statuses.PendingApproval;
                await _historyService.QuoteSubmitted(projectId, quote);
            }
            else
            {
                await _historyService.QuoteSaved(projectId, quote);
            }

            await _dbContext.SaveChangesAsync();

            if (submit)
            {
                //Email needs the quote and PI
                var project = await _dbContext.Projects.Include(a => a.PrincipalInvestigator).SingleAsync(p => p.Id == projectId);
                await _emailService.ProfessorQuoteReady(project, quote);
            }

            return Ok(quote);
        }
    }
}
