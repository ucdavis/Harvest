using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Harvest.Core.Data;
using Harvest.Core.Domain;
using Harvest.Core.Extensions;
using Harvest.Core.Models.Settings;
using Harvest.Core.Services;
using Harvest.Email.Models;
using Harvest.Email.Models.Ticket;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Razor.Templating.Core;
using Serilog;

namespace Harvest.Core.Services
{
    public interface IEmailService
    {
        Task<bool> ProfessorQuoteReady(Project project, Quote quote);
        Task<bool> NewFieldRequest(Project project);
        Task<bool> ChangeRequest(Project project);

        Task<bool> QuoteApproved(Project project);
        Task<bool> QuoteDenied(Project project, string reason);

        Task<bool> ApproveAccounts(Project project, string[] emails);

        Task<bool> InvoiceExceedsQuote(Project project, decimal invoiceAmount, decimal quoteRemaining);
        Task<bool> NewTicketCreated(Project project, Ticket ticket);
        Task<bool> TicketReplyAdded(Project project, Ticket ticket, TicketMessage ticketMessage);
        Task<bool> TicketAttachmentAdded(Project project, Ticket ticket, TicketAttachment[] ticketAttachments);
        Task<bool> TicketClosed(Project project, Ticket ticket, User ClosedBy);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="days">Specifies number of days before the project ends to include in the notification</param>
        /// <returns></returns>
        Task<int> SendExpiringProjectsNotification(int days);
    }

    public class EmailService : IEmailService
    {
        private readonly AppDbContext _dbContext;
        private readonly INotificationService _notificationService;
        private readonly EmailSettings _emailSettings;

        public EmailService(AppDbContext dbContext, INotificationService notificationService, IOptions<EmailSettings> emailSettings)
        {
            _dbContext = dbContext;
            _notificationService = notificationService;
            _emailSettings = emailSettings.Value;
        }
        public async Task<bool> ProfessorQuoteReady(Project project, Quote quote)
        {
            var url = $"{_emailSettings.BaseUrl}/request/approve/";
            if (quote == null)
            {
                throw new Exception("No quote.");
            }


            try
            {
                var model = new ProfessorQuoteModel()
                {
                    ProfName = project.PrincipalInvestigator.Name,
                    ProjectName = project.Name,
                    ProjectStart = project.Start.ToPacificTime().Date.Format("d"),
                    ProjectEnd = project.End.ToPacificTime().Date.Format("d"),
                    QuoteAmount = quote.Total.ToString("C"),
                    ButtonUrl = $"{url}{project.Id}"
                };

                var emailBody = await RazorTemplateEngine.RenderAsync("/Views/Emails/ProfessorQuoteNotification.cshtml", model);

                await _notificationService.SendNotification(new []{project.PrincipalInvestigator.Email},await FieldManagersEmails(), emailBody, "A quote is ready for your review/approval for your harvest project.", "Harvest Notification - Quote Ready");
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
            var url = $"{_emailSettings.BaseUrl}/project/details/";

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
                var emailBody = await RazorTemplateEngine.RenderAsync("/Views/Emails/NewFieldRequest.cshtml", model);

                await _notificationService.SendNotification(await FieldManagersEmails(), new []{project.PrincipalInvestigator.Email}, emailBody, "A new field request has been made.", "Harvest Notification - New Field Request");
            }
            catch (Exception e)
            {
                Log.Error("Error trying to email New Field Request", e);
                return false;
            }

            return true;
        }

        public async Task<bool> ChangeRequest(Project project)
        {
            var quoteUrl   = $"{_emailSettings.BaseUrl}/quote/create/";
            var projectUrl = $"{_emailSettings.BaseUrl}/Project/Details/";

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
                var emailBody = await RazorTemplateEngine.RenderAsync("/Views/Emails/ChangeRequest.cshtml", model);

                var textVersion = $"A change request has been made by {model.PI} for project {model.ProjectName}.";

                await _notificationService.SendNotification(await FieldManagersEmails(), null, emailBody, textVersion, "Harvest Notification - Change Request");
            }
            catch (Exception e)
            {
                Log.Error("Error trying to email Quote", e);
                return false;
            }

            return true;
        }

        private async Task<bool> QuoteDecision(Project project, string reason, bool approved)
        {
            var url = $"{_emailSettings.BaseUrl}/Project/Details/";

            var model = new QuoteDecisionModel()
            {
                PI = project.PrincipalInvestigator.NameAndEmail,
                ProjectName = project.Name,
                ProjectStart = project.Start.ToPacificTime().Date.Format("d"),
                ProjectEnd = project.End.ToPacificTime().Date.Format("d"),
                Decision = approved ? "Approved":"Denied",
                DecisionColor = approved ? QuoteDecisionModel.Colors.Approved : QuoteDecisionModel.Colors.Denied,
                ButtonUrl = $"{url}{project.Id}",
                RejectReason = reason
            };

            var textVersion = $"A quote has been {model.Decision} for project {model.ProjectName} by {model.PI}";

            try
            {
                var emailBody = await RazorTemplateEngine.RenderAsync("/Views/Emails/QuoteDecisionEmail.cshtml", model);

                await _notificationService.SendNotification(await FieldManagersEmails(), new []{project.PrincipalInvestigator.Email}, emailBody, textVersion, $"Harvest Notification - Quote {model.Decision}");
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
            return await QuoteDecision(project, null, true);
        }

        public async Task<bool> QuoteDenied(Project project, string reason)
        {
            return await QuoteDecision(project, reason, false);
        }

        public async Task<bool> ApproveAccounts(Project project, string[] emails)
        {
            var url = $"{_emailSettings.BaseUrl}/Project/AccountApproval/";

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
                var emailBody = await RazorTemplateEngine.RenderAsync("/Views/Emails/AccountPendingApproval.cshtml", model);

                await _notificationService.SendNotification(emails, null, emailBody, textVersion, $"Harvest Notification - Accounts need approval");
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
            var url = $"{_emailSettings.BaseUrl}/Project/Details/";

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
                var emailBody = await RazorTemplateEngine.RenderAsync("/Views/Emails/InvoiceExceedsRemainingAmount.cshtml", model);

                await _notificationService.SendNotification(await FieldManagersEmails(), null, emailBody, textVersion, $"Harvest Notification - Invoice can't be created");
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
            try
            {
                var ticketUrl = $"{_emailSettings.BaseUrl}/Ticket/Details/";
                var projectUrl = $"{_emailSettings.BaseUrl}/Project/Details/";
                var model = new NewTicketModel()
                {
                    ProjectName = project.Name,
                    PI = project.PrincipalInvestigator.NameAndEmail,
                    CreatedOn = ticket.CreatedOn.ToPacificTime().Date.Format("d"),
                    DueDate = ticket.DueDate.HasValue ? ticket.DueDate.Value.ToPacificTime().Date.Format("d") : "N/A",
                    Subject = ticket.Name,
                    Requirements = ticket.Requirements,
                    ButtonUrlForProject = $"{projectUrl}{project.Id}",
                    ButtonUrlForTicket = $"{ticketUrl}{project.Id}/{ticket.Id}",
                };
                var emailBody = await RazorTemplateEngine.RenderAsync("/Views/Emails/Ticket/NewTicket.cshtml", model);
                var textVersion = $"A new ticket has been created for project {model.ProjectName} by {model.PI}";
                await _notificationService.SendNotification(await FieldManagersEmails(), null, emailBody, textVersion, "Harvest Notification - New Ticket");
            }
            catch (Exception e)
            {
                Log.Error("Error trying to email New Ticket", e);
                return false;
            }
            

            return true;
        }

        public async Task<bool> TicketReplyAdded(Project project, Ticket ticket, TicketMessage ticketMessage)
        {
            //if ticketMessage.createdby == project.pi, email fieldManages emails, otherwise email PI
            try
            {
                var emailTo = await FieldManagersEmails();
                if (ticketMessage.CreatedById != project.PrincipalInvestigatorId)
                {
                    emailTo = new[] {project.PrincipalInvestigator.Email};
                }
                var ticketUrl = $"{_emailSettings.BaseUrl}/Ticket/Details/";
                var projectUrl = $"{_emailSettings.BaseUrl}/Project/Details/";
                var model = new TicketReplyModel()
                {
                    ProjectName = project.Name,
                    From = ticketMessage.CreatedBy.NameAndEmail,
                    CreatedOn = ticketMessage.CreatedOn.ToPacificTime().Date.Format("d"),
                    Subject = ticket.Name,
                    Reply = ticketMessage.Message,
                    ButtonUrlForProject = $"{projectUrl}{project.Id}",
                    ButtonUrlForTicket = $"{ticketUrl}{project.Id}/{ticket.Id}",
                };
                var emailBody = await RazorTemplateEngine.RenderAsync("/Views/Emails/Ticket/TicketReply.cshtml", model);
                var textVersion = $"A reply to the ticket in the project {model.ProjectName} by {model.From}";
                await _notificationService.SendNotification(emailTo, null, emailBody, textVersion, "Harvest Notification - Ticket Reply");
            }
            catch (Exception e)
            {
                Log.Error("Error trying to email Ticket Reply", e);
                return false;
            }


            return true;
        }

        public async Task<bool> TicketAttachmentAdded(Project project, Ticket ticket, TicketAttachment[] ticketAttachments)
        {
            //if ticketattachments[0].createdby == project.pi, email fieldManages emails, otherwise email PI
                        try
            {
                var emailTo = await FieldManagersEmails();
                var firstAttachment = ticketAttachments.First();
                if (firstAttachment.CreatedById != project.PrincipalInvestigatorId)
                {
                    emailTo = new[] {project.PrincipalInvestigator.Email};
                }
                var ticketUrl = $"{_emailSettings.BaseUrl}/Ticket/Details/";
                var projectUrl = $"{_emailSettings.BaseUrl}/Project/Details/";
                var model = new TicketAttachmentModel()
                {
                    ProjectName = project.Name,
                    From = firstAttachment.CreatedBy.NameAndEmail,
                    CreatedOn = firstAttachment.CreatedOn.ToPacificTime().Date.Format("d"),
                    Subject = ticket.Name,   
                    AttachmentNames = ticketAttachments.Select(x => x.FileName).ToArray(),         
                    ButtonUrlForProject = $"{projectUrl}{project.Id}",
                    ButtonUrlForTicket = $"{ticketUrl}{project.Id}/{ticket.Id}",
                };
                var emailBody = await RazorTemplateEngine.RenderAsync("/Views/Emails/Ticket/TicketAttachment.cshtml", model);
                var textVersion = $"An attachment was added to the ticket in the project {model.ProjectName} by {model.From}";
                await _notificationService.SendNotification(emailTo, null, emailBody, textVersion, "Harvest Notification - Ticket Attachment Added");
            }
            catch (Exception e)
            {
                Log.Error("Error trying to email Ticket Reply", e);
                return false;
            }


            return true;
        }

        public async Task<bool> TicketClosed(Project project, Ticket ticket, User closedBy)
        {
            try
            {
                string[] emailTo = null;
                string[] ccEmails = null;
                if (ticket.Project.PrincipalInvestigatorId == closedBy.Id)
                {
                    emailTo = await FieldManagersEmails();
                    ccEmails = new[] {project.PrincipalInvestigator.Email};
                }
                else
                {
                    emailTo = new[] {project.PrincipalInvestigator.Email};
                    ccEmails = await FieldManagersEmails();
                }

                var ticketUrl = $"{_emailSettings.BaseUrl}/Ticket/Details/";
                var projectUrl = $"{_emailSettings.BaseUrl}/Project/Details/";
                var model = new TicketReplyModel()
                {
                    ProjectName = project.Name,
                    Subject = ticket.Name,
                    ButtonUrlForProject = $"{projectUrl}{project.Id}",
                    ButtonUrlForTicket = $"{ticketUrl}{project.Id}/{ticket.Id}",
                };
                var emailBody = await RazorTemplateEngine.RenderAsync("/Views/Emails/Ticket/TicketClosed.cshtml", model);
                var textVersion = "Ticket Has been closed.";
                await _notificationService.SendNotification(emailTo, ccEmails, emailBody, textVersion, "Harvest Notification - Ticket Closed");
            }
            catch (Exception e)
            {
                Log.Error("Error trying to email Close Ticket notification", e);
                return false;
            }

            return true;
        }

        public async Task<int> SendExpiringProjectsNotification(int days = 7)
        {
            try
            {
                var emailTo = await FieldManagersEmails();
                var model = await _dbContext.Projects.Where(a => a.IsActive && a.Status == Project.Statuses.Active && a.End <= DateTime.UtcNow.AddDays(days))
                    .OrderBy(a => a.End).Select(s => new ExpiringProjectsModel
                    {
                        EndDate = s.End.ToShortDateString(),
                        Name = s.Name,
                        ProjectUrl = $"{_emailSettings.BaseUrl}/Project/Details/{s.Id}"
                    }).ToArrayAsync();
                if (model == null || model.Length == 0)
                {
                    Log.Information($"No projects have expired or will expire in {days} days");
                    return 0;
                }
                var emailBody = await RazorTemplateEngine.RenderAsync("/Views/Emails/ExpiringProjects.cshtml", model);
                var textVersion = $"One or more projects have expired or will expire in {days} days.";
                await _notificationService.SendNotification(emailTo, null, emailBody, textVersion, "Harvest Notification - Expiring Projects");
                return model.Length;
            }
            catch (Exception ex)
            {
                Log.Error("Error emailing expiring projects", ex);
            }

            return 0;
        }
    }
}
