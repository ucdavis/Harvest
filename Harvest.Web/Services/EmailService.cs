using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Harvest.Core.Domain;
using Harvest.Core.Extensions;
using Harvest.Core.Services;
using Harvest.Email.Models;
using Harvest.Email.Services;
using Serilog;

namespace Harvest.Web.Services
{
    public interface IEmailService
    {
        Task<bool> ProfessorQuoteReady(Project project);
    }

    public class EmailService : IEmailService
    {
        private readonly IEmailBodyService _emailBodyService;
        private readonly INotificationService _notificationService;

        public EmailService(IEmailBodyService emailBodyService, INotificationService notificationService)
        {
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

                await _notificationService.SendNotification(project.PrincipalInvestigator.Email, emailBody, "A quote is ready for your review/approval for your harvest project.", "Harvest Notification - Quote Ready");
            }
            catch (Exception e)
            {
                Log.Error("Error trying to email Quote", e);
                return false;
            }

            return true;

        }
    }
}
