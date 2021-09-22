using System.Linq;
using System.Threading.Tasks;
using Harvest.Core.Data;
using Harvest.Core.Models;
using Harvest.Core.Models.InvoiceModels;
using Harvest.Core.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Harvest.Web.Controllers.Api
{
    [Authorize(Policy = AccessCodes.PrincipalInvestigator)]
    public class InvoiceController : Controller
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
        [HttpGet("/invoice/get/{id}")]
        public async Task<ActionResult> Get(int id)
        {
            var invoice = await _dbContext.Invoices
                .Include(a => a.Transfers)
                .Include(i => i.Expenses)
                .ThenInclude(e => e.Rate)
                .AsNoTracking()
                .SingleAsync(i => i.Id == id);
            var project = await _dbContext.Projects
                .Include(p => p.PrincipalInvestigator)
                .Include(p => p.CreatedBy)
                .Include(p => p.Accounts)
                .AsNoTracking()
                .SingleOrDefaultAsync(p => p.Id == invoice.ProjectId);
            return Json(new ProjectInvoiceModel { Project = project, Invoice = new InvoiceModel(invoice) });
        }
        [HttpGet]
        public async Task<ActionResult> List(int projectId)
        {
            var user = await _userService.GetCurrentUser();
            var hasAccess = await _userService.HasAccess(AccessCodes.FieldManagerAccess);
            return Ok(await _dbContext.Invoices.Where(a =>
                    a.ProjectId == projectId
                    && (hasAccess || a.Project.PrincipalInvestigatorId == user.Id))
                .ToArrayAsync());
        }

        // Get react view, whose router should choose InvoiceDetailContainer
        [HttpGet]
        public ActionResult Details(int id)
        {
            return View("React");
        }

        [HttpPost]
        [Authorize(Policy = AccessCodes.FieldManagerAccess)]
        public async Task<ActionResult> DoCloseout(int id)
        {
            var result = await _invoiceService.CreateInvoice(id, true);
            return Ok(result);
        }
    }
}