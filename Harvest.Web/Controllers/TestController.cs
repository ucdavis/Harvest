using Harvest.Core.Data;
using Harvest.Core.Models;
using Harvest.Core.Services;
using Harvest.Email.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Razor.Templating.Core;
using System;
using System.Linq;
using System.Threading.Tasks;
using Harvest.Email.Models.Invoice;

namespace Harvest.Web.Controllers
{
    [Authorize(Policy = AccessCodes.SystemAccess)]
    public class TestController : SuperController
    {
        private readonly AppDbContext _dbContext;
        private readonly IUserService _userService;
        private readonly INotificationService _notificationService;
        private readonly IEmailService _emailService;

        public TestController(AppDbContext dbContext, IUserService userService,
            INotificationService notificationService, IEmailService emailService)
        {
            _dbContext = dbContext;
            _userService = userService;
            _notificationService = notificationService;
            _emailService = emailService;
        }
        public async Task<IActionResult> TestBody()
        {
            var model = new InvoiceErrorModel();



            var results = await RazorTemplateEngine.RenderAsync("/Views/Emails/Invoice/InvoiceErrors_mjml.cshtml", model);

            return Content(results);
        }

        [Authorize(Policy = AccessCodes.SystemAccess)]
        public async Task<IActionResult> TestEmail()
        {
            var user = await _userService.GetCurrentUser();

            var model = await _dbContext.Projects.Where(a => a.IsActive && a.Name != null && a.End <= DateTime.UtcNow.AddYears(1))
                .OrderBy(a => a.End).Take(5).Select(s => new ExpiringProjectsModel
                {
                    EndDate = s.End.ToShortDateString(), Name = s.Name,
                    ProjectUrl = $"https://harvest.caes.ucdavis.edu/Project/Details/{s.Id}"
                }).ToArrayAsync();
            


            var emailBody = await RazorTemplateEngine.RenderAsync("/Views/Emails/ExpiringProjects.cshtml", model);
            await _notificationService.SendNotification(new string[] { user.Email }, null, emailBody, "EXPIRE", "EXPIRE");

            return Content("Done. Maybe. Well, possibly. If you don't get it, check the settings.");
        }


        public async Task<IActionResult> TestNewFieldNotify()
        {
            var project = await _dbContext.Projects.Include(a => a.PrincipalInvestigator).SingleAsync(a => a.Id == 11);
            if (await _emailService.NewFieldRequest(project))
            {
                return Content("Done.");
            }
            return Content("Looks like there was a problem.");
        }

        public async Task<IActionResult> TestQuoteApproved()
        {
            var project = await _dbContext.Projects.Include(a => a.PrincipalInvestigator).SingleAsync(a => a.Id == 11);
            if (await _emailService.QuoteApproved(project))
            {
                return Content("Done.");
            }
            return Content("Looks like there was a problem.");
        }
        public async Task<IActionResult> TestQuoteDenied()
        {
            var project = await _dbContext.Projects.Include(a => a.PrincipalInvestigator).SingleAsync(a => a.Id == 11);
            if (await _emailService.QuoteDenied(project, "Testy McTestFace"))
            {
                return Content("Done.");
            }
            return Content("Looks like there was a problem.");
        }

        public async Task<IActionResult> TestChangeRequest()
        {
            var project = await _dbContext.Projects.Include(a => a.PrincipalInvestigator).Include(a => a.OriginalProject).SingleAsync(a => a.Id == 10);
            if (await _emailService.ChangeRequest(project))
            {
                return Content("Done.");
            }
            return Content("Looks like there was a problem.");
        }

        public async Task<IActionResult> TestAccountApproval()
        {
            var user = await _userService.GetCurrentUser();
            var project = await _dbContext.Projects.Include(a => a.PrincipalInvestigator).Include(a => a.Accounts).SingleAsync(a => a.Id == 1);
            if (await _emailService.ApproveAccounts(project, new []{user.Email}))
            {
                return Content("Done.");
            }
            return Content("Looks like there was a problem.");
        }

        public async Task<IActionResult> TestInvoiceTooBig()
        {
            var project = await _dbContext.Projects.Include(a => a.PrincipalInvestigator).Include(a => a.Accounts).SingleAsync(a => a.Id == 1);
            if (await _emailService.InvoiceExceedsQuote(project, 12345.67000m, 530.00000m))
            {
                return Content("Done.");
            }
            return Content("Looks like there was a problem.");
        }

        public async Task<IActionResult> TestExpiringProjects()
        {
            var numProjects = await _emailService.SendExpiringProjectsNotification(7);

            return Content($"Sent email for {numProjects} projects");
        }

        public async Task<IActionResult> TestInvoiceCreated()
        {
            var invoice = await _dbContext.Invoices.SingleAsync(a => a.Id == 4);
            var rtValue = await _emailService.InvoiceCreated(invoice);
            return Content($"Email was {rtValue}");
        }

        public async Task<IActionResult> TestInvoiceCancelled()
        {
            var invoice = await _dbContext.Invoices.SingleAsync(a => a.Id == 4);
            var rtValue = await _emailService.InvoiceDone(invoice, "Cancelled");
            return Content($"Email was {rtValue}");
        }

        public async Task<IActionResult> TestInvoiceErrors()
        {
            var invoice = await _dbContext.Invoices.SingleAsync(a => a.Id == 1);
            var rtValue = await _emailService.InvoiceError(invoice);
            return Content($"Email was {rtValue}");
        }
    }
}
