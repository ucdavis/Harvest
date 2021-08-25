using System;
using System.Linq;
using System.Threading.Tasks;
using Harvest.Core.Data;
using Harvest.Core.Domain;
using Harvest.Core.Models;
using Harvest.Core.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Harvest.Web.Controllers
{
    [Authorize]
    public class TempInvoiceController : SuperController
    {
        private readonly AppDbContext _dbContext;
        private readonly IUserService _userService;
        private readonly IInvoiceService _invoiceService;

        public TempInvoiceController(AppDbContext dbContext, IUserService userService, IInvoiceService invoiceService)
        {
            _dbContext = dbContext;
            _userService = userService;
            _invoiceService = invoiceService;
        }

        // view all invoices for a given project
        public async Task<ActionResult> Index(int projectId)
        {
            return View(await _dbContext.Invoices.Where(x => x.ProjectId == projectId).AsNoTracking().ToArrayAsync());
        }

        public async Task<ActionResult> Unbilled(int projectId)
        {
            return View(await _dbContext.Expenses.Where(e => e.Invoice == null && e.ProjectId == projectId).ToArrayAsync());
        }

        public async Task<ActionResult> Details(int id)
        {
            return View(await _dbContext.Invoices.Include(x => x.Expenses).SingleAsync(x => x.Id == id));
        }

        // Manually create an invoice for the given project based on all currently unbilled expenses
        // Acreage fees will be ignored for manually created invoices
        [HttpPost]
        [Authorize(Policy = AccessCodes.FieldManagerAccess)]
        public async Task<ActionResult> Create(int projectId)
        {
            var result = await _invoiceService.CreateInvoice(projectId, true);

            if (result.IsError)
            {
                Message = result.Message;
                return RedirectToAction("Index", new { projectId = projectId });
            }

            Message = "Invoice created";

            return RedirectToAction("Details", new { id = result.Value });
        }
    }
}
