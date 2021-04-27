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
            //var model = new ChangeRequestModel();
            //model.InitForMjml();

            //var results = await _emailBodyService.RenderBody("/Views/Emails/ChangeRequest_mjml.cshtml", model);

            //var model = new TestEmailModel();
            //model.InitForMjml();
            //var results = await _emailBodyService.RenderBody("/Views/Emails/TestEmail_mjml.cshtml", model);

            var model = new AccountPendingApprovalModel();
            model.InitForMjml();

            var results = await _emailBodyService.RenderBody("/Views/Emails/AccountPendingApproval_mjml.cshtml", model);

            return Content(results);
        }

        [Authorize(Policy = AccessCodes.SystemAccess)]
        public async Task<IActionResult> TestEmail()
        {
            var user = await _userService.GetCurrentUser();
            //var model = new TestEmailModel()
            //{
            //    Name = user.Name
            //};
            //var xxx = await _emailBodyService.RenderBody("/Views/Emails/TestEmail.cshtml", model);
            //await _notificationService.SendSampleNotificationMessage(user.Email, xxx);
            //var model = new QuoteDecisionModel()
            //{
            //    PI = user.NameAndEmail,
            //    ProjectName = "Your Awesome Project",
            //    ProjectStart = DateTime.UtcNow.ToPacificTime().Date.Format("d"),
            //    ProjectEnd = DateTime.UtcNow.AddYears(2).ToPacificTime().Date.Format("d"),
            //    Decision = "Approved",
            //    DecisionColor = QuoteDecisionModel.Colors.Approved,
            //    //ButtonUrl = "???"
            //};

            //var emailBody = await _emailBodyService.RenderBody("/Views/Emails/QuoteDecisionEmail.cshtml", model);

            //var model = new TestEmailModel();
            //model.Name = "Jason";
            //model.MyList = new List<string>();
            //model.MyList.Add("Test Line 1");
            //model.MyList.Add("Test Line 2");
            //model.MyList.Add("For The WIN");
            //model.MyList.Add("Last Line");
            //var emailBody = await _emailBodyService.RenderBody("/Views/Emails/TestEmail.cshtml", model);

            //await _notificationService.SendNotification(new string[]{ user.Email }, emailBody, "A quote is ready for your review/approval for your harvest project.", "Harvest Notification - Quote Ready");

            var model = new AccountPendingApprovalModel();
            model.PI = user.NameAndEmail;
            model.ProjectName = "Jason's Awesome Project";
            model.ProjectStart = DateTime.UtcNow.ToPacificTime().Date.Format("d");
            model.ProjectEnd = DateTime.UtcNow.AddYears(2).ToPacificTime().Date.Format("d");
            model.ButtonUrl = "https://harvest.caes.ucdavis.edu";
            model.AccountsList = new List<AccountsForApprovalModel>();
            model.AccountsList.Add(new AccountsForApprovalModel() { Account = "3-CRU9033", Name = "COMPUTING RESOURCES UNIT- GETCHELL", Percent = "75%" });
            model.AccountsList.Add(new AccountsForApprovalModel() { Account = "3-APSNFDS", Name = "PS:FIELD:DAVIS SHOP ACCOUNT", Percent = "15%" });
            model.AccountsList.Add(new AccountsForApprovalModel() { Account = "3-RRCNTRY", Name = "RUSSELL RANCH CENTURY PROJECT", Percent = "10%" });

            var emailBody = await _emailBodyService.RenderBody("/Views/Emails/AccountPendingApproval.cshtml", model);
            await _notificationService.SendNotification(new string[] { user.Email }, emailBody, "A quote is ready for your review/approval for your harvest project.", "Harvest Notification - Accounts Need Approval");

            return Content("Done. Maybe. Well, possibly. If you don't get it, check the settings.");
        }

        public async Task<IActionResult> TestQuoteNotify()
        {
            var project = await _dbContext.Projects.Include(a => a.PrincipalInvestigator).SingleAsync(a => a.Id == 7);
            if (await _emailService.ProfessorQuoteReady(project))
            {
                return Content("Done.");
            }
            return Content("Looks like there was a problem.");
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
    }
}
