using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Harvest.Core.Data;
using Harvest.Core.Domain;
using Harvest.Core.Models;
using Harvest.Core.Models.InvoiceModels;
using Harvest.Core.Services;
using Harvest.Core.Utilities;
using Harvest.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;

namespace Harvest.Web.Controllers
{
    [Authorize(Policy = AccessCodes.PrincipalInvestigator)]
    public class InvoiceController : Controller
    {
        private readonly AppDbContext _dbContext;
        private readonly IUserService _userService;

        public InvoiceController(AppDbContext dbContext, IUserService userService)
        {
            this._dbContext = dbContext;
            this._userService = userService;
        }

        // Get info on the project as well as invoice
        [HttpGet]
        public async Task<ActionResult> Get(int id)
        {
            var invoice = await _dbContext.Invoices
                .Include(a => a.Transfers)
                .Include(i => i.Expenses)
                .ThenInclude(e => e.Rate)
                .AsNoTracking()
                .SingleOrDefaultAsync(i => i.Id == id);
            var project = await _dbContext.Projects
                .Include(p => p.PrincipalInvestigator)
                .Include(p => p.CreatedBy)
                .Include(p => p.Accounts)
                .AsNoTracking()
                .SingleOrDefaultAsync(p => p.Id == invoice.ProjectId);
            return Json(new ProjectInvoiceModel { Project = project, Invoice = new InvoiceModel(invoice) });
        }

        // Get react view, whose router should choose InvoiceDetailContainer
        [HttpGet]
        public ActionResult Details(int id)
        {
            return View("React");
        }
    }
}