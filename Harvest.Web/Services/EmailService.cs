using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Harvest.Core.Data;
using Harvest.Core.Domain;
using Harvest.Core.Extensions;
using Harvest.Core.Services;
using Harvest.Email.Models;
using Harvest.Email.Models.Ticket;
using Harvest.Email.Services;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace Harvest.Web.Services
{
    public interface IEmailService
    {
        Task<bool> ProfessorQuoteReady(Project project);
        Task<bool> NewFieldRequest(Project project);
        Task<bool> ChangeRequest(Project project);

        Task<bool> QuoteApproved(Project project);
        Task<bool> QuoteDenied(Project project);

        Task<bool> ApproveAccounts(Project project, string[] emails);

        Task<bool> InvoiceExceedsQuote(Project project, decimal invoiceAmount, decimal quoteRemaining);
        Task<bool> NewTicketCreated(Project project, Ticket ticket);
        Task<bool> TicketReplyAdded(Project project, Ticket ticket, TicketMessage ticketMessage);
        Task<bool> TicketAttachmentAdded(Project project, Ticket ticket, TicketAttachment[] ticketAttachments);
        Task<bool> TicketClosed(Project project, Ticket ticket);
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

        private async Task<string[]> FieldManagersEmails()
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

                await _notificationService.SendNotification(await FieldManagersEmails(), emailBody, "A new field request has been made.", "Harvest Notification - New Field Request");
            }
            catch (Exception e)
            {
                Log.Error("Error trying to email Quote", e);
                return false;
            }

            return true;
        }

        public async Task<bool> ChangeRequest(Project project)
        {
            var quoteUrl   = "https://harvest.caes.ucdavis.edu/quote/create/";
            var projectUrl = "https://harvest.caes.ucdavis.edu/Project/Details/";

            var model = new ChangeRequestModel()
            {
                PI = project.PrincipalInvestigator.NameAndEmail,
                ProjectName = project.Name,
                ProjectStart = project.Start.ToPacificTime().Date.Format("d"),
                ProjectEnd = project.End.ToPacificTime().Date.Format("d"),
                CropType = project.CropType,
                Crops = project.Crop,
                Requirements = project.Requirements,
                ButtonUrlForQuote = $"{quoteUrl}{project.Id}",
                ButtonUrlForProject = $"{projectUrl}{project.OriginalProjectId}",
            };

            try
            {
                var emailBody = await _emailBodyService.RenderBody("/Views/Emails/ChangeRequest.cshtml", model);

                var textVersion = $"A change request has been made by {model.PI} for project {model.ProjectName}.";

                await _notificationService.SendNotification(await FieldManagersEmails(), emailBody, textVersion, "Harvest Notification - Change Request");
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

                await _notificationService.SendNotification(await FieldManagersEmails(), emailBody, textVersion, $"Harvest Notification - Quote {model.Decision}");
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

        public async Task<bool> ApproveAccounts(Project project, string[] emails)
        {
            var url = "https://harvest.caes.ucdavis.edu/Project/AccountApproval/";

            var model = new AccountPendingApprovalModel()
            {
                PI = project.PrincipalInvestigator.NameAndEmail,
                ProjectName = project.Name,
                ProjectStart = project.Start.ToPacificTime().Date.Format("d"),
                ProjectEnd = project.End.ToPacificTime().Date.Format("d"),
                AccountsList = new List<AccountsForApprovalModel>(),
                ButtonUrl = $"{url}{project.Id}"
            };
            foreach (var account in project.Accounts.Where(a => a.ApprovedBy == null))
            {
                model.AccountsList.Add(new AccountsForApprovalModel(){Account = account.Number, Name = account.Name, Percent = $"{account.Percentage}%"});
            }

            var textVersion = $"Accounts require your approval for use in project {model.ProjectName} by {model.PI}";

            try
            {
                var emailBody = await _emailBodyService.RenderBody("/Views/Emails/AccountPendingApproval.cshtml", model);

                await _notificationService.SendNotification(emails, emailBody, textVersion, $"Harvest Notification - Accounts need approval");
            }
            catch (Exception e)
            {
                Log.Error("Error trying to email Quote", e);
                return false;
            }

            return true;
        }

        public async Task<bool> InvoiceExceedsQuote(Project project, decimal invoiceAmount, decimal quoteRemaining)
        {
            var url = "https://harvest.caes.ucdavis.edu/Project/Details/";

            var model = new InvoiceExceedsQuoteModel()
            {
                PI              = project.PrincipalInvestigator.NameAndEmail,
                ProjectName     = project.Name,
                ProjectStart    = project.Start.ToPacificTime().Date.Format("d"),
                ProjectEnd      = project.End.ToPacificTime().Date.Format("d"),
                InvoiceAmount   = invoiceAmount.ToString("C"),
                RemainingAmount = quoteRemaining.ToString("C"),
                ButtonUrl       = $"{url}{project.Id}"
            };

            var textVersion = $"An invoice can't be created for project {model.ProjectName} because it exceeds the remaining amount for the quote. RED ALERT!!!";

            try
            {
                var emailBody = await _emailBodyService.RenderBody("/Views/Emails/InvoiceExceedsRemainingAmount.cshtml", model);

                await _notificationService.SendNotification(await FieldManagersEmails(), emailBody, textVersion, $"Harvest Notification - Invoice can't be created");
            }
            catch (Exception e)
            {
                Log.Error("Error trying to email Invoice can't be created", e);
                return false;
            }

            return true;
        }

        public async Task<bool> NewTicketCreated(Project project, Ticket ticket)
        {
            //Notify FieldManagersEmails
            try
            {
                throw new NotImplementedException();
            }
            catch (Exception e)
            {
                Log.Error("Error trying to email New Ticket", e);
                return false;
            }
            

            return true;
        }

        public Task<bool> TicketReplyAdded(Project project, Ticket ticket, TicketMessage ticketMessage)
        {
            //if ticketMessage.createdby == project.pi, email fieldManages emails, otherwise email PI
            throw new NotImplementedException();
        }

        public Task<bool> TicketAttachmentAdded(Project project, Ticket ticket, TicketAttachment[] ticketAttachments)
        {
            //if ticketattachments[0].createdby == project.pi, email fieldManages emails, otherwise email PI
            throw new NotImplementedException();
        }

        public Task<bool> TicketClosed(Project project, Ticket ticket)
        {
            //Email PI
            throw new NotImplementedException();
        }
    }
}
