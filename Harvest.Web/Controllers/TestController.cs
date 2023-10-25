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
using Harvest.Email.Models.Ticket;
using System.Collections.Generic;

namespace Harvest.Web.Controllers
{
    [Authorize(Policy = AccessCodes.SystemAccess)]
    public class TestController : SuperController
    {
        private readonly AppDbContext _dbContext;
        private readonly IUserService _userService;
        private readonly INotificationService _notificationService;
        private readonly IEmailService _emailService;
        private readonly IAggieEnterpriseService _aggieEnterpriseService;

        public TestController(AppDbContext dbContext, IUserService userService,
            INotificationService notificationService, IEmailService emailService, IAggieEnterpriseService aggieEnterpriseService)
        {
            _dbContext = dbContext;
            _userService = userService;
            _notificationService = notificationService;
            _emailService = emailService;
            _aggieEnterpriseService = aggieEnterpriseService;
        }
        public async Task<IActionResult> TestBody()
        {
            var model = new ProjectClosedModel();
      


            var results = await RazorTemplateEngine.RenderAsync("/Views/Emails/ProjectClosed_mjml.cshtml", model);

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
            var project = await _dbContext.Projects.Include(a => a.PrincipalInvestigator).Include(a => a.Accounts).SingleAsync(a => a.Id == 2);
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

        public async Task<IActionResult> TestCloseoutApproved()
        {
            var project = await _dbContext.Projects.SingleAsync(a => a.Id == 19);
            var rtValue = await _emailService.ProjectClosed(project, true);

            return Content($"Email was {rtValue}");
        }


        public async Task<IActionResult> TestAccountConvert()
        {
            var inputAccounts = new[] { "3-AARRF02", "3-AARRI12", "3-ANS0829-DAIRY", "3-APMRE11-AEND2", "3-APSD260", "3-APSF720", "3-APSF999", "3-APSFA20", "3-APSFA61", "3-APSFA63", "3-APSFA79", "3-APSFA82", "3-APSFA83", "3-APSFA88", "3-APSFB01", "3-APSFB09", "3-APSFB31", "3-APSFB40", "3-APSFB50", "3-APSFB55", "3-APSFB71", "3-APSFB82", "3-APSG347", "3-APSIGAA-AEFOT", "3-APSIGAA-CS111", "3-APSIGAA-CS150", "3-APSK053", "3-APSK160", "3-APSM596", "3-APSM626", "3-APSM628", "3-APSM673", "3-APSM676", "3-APSM677", "3-APSM700", "3-APSM702", "3-APSM786", "3-APSN036", "3-APSNFLP", "3-APSPAJ4", "3-APSPAN1", "3-APSPP54", "3-APSPR94", "3-APSPRA6", "3-APSPT52", "3-APSPWJ6", "3-APSPWK1", "3-APSPWL1", "3-APSRFLV", "3-APSRGB2", "3-APSRICC-DPUMP", "3-APSV034", "3-APSV081", "3-APSV281", "3-CENPROJ-CABA2", "3-COEADSU", "3-CXN1C57", "3-CXN5F84", "3-ENVIGRI", "3-FPMSEDR", "3-FPSPIST", "3-FPSUSER", "3-FSEEDSV", "3-JMPOPDY", "3-JRINSFC", "3-KIS7D99", "3-KISE131", "3-KISED92", "3-LSVTGIF", "3-OCR6270", "3-PANORTH", "3-RMSCRI3", "3-RRACRES-CNTRY", "3-V0D9518", "3-VCR883R", "3-VEIVY75", "L-151WRC6", "L-257DSS0" };

            var rtValue = new List<string>();

            foreach (var account in inputAccounts)
            {
                var rt = await _aggieEnterpriseService.ConvertKfsAccount(account);
                if (rt == null)
                {
                    rtValue.Add($"{account} -> {rt} NOT FOUND!!!");
                    continue;
                }
                var validate = await _aggieEnterpriseService.IsAccountValid(rt);

                rtValue.Add($"{account} -> {rt} {(validate.IsValid ? "" : "INVALID!!! ")} {(validate.IsValid ? "" : validate.Message)}");
            }
            


            return Content(string.Join("\n", rtValue));
        }
    }
}
