using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Harvest.Core.Data;
using Harvest.Core.Extensions;
using Harvest.Core.Models;
using Harvest.Core.Services;
using Harvest.Email.Models;
using Harvest.Email.Models.Ticket;
using Harvest.Email.Services;
using Harvest.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace Harvest.Web.Controllers
{
    [Authorize(Policy = AccessCodes.SystemAccess)]
    public class TestController : SuperController
    {
        private readonly AppDbContext _dbContext;
        private readonly IUserService _userService;
        private readonly INotificationService _notificationService;
        private readonly IEmailBodyService _emailBodyService;
        private readonly IEmailService _emailService;

        public TestController(AppDbContext dbContext, IUserService userService,
            INotificationService notificationService, IEmailBodyService emailBodyService, IEmailService emailService)
        {
            _dbContext = dbContext;
            _userService = userService;
            _notificationService = notificationService;
            _emailBodyService = emailBodyService;
            _emailService = emailService;
        }
        public async Task<IActionResult> TestBody()
        {


            var model = new TicketAttachmentModel();
            
            var results = await _emailBodyService.RenderBody("/Views/Emails/Ticket/TicketAttachment_mjml.cshtml", model);

            return Content(results);
        }

        [Authorize(Policy = AccessCodes.SystemAccess)]
        public async Task<IActionResult> TestEmail()
        {
            var user = await _userService.GetCurrentUser();
            
            var model = new TicketAttachmentModel();
            model.From = user.NameAndEmail;
            model.ProjectName = "Jason's Awesome Project";
            model.CreatedOn = DateTime.UtcNow.ToPacificTime().Date.Format("d");

            var attachmentNames = new List<string>();
            attachmentNames.Add("test.txt");
            attachmentNames.Add("test2.txt");
            model.AttachmentNames = attachmentNames.ToArray();
            model.Subject = "Test subject";
            model.ButtonUrlForProject = "https://harvest.caes.ucdavis.edu/Fake2";
            model.ButtonUrlForTicket = "https://harvest.caes.ucdavis.edu/Fake1";


            var emailBody = await _emailBodyService.RenderBody("/Views/Emails/Ticket/TicketAttachment.cshtml", model);
            await _notificationService.SendNotification(new string[] { user.Email }, emailBody, "A new Ticket has been create", "Harvest Notification - New Ticket");

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
            if (await _emailService.QuoteDenied(project))
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
    }
}
