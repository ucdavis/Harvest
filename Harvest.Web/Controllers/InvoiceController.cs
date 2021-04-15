using System;
using System.Linq;
using System.Threading.Tasks;
using Harvest.Core.Data;
using Harvest.Core.Domain;
using Harvest.Core.Models;
using Harvest.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Harvest.Web.Controllers
{
    [Authorize]
    public class InvoiceController : SuperController
    {
        private readonly AppDbContext _dbContext;
        private readonly IUserService _userService;

        public InvoiceController(AppDbContext dbContext, IUserService userService)
        {
            this._dbContext = dbContext;
            this._userService = userService;
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
        public async Task<ActionResult> Create(int projectId)
        {
            var unbilled = await _dbContext.Expenses.Where(e => e.Invoice == null && e.ProjectId == projectId).ToArrayAsync();

            if (unbilled.Length == 0)
            {
                Message = "No unbilled expenses exist";
                return RedirectToAction("Index", new { projectId = projectId });
            }

            var newInvoice = new Invoice { CreatedOn = DateTime.UtcNow, ProjectId = projectId, Status = Invoice.Statuses.Created, Total = unbilled.Sum(x => x.Total) };

            newInvoice.Expenses = new System.Collections.Generic.List<Expense>(unbilled);

            _dbContext.Invoices.Add(newInvoice);
            await _dbContext.SaveChangesAsync();

            Message = "Invoice created";

            return RedirectToAction("Details", new { id = newInvoice.Id });
        }
    }
}
