using System;
using System.Linq;
using System.Threading.Tasks;
using Harvest.Core.Data;
using Harvest.Core.Domain;
using Harvest.Core.Models;
using Harvest.Core.Models.InvoiceModels;
using Harvest.Core.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Harvest.Web.Controllers.Api
{
    [Authorize(Policy = AccessCodes.InvoiceAccess)]
    public class InvoiceController : SuperController
    {
        private readonly AppDbContext _dbContext;
        private readonly IUserService _userService;
        private readonly IInvoiceService _invoiceService;
        private readonly IProjectHistoryService _historyService;

        public InvoiceController(AppDbContext dbContext, IUserService userService, IInvoiceService invoiceService, IProjectHistoryService historyService)
        {
            _dbContext = dbContext;
            _userService = userService;
            _invoiceService = invoiceService;
            _historyService = historyService;
        }

        // Get info on the project as well as invoice
        [HttpGet]
        public async Task<ActionResult> Get(int projectId, int invoiceId, Guid? shareId)
        {

            var invoice = await _dbContext.Invoices
                .Include(a => a.Transfers)
                .Include(i => i.Expenses)
                .ThenInclude(e => e.Rate)
                .AsNoTracking()
                .AsSplitQuery()
                .SingleAsync(i => i.Id == invoiceId && i.Project.Team.Slug == TeamSlug);

            if (invoice.ProjectId != projectId)
            {
                return BadRequest("Invoice not associated with the current project");
            }

            var project = await _dbContext.Projects
                .Include(p => p.PrincipalInvestigator)
                .Include(p => p.CreatedBy)
                .Include(p => p.Accounts)
                .Include(p => p.Team)
                .AsNoTracking()
                .AsSplitQuery()
                .SingleOrDefaultAsync(p => p.Id == invoice.ProjectId && p.Team.Slug == TeamSlug);

            if(shareId != null && invoice.Project.ShareId != shareId)
            {
                return BadRequest("share id invalid");
            }

            return Json(new ProjectInvoiceModel { Project = project, Invoice = new InvoiceModel(invoice) });
        }
        [HttpGet]
        public async Task<ActionResult> List(int projectId, int? maxRows, Guid? shareId)
        {
            var user = await _userService.GetCurrentUser();
            //var hasAccess = await _userService.HasAccess(AccessCodes.FieldManagerAccess) || await _userService.HasAccess(AccessCodes.FinanceAccess);
            var hasAccess = await _userService.HasAccess(new []{ AccessCodes.FieldManagerAccess, AccessCodes.FinanceAccess}, TeamSlug);

            var invoiceQuery = _dbContext.Invoices.Where(a =>
                    a.ProjectId == projectId && a.Project.Team.Slug == TeamSlug
                    && (hasAccess || a.Project.PrincipalInvestigatorId == user.Id || (shareId != null && a.Project.ShareId == shareId)))
                    .OrderByDescending(a => a.CreatedOn);

            if (maxRows.HasValue)
            {
                invoiceQuery = (IOrderedQueryable<Invoice>)invoiceQuery.Take(maxRows.Value);
            }
            return Ok(await invoiceQuery.ToListAsync());
        }

        [HttpPost]
        [Authorize(Policy = AccessCodes.FieldManagerAccess)]
        public async Task<ActionResult> InitiateCloseout(int projectId)
        {
            if (await _dbContext.Projects.AnyAsync(a => a.Id == projectId && a.Team.Slug != TeamSlug))
            {
                return BadRequest("Project not associated with the current team");
            }

            var result = await _invoiceService.InitiateCloseout(projectId);
            return Ok(result);
        }

        [HttpPost]
        [Authorize(Policy = AccessCodes.PrincipalInvestigator)]
        public async Task<ActionResult> DoCloseout(int projectId)
        {
            //Some of these may fail from old emails...
            var project = await _dbContext.Projects.SingleAsync(a => a.Id == projectId && a.Team.Slug == TeamSlug);
            await _historyService.ProjectCloseoutApproved(projectId, project); //Deal with this if the result has errors? I think logging that they approved it is fine, even if it fails.
            await _dbContext.SaveChangesAsync();

            var result = await _invoiceService.CreateInvoice(projectId, true);
            return Ok(result);
        }
    }
}