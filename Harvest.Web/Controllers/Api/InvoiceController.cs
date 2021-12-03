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
    [Authorize(Policy = AccessCodes.PrincipalInvestigator)]
    public class InvoiceController : SuperController
    {
        private readonly AppDbContext _dbContext;
        private readonly IUserService _userService;
        private readonly IInvoiceService _invoiceService;

        public InvoiceController(AppDbContext dbContext, IUserService userService, IInvoiceService invoiceService)
        {
            this._dbContext = dbContext;
            this._userService = userService;
            this._invoiceService = invoiceService;
        }

        // Get info on the project as well as invoice
        [HttpGet]
        public async Task<ActionResult> Get(int projectId, int invoiceId)
        {
            var invoice = await _dbContext.Invoices
                .Include(a => a.Transfers)
                .Include(i => i.Expenses)
                .ThenInclude(e => e.Rate)
                .AsNoTracking()
                .SingleAsync(i => i.Id == invoiceId);

            if (invoice.ProjectId != projectId)
            {
                return BadRequest("Invoice not associated with the current project");
            }

            var project = await _dbContext.Projects
                .Include(p => p.PrincipalInvestigator)
                .Include(p => p.CreatedBy)
                .Include(p => p.Accounts)
                .AsNoTracking()
                .SingleOrDefaultAsync(p => p.Id == invoice.ProjectId);

            return Json(new ProjectInvoiceModel { Project = project, Invoice = new InvoiceModel(invoice) });
        }
        [HttpGet]
        public async Task<ActionResult> List(int projectId, int? maxRows)
        {
            var user = await _userService.GetCurrentUser();
            var hasAccess = await _userService.HasAccess(AccessCodes.FieldManagerAccess);

            var invoiceQuery = _dbContext.Invoices.Where(a =>
                    a.ProjectId == projectId
                    && (hasAccess || a.Project.PrincipalInvestigatorId == user.Id))
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
            var result = await _invoiceService.InitiateCloseout(projectId);
            return Ok(result);
        }

        [HttpPost]
        [Authorize(Policy = AccessCodes.PrincipalInvestigator)]
        public async Task<ActionResult> DoCloseout(int projectId)
        {
            var result = await _invoiceService.CreateInvoice(projectId, true);
            return Ok(result);
        }
    }
}