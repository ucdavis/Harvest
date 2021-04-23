using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Harvest.Core.Data;
using Harvest.Core.Domain;
using Harvest.Core.Extensions;
using Harvest.Core.Services;
using Harvest.Email.Models;
using Harvest.Email.Services;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace Harvest.Web.Services
{
    public interface IEmailService
    {
        Task<bool> ProfessorQuoteReady(Project project);
        Task<bool> NewFieldRequest(Project project);

        Task<bool> QuoteApproved(Project project);
        Task<bool> QuoteDenied(Project project);
    }

    public class EmailService : IEmailService
    {
        private readonly AppDbContext _dbContext;
        private readonly IEmailBodyService _emailBodyService;
        private readonly INotificationService _notificationService;

        public EmailService(AppDbContext dbContext, IEmailBodyService emailBodyService, INotificationService notificationService)
        {
            _dbContext = dbContext;
            _emailBodyService = emailBodyService;
            _notificationService = notificationService;
        }
        public async Task<bool> ProfessorQuoteReady(Project project)
        {
            var url = "https://harvest.caes.ucdavis.edu/request/approve/";
            if (project.QuoteId == null)
            {
                return false; //No quote
            }

            var model = new ProfessorQuoteModel()
            {
                ProfName = project.PrincipalInvestigator.Name,
                ProjectName = project.Name,
                ProjectStart = project.Start.ToPacificTime().Date.Format("d"),
                ProjectEnd = project.End.ToPacificTime().Date.Format("d"),
                QuoteAmount = project.QuoteTotal.ToString("C"),
                ButtonUrl = $"{url}{project.Id}"
            };
            try
            {
                var emailBody = await _emailBodyService.RenderBody("/Views/Emails/ProfessorQuoteNotification.cshtml", model);

                await _notificationService.SendNotification(new string[] {project.PrincipalInvestigator.Email}, emailBody, "A quote is ready for your review/approval for your harvest project.", "Harvest Notification - Quote Ready");
            }
            catch (Exception e)
            {
                Log.Error("Error trying to email Quote", e);
                return false;
            }

            return true;

        }

        private async Task<string[]> FieldWorkersEmails()
        {
            return await _dbContext.Permissions.Where(a => a.Role.Name == Role.Codes.FieldManager).Select(a => a.User.Email).ToArrayAsync();
        }

        public async Task<bool> NewFieldRequest(Project project)
        {
            var url = "https://harvest.caes.ucdavis.edu/quote/create/";

            var model = new NewFieldRequestModel()
            {
                PI = project.PrincipalInvestigator.NameAndEmail,
                ProjectName = project.Name,
                ProjectStart = project.Start.ToPacificTime().Date.Format("d"),
                ProjectEnd = project.End.ToPacificTime().Date.Format("d"),
                CropType = project.CropType,
                Crops = project.Crop,
                Requirements = project.Requirements,
                ButtonUrl = $"{url}{project.Id}"
            };

            try
            {
                var emailBody = await _emailBodyService.RenderBody("/Views/Emails/NewFieldRequest.cshtml", model);

                await _notificationService.SendNotification(await FieldWorkersEmails(), emailBody, "A new field request has been made.", "Harvest Notification - New Field Request");
            }
            catch (Exception e)
            {
                Log.Error("Error trying to email Quote", e);
                return false;
            }

            return true;
        }

        private async Task<bool> QuoteDecision(Project project, bool approved)
        {
            var url = "https://harvest.caes.ucdavis.edu/Project/Details/";

            var model = new QuoteDecisionModel()
            {
                PI = project.PrincipalInvestigator.NameAndEmail,
                ProjectName = project.Name,
                ProjectStart = project.Start.ToPacificTime().Date.Format("d"),
                ProjectEnd = project.End.ToPacificTime().Date.Format("d"),
                Decision = approved ? "Approved":"Denied",
                DecisionColor = approved ? QuoteDecisionModel.Colors.Approved : QuoteDecisionModel.Colors.Denied,
                ButtonUrl = $"{url}{project.Id}"
            };

            var textVersion = $"A quote has been {model.Decision} for project {model.ProjectName} by {model.PI}";

            try
            {
                var emailBody = await _emailBodyService.RenderBody("/Views/Emails/QuoteDecisionEmail.cshtml", model);

                await _notificationService.SendNotification(await FieldWorkersEmails(), emailBody, textVersion, $"Harvest Notification - Quote {model.Decision}");
            }
            catch (Exception e)
            {
                Log.Error("Error trying to email Quote", e);
                return false;
            }

            return true;
        }

        public async Task<bool> QuoteApproved(Project project)
        {
            return await QuoteDecision(project, true);
        }

        public async Task<bool> QuoteDenied(Project project)
        {
            return await QuoteDecision(project, false);
        }
    }
}
