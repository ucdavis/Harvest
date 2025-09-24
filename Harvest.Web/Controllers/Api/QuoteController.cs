using System;
using System.Linq;
using System.Threading.Tasks;
using Harvest.Core.Data;
using Harvest.Core.Domain;
using Harvest.Core.Models;
using Harvest.Core.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;

namespace Harvest.Web.Controllers.Api
{
    [Authorize(Policy = AccessCodes.InvoiceAccess)] //Need a better name for this.
    public class QuoteController : SuperController
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
            var project = await _dbContext.Projects.Include(p => p.Team).Include(p => p.PrincipalInvestigator)
                .Include(p => p.Accounts)
                .Include(p => p.CreatedBy).SingleAsync(p => p.Id == projectId && p.Team.Slug == TeamSlug);
            var openQuote = await _dbContext.Quotes.Where(q => q.ProjectId == projectId && q.ApprovedOn == null)
                .Select(q => QuoteDetail.Deserialize(q.Text)).SingleOrDefaultAsync();

            var model = new QuoteModel { Project = project, Quote = openQuote };

            return Ok(model);
        }

        // Get info on the project as well as in-progress quote if it exists
        [HttpGet]
        [Route("/api/{team}/Quote/GetApproved/{projectId}/{shareId?}")]
        public async Task<ActionResult> GetApproved(int projectId, Guid? shareId)
        {
            //TODO: Check the has access here, because the shareId might open this up to other roles.
            var hasAccess = await _userService.HasAccess(new[] { AccessCodes.FieldManagerAccess, AccessCodes.FinanceAccess }, TeamSlug);
            var user = await _userService.GetCurrentUser();
            var project = await _dbContext.Projects
                .Include(p => p.Team)
                .Include(p => p.Quote).ThenInclude(a => a.ApprovedBy)
                .Include(p => p.PrincipalInvestigator)
                .Include(p => p.Accounts)
                .Include(p => p.CreatedBy)
                .Include(p => p.ProjectPermissions).ThenInclude(pp => pp.User)
                .SingleAsync(p => p.Id == projectId && p.Team.Slug == TeamSlug);

            if (shareId != null)
            {
                if (project.ShareId != shareId)
                {
                    //return not authorized
                    return Unauthorized();
                }
            }
            else
            {
                if(!hasAccess && project.PrincipalInvestigator.Id != user.Id && !project.ProjectPermissions.Any(a => a.User.Id == user.Id))
                {
                    //return not authorized
                    return Unauthorized();
                }
            }

            

            var model = new QuoteModel
            {
                Project = project,
                Quote = !string.IsNullOrWhiteSpace(project.Quote?.Text) ? QuoteDetail.Deserialize(project.Quote.Text) : null,
            };
            if (project.Quote != null)
            {
                model.Quote.ApprovedBy = project.Quote?.ApprovedBy;
                model.Quote.ApprovedOn = project.Quote?.ApprovedOn;
            }

            if(project.QuoteId == null && project.OriginalProjectId != null)
            {
                // Ok, we want to try to view the pending change request's quote.
                var newQuote = await _dbContext.Quotes.SingleOrDefaultAsync(q => q.ProjectId == project.Id);
                if(newQuote != null)
                {
                    model.Quote = QuoteDetail.Deserialize(newQuote.Text);
                }
            }

            return Ok(model);
        }

        [Authorize(Policy = AccessCodes.SupervisorAccess)]
        [HttpPost]
        public async Task<ActionResult> Save(int projectId, bool submit, [FromBody] QuoteDetail quoteDetail)
        {
            // only FM is allowed to submit a quote, anyone with access can save
            if (submit && !await _userService.HasAccess(AccessCodes.FieldManagerAccess, TeamSlug))
            {
                return Unauthorized();
            }


            // Use existing quote if it exists, otherwise create new one
            var quote = await _dbContext.Quotes.Where(q => q.ProjectId == projectId && q.ApprovedOn == null).SingleOrDefaultAsync();
            var project = await _dbContext.Projects.Include(a => a.Quote).SingleAsync(a => a.Id == projectId && a.Team.Slug == TeamSlug);

            if (quote == null)
            {
                quote = new Quote();

                // newly created quote, populate defaults
                var currentUser = await this._userService.GetCurrentUser();
                quote.InitiatedById = currentUser.Id;
                quote.Status = "New"; // TODO: definte status progression
                quote.CreatedDate = DateTime.UtcNow;
                quote.ProjectId = projectId;
                project.Quote = quote;
                await _dbContext.Quotes.AddAsync(quote);
            }

            quote.Total = (decimal)Math.Round(quoteDetail.GrandTotal, 2);
            quote.Text = QuoteDetail.Serialize(quoteDetail);

            if (submit)
            {
                quote.Status = Quote.Statuses.Proposed;
                project.UpdateStatus(Project.Statuses.PendingApproval);
                await _historyService.QuoteSubmitted(projectId, quote);
            }
            else
            {
                await _historyService.QuoteSaved(projectId, quote);
                if(!await _userService.HasAccess(AccessCodes.FieldManagerAccess, TeamSlug))
                {
                    await _emailService.SupervisorSavedQuote(project, quote);
                }
            }

            await _dbContext.SaveChangesAsync();

            if (submit)
            {
                //Email needs the quote and PI
                project = await _dbContext.Projects.Include(a => a.PrincipalInvestigator).Include(a => a.Team).SingleAsync(p => p.Id == projectId);
                await _emailService.ProfessorQuoteReady(project, quote);
            }

            return Ok(quote);
        }
    }
}
