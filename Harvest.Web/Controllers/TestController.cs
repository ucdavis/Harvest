﻿using Microsoft.AspNetCore.Mvc;
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
            var model = new QuoteDecisionModel();
            model.InitForMjml();

            var results = await _emailBodyService.RenderBody("/Views/Emails/QuoteDecisionEmail_mjnl.cshtml", model);

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
            var model = new QuoteDecisionModel()
            {
                PI = user.NameAndEmail,
                ProjectName = "Your Awesome Project",
                ProjectStart = DateTime.UtcNow.ToPacificTime().Date.Format("d"),
                ProjectEnd = DateTime.UtcNow.AddYears(2).ToPacificTime().Date.Format("d"),
                Decision = "Approved",
                DecisionColor = QuoteDecisionModel.Colors.Approved,
                //ButtonUrl = "???"
            };

            var emailBody = await _emailBodyService.RenderBody("/Views/Emails/QuoteDecisionEmail.cshtml", model);

            await _notificationService.SendNotification(new string[]{ user.Email }, emailBody, "A quote is ready for your review/approval for your harvest project.", "Harvest Notification - Quote Ready");
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
    }
}
