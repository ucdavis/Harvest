﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Harvest.Core.Data;
using Harvest.Core.Domain;
using Harvest.Core.Extensions;
using Harvest.Core.Models.Settings;
using Harvest.Core.Services;
using Harvest.Email.Models;
using Harvest.Email.Models.Invoice;
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
        Task<bool> SupervisorSavedQuote(Project project, Quote quote);
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
        Task<bool> AdhocProjectCreated(Project project);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="days">Specifies number of days before the project ends to include in the notification</param>
        /// <returns></returns>
        Task<int> SendExpiringProjectsNotification(int days);

        Task<bool> InvoiceCreated(Invoice invoice);
        Task<bool> InvoiceDone(Invoice invoice, string status);
        Task<bool> InvoiceError(Invoice invoice);

        Task<bool> CloseoutConfirmation(Project project, bool ccFieldManagers = true); //Project is awaiting PI to close it
        Task<bool> ProjectClosed(Project project, bool isAutoCloseout); //Project has been closed by PI

        Task<int> SendCloseoutNotifications();
    }

    public class EmailService : IEmailService
    {
        private readonly AppDbContext _dbContext;
        private readonly INotificationService _notificationService;
        private readonly IFinancialService _financialService;
        private readonly EmailSettings _emailSettings;

        public EmailService(AppDbContext dbContext, INotificationService notificationService, IOptions<EmailSettings> emailSettings, IFinancialService financialService)
        {
            _dbContext = dbContext;
            _notificationService = notificationService;
            _financialService = financialService;
            _emailSettings = emailSettings.Value;
        }
        public async Task<bool> ProfessorQuoteReady(Project project, Quote quote)
        {
            project = await CheckForMissingDataForProject(project);            

            var url = $"{_emailSettings.BaseUrl}/{project.Team.Slug}/request/approve/";
            if (quote == null)
            {
                throw new Exception("No quote.");
            }


            try
            {
                var model = new ProfessorQuoteModel()
                {
                    ProfName = project.PrincipalInvestigator.Name,
                    ProjectName = project.NameAndId,
                    ProjectStart = project.Start.ToPacificTime().Date.Format("d"),
                    ProjectEnd = project.End.ToPacificTime().Date.Format("d"),
                    QuoteAmount = quote.Total.ToString("C"),
                    ButtonUrl = $"{url}{project.Id}"
                };

                var emailBody = await RazorTemplateEngine.RenderAsync("/Views/Emails/ProfessorQuoteNotification.cshtml", model);

                await _notificationService.SendNotification(GetPiAndProjectEditorEmails(project),await FieldManagersEmails(project.TeamId), emailBody, "A quote is ready for your review/approval for your harvest project.", "Harvest Notification - Quote Ready");
            }
            catch (Exception e)
            {
                Log.Error("Error trying to email Quote", e);
                return false;
            }

            return true;

        }

        public async Task<bool> SupervisorSavedQuote(Project project, Quote quote)
        {
            project = await CheckForMissingDataForProject(project);

            var url = $"{_emailSettings.BaseUrl}/{project.Team.Slug}/quote/create/";

            if (quote == null)
            {
                throw new Exception("No quote.");
            }


            try
            {
                var model = new ProfessorQuoteModel()
                {
                    ProfName = "Field Managers",
                    ProjectName = project.NameAndId,
                    ProjectStart = project.Start.ToPacificTime().Date.Format("d"),
                    ProjectEnd = project.End.ToPacificTime().Date.Format("d"),
                    QuoteAmount = quote.Total.ToString("C"),
                    ButtonUrl = $"{url}{project.Id}",
                    ButtonQuoteText = "View Saved Quote"
                };

                var emailBody = await RazorTemplateEngine.RenderAsync("/Views/Emails/ProfessorQuoteNotification.cshtml", model);

                await _notificationService.SendNotification( await FieldManagersEmails(project.TeamId), null, emailBody, "A quote is ready for your review/approval for your harvest project.", "Harvest Notification - Supervisor Saved Quote");
            }
            catch (Exception e)
            {
                Log.Error("Error trying to email Quote", e);
                return false;
            }

            return true;

        }

        private async Task<string[]> FieldManagersEmails(int teamId)
        {
            return await _dbContext.Permissions.Where(a => a.TeamId == teamId && a.Role.Name == Role.Codes.FieldManager).Select(a => a.User.Email).ToArrayAsync();
        }

        private async Task<string[]> FieldManagersAndSupervisorEmails(int teamId)
        {
            return await _dbContext.Permissions.Where(a => a.TeamId == teamId && (a.Role.Name == Role.Codes.FieldManager || a.Role.Name == Role.Codes.Supervisor)).Select(a => a.User.Email).Distinct().ToArrayAsync();
        }

        private string[] GetPiAndProjectEditorEmails(Project project)
        {
            var emails = new List<string>
            {
                project.PrincipalInvestigator.Email
            };

            emails.AddRange(project.ProjectPermissions
                .Where(a => a.Permission == Role.Codes.ProjectEditor)
                .Select(a => a.User.Email));

            // Filter out duplicates
            return emails.Distinct().ToArray();
        }

        private string[] GetAllProjectEmails(Project project)
        {
            var emails = new List<string>
            {
                project.PrincipalInvestigator.Email
            };

            emails.AddRange(project.ProjectPermissions
                .Where(a => a.Permission == Role.Codes.ProjectEditor)
                .Select(a => a.User.Email));

            //Just to order them by "importance"
            emails.AddRange(project.ProjectPermissions
                .Where(a => a.Permission == Role.Codes.ProjectViewer)
                .Select(a => a.User.Email));

            // Filter out duplicates
            return emails.Distinct().ToArray();
        }

        public async Task<bool> NewFieldRequest(Project project)
        {
            project = await CheckForMissingDataForProject(project);

            var url = $"{_emailSettings.BaseUrl}/{project.Team.Slug}/project/details/";

            var model = new NewFieldRequestModel()
            {
                PI = project.PrincipalInvestigator.NameAndEmail,
                ProjectName = project.NameAndId,
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

                await _notificationService.SendNotification(await FieldManagersAndSupervisorEmails(project.TeamId), new []{project.PrincipalInvestigator.Email}, emailBody, "A new field request has been made.", "Harvest Notification - New Field Request");
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
            project = await CheckForMissingDataForProject(project);

            var quoteUrl   = $"{_emailSettings.BaseUrl}/{project.Team.Slug}/quote/create/";
            var projectUrl = $"{_emailSettings.BaseUrl}/{project.Team.Slug}/Project/Details/";

            var model = new ChangeRequestModel()
            {
                PI = project.PrincipalInvestigator.NameAndEmail,
                ProjectName = project.NameAndId,
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

                await _notificationService.SendNotification(await FieldManagersAndSupervisorEmails(project.TeamId), null, emailBody, textVersion, "Harvest Notification - Change Request");
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
            project = await CheckForMissingDataForProject(project);


            var url = $"{_emailSettings.BaseUrl}/{project.Team.Slug}/Project/Details/";

            var model = new QuoteDecisionModel()
            {
                PI = project.PrincipalInvestigator.NameAndEmail,
                ProjectName = project.NameAndId,
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

                //INC2065591 Brain wants supervisors to be included in the email
                await _notificationService.SendNotification(await FieldManagersAndSupervisorEmails(project.TeamId), GetPiAndProjectEditorEmails(project), emailBody, textVersion, $"Harvest Notification - Quote {model.Decision}");
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
        [Obsolete("This was not implemented. If it is, the email has to be updated with new style.")]
        public Task<bool> ApproveAccounts(Project project, string[] emails)
        {
            throw new NotImplementedException();
            //If ever implemented, this will need the team
            // var url = $"{_emailSettings.BaseUrl}/Project/AccountApproval/";

            // var model = new AccountPendingApprovalModel()
            // {
            //     PI = project.PrincipalInvestigator.NameAndEmail,
            //     ProjectName = project.Name,
            //     ProjectStart = project.Start.ToPacificTime().Date.Format("d"),
            //     ProjectEnd = project.End.ToPacificTime().Date.Format("d"),
            //     AccountsList = new List<AccountsForApprovalModel>(),
            //     ButtonUrl = $"{url}{project.Id}"
            // };
            // foreach (var account in project.Accounts.Where(a => a.ApprovedBy == null))
            // {
            //     model.AccountsList.Add(new AccountsForApprovalModel(){Account = account.Number, Name = account.Name, Percent = $"{account.Percentage}%"});
            // }

            // var textVersion = $"Accounts require your approval for use in project {model.ProjectName} by {model.PI}";

            // try
            // {
            //     var emailBody = await RazorTemplateEngine.RenderAsync("/Views/Emails/AccountPendingApproval.cshtml", model);

            //     await _notificationService.SendNotification(emails, null, emailBody, textVersion, $"Harvest Notification - Accounts need approval");
            // }
            // catch (Exception e)
            // {
            //     Log.Error("Error trying to email Quote", e);
            //     return false;
            // }

            // return true;
        }

        public async Task<bool> InvoiceExceedsQuote(Project project, decimal invoiceAmount, decimal quoteRemaining)
        {
            project = await CheckForMissingDataForProject(project);

            var url = $"{_emailSettings.BaseUrl}/{project.Team.Slug}/Project/Details/";

            var model = new InvoiceExceedsQuoteModel()
            {
                PI              = project.PrincipalInvestigator.NameAndEmail,
                ProjectName     = project.NameAndId,
                ProjectStart    = project.Start.ToPacificTime().Date.Format("d"),
                ProjectEnd      = project.End.ToPacificTime().Date.Format("d"),
                InvoiceAmount   = invoiceAmount.ToString("C"),
                RemainingAmount = quoteRemaining.ToString("C"),
                ButtonUrl       = $"{url}{project.Id}"
            };

            var textVersion = $"An invoice can't be created for project {model.ProjectName} because it exceeds the remaining amount for the quote. RED ALERT!!!";

            try
            {
                var emailBody = await RazorTemplateEngine.RenderAsync("/Views/Emails/Invoice/InvoiceExceedsRemainingAmount.cshtml", model);

                await _notificationService.SendNotification(await FieldManagersEmails(project.TeamId), null, emailBody, textVersion, $"Harvest Notification - Invoice can't be created");
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
            project = await CheckForMissingDataForProject(project);

            try
            {
                var ticketUrl = $"{_emailSettings.BaseUrl}/{project.Team.Slug}/Ticket/Details/";
                var projectUrl = $"{_emailSettings.BaseUrl}/{project.Team.Slug}/Project/Details/";
                var model = new NewTicketModel()
                {
                    ProjectName = project.NameAndId,
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
                await _notificationService.SendNotification(await FieldManagersAndSupervisorEmails(project.TeamId), GetPiAndProjectEditorEmails(project), emailBody, textVersion, "Harvest Notification - New Ticket");
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
            project = await CheckForMissingDataForProject(project);

            //if ticketMessage.createdby == project.pi, email fieldManages emails, otherwise email PI
            //Above, not anymore because we want to allow Project Editors to reply to tickets and they are not PI.
            //Possibly someone can be both a field manager and the PI/Project Editor... But don't care about that edge case.
            var piAndProjectEditorsEmails = GetPiAndProjectEditorEmails(project);


            try
            {
                var emailTo = await FieldManagersAndSupervisorEmails(project.TeamId);
                string[] ccEmails = piAndProjectEditorsEmails;
                if (!piAndProjectEditorsEmails.Contains(ticketMessage.CreatedBy.Email))
                {
                    ccEmails = emailTo;
                    emailTo = piAndProjectEditorsEmails;
                }
                var ticketUrl = $"{_emailSettings.BaseUrl}/{project.Team.Slug}/Ticket/Details/";
                var projectUrl = $"{_emailSettings.BaseUrl}/{project.Team.Slug}/Project/Details/";
                var model = new TicketReplyModel()
                {
                    ProjectName = project.NameAndId,
                    From = ticketMessage.CreatedBy.NameAndEmail,
                    CreatedOn = ticketMessage.CreatedOn.ToPacificTime().Date.Format("d"),
                    Subject = ticket.Name,
                    Reply = ticketMessage.Message,
                    ButtonUrlForProject = $"{projectUrl}{project.Id}",
                    ButtonUrlForTicket = $"{ticketUrl}{project.Id}/{ticket.Id}",
                };
                var emailBody = await RazorTemplateEngine.RenderAsync("/Views/Emails/Ticket/TicketReply.cshtml", model);
                var textVersion = $"A reply to the ticket in the project {model.ProjectName} by {model.From}";
                await _notificationService.SendNotification(emailTo, ccEmails, emailBody, textVersion, "Harvest Notification - Ticket Reply");
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
            project = await CheckForMissingDataForProject(project);
            var piAndProjectEditorsEmails = GetPiAndProjectEditorEmails(project);

            //if ticketattachments[0].createdby == project.pi, email fieldManages emails, otherwise email PI
            try
            {
                var emailTo = await FieldManagersAndSupervisorEmails(project.TeamId);
                string[] ccEmails = piAndProjectEditorsEmails;
                var firstAttachment = ticketAttachments.First();
                if (!piAndProjectEditorsEmails.Contains(firstAttachment.CreatedBy.Email))
                {
                    ccEmails = emailTo;
                    emailTo = piAndProjectEditorsEmails;
                }
                var ticketUrl = $"{_emailSettings.BaseUrl}/{project.Team.Slug}/Ticket/Details/";
                var projectUrl = $"{_emailSettings.BaseUrl}/{project.Team.Slug}/Project/Details/";
                var model = new TicketAttachmentModel()
                {
                    ProjectName = project.NameAndId,
                    From = firstAttachment.CreatedBy.NameAndEmail,
                    CreatedOn = firstAttachment.CreatedOn.ToPacificTime().Date.Format("d"),
                    Subject = ticket.Name,   
                    AttachmentNames = ticketAttachments.Select(x => x.FileName).ToArray(),         
                    ButtonUrlForProject = $"{projectUrl}{project.Id}",
                    ButtonUrlForTicket = $"{ticketUrl}{project.Id}/{ticket.Id}",
                };
                var emailBody = await RazorTemplateEngine.RenderAsync("/Views/Emails/Ticket/TicketAttachment.cshtml", model);
                var textVersion = $"An attachment was added to the ticket in the project {model.ProjectName} by {model.From}";
                await _notificationService.SendNotification(emailTo, ccEmails, emailBody, textVersion, "Harvest Notification - Ticket Attachment Added");
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
            project = await CheckForMissingDataForProject(project);
            var piAndProjectEditorsEmails = GetPiAndProjectEditorEmails(project);

            try
            {
                string[] emailTo = null;
                string[] ccEmails = null;
                if (piAndProjectEditorsEmails.Contains(closedBy.Email))
                {
                    emailTo = await FieldManagersAndSupervisorEmails(project.TeamId);
                    ccEmails = piAndProjectEditorsEmails;
                }
                else
                {
                    emailTo = piAndProjectEditorsEmails;
                    ccEmails = await FieldManagersAndSupervisorEmails(project.TeamId);
                }

                var ticketUrl = $"{_emailSettings.BaseUrl}/{project.Team.Slug}/Ticket/Details/";
                var projectUrl = $"{_emailSettings.BaseUrl}/{project.Team.Slug}/Project/Details/";
                var model = new TicketReplyModel()
                {
                    ProjectName = project.NameAndId,
                    Subject = ticket.Name,
                    ButtonUrlForProject = $"{projectUrl}{project.Id}",
                    ButtonUrlForTicket = $"{ticketUrl}{project.Id}/{ticket.Id}",
                    From = closedBy.NameAndEmail,
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
            var total = 0;
            try
            {
                //Could use fancy groupby here, but don't really see the need.
                var teams = await _dbContext.Teams.ToArrayAsync();
                foreach (var team in teams)
                {
                    var model = await _dbContext.Projects.Where(a => a.IsActive && a.TeamId == team.Id && a.Status == Project.Statuses.Active && a.End <= DateTime.UtcNow.AddDays(days))
                        .OrderBy(a => a.End).Select(s => new ExpiringProjectsModel
                        {
                            EndDate = s.End.ToShortDateString(),
                            Name = s.NameAndId,
                            ProjectUrl = $"{_emailSettings.BaseUrl}/{team.Slug}/Project/Details/{s.Id}"
                        }).ToArrayAsync();
                    if (model == null || model.Length == 0)
                    {
                        Log.Information($"No projects for team {team.Name} have expired or will expire in {days} days");
                        continue;
                    }
                    var emailTo = await FieldManagersEmails(team.Id);
                    var emailBody = await RazorTemplateEngine.RenderAsync("/Views/Emails/ExpiringProjects.cshtml", model);
                    var textVersion = $"One or more projects for team {team.Name} have expired or will expire in {days} days.";
                    await _notificationService.SendNotification(emailTo, null, emailBody, textVersion, $"Harvest Notification - Expiring Projects for Team {team.Name}");

                    Log.Information($"Projects for team {team.Slug} close to closeout: {model.Length}");
                    total += model.Length;
                }

                return total;
            }
            catch (Exception ex)
            {
                Log.Error("Error emailing expiring projects", ex);
            }

            return 0;
        }

        public async Task<bool> InvoiceCreated(Invoice invoice)
        {
            
            try
            {
                var project = await CheckForMissingDataForInvoice(invoice); //Might need to include ProjectPermissions here.

                var projectUrl = $"{_emailSettings.BaseUrl}/{project.Team.Slug}/Project/Details/";
                var invoiceUrl = $"{_emailSettings.BaseUrl}/{project.Team.Slug}/Invoice/Details/";
                
                var emailTo = GetAllProjectEmails(project);
                //CC field managers? I'd say no....

                var model = new InvoiceEmailModel()
                {
                    InvoiceAmount = invoice.Total.ToString("C"),
                    InvoiceStatus = invoice.Status.SafeHumanizeTitle(),
                    InvoiceId = invoice.Id,
                    InvoiceCreatedOn = invoice.CreatedOn.ToPacificTime().Date.Format("d"),
                    ProjectName = project.NameAndId,
                    Title = $"Invoice {invoice.Id} Has Been Created.",
                    ButtonUrlForProject = $"{projectUrl}{project.Id}",
                    ButtonUrlForInvoice = $"{invoiceUrl}{project.Id}/{invoice.Id}",
                    PiName = project.PrincipalInvestigator.Name,
                };

                var emailBody = await RazorTemplateEngine.RenderAsync("/Views/Emails/Invoice/InvoiceCreated.cshtml", model);
                var textVersion = $"Invoice for project {model.ProjectName} has been created.";
                await _notificationService.SendNotification(emailTo, null, emailBody, textVersion, "Harvest Notification - Invoice Created");
            }
            catch (Exception ex)
            {
                Log.Error("Error emailing invoice created", ex);
                return false;
            }

            return true;
        }

        public async Task<bool> InvoiceDone(Invoice invoice, string status) //Status like Completed or Cancelled
        {
            try
            {
                var project = await CheckForMissingDataForInvoice(invoice);

                var projectUrl = $"{_emailSettings.BaseUrl}/{project.Team.Slug}/Project/Details/";
                var invoiceUrl = $"{_emailSettings.BaseUrl}/{project.Team.Slug}/Invoice/Details/";
                
                var emailTo = GetAllProjectEmails(project);
                //CC field managers? I'd say no....

                var model = new InvoiceEmailModel()
                {
                    InvoiceAmount = invoice.Total.ToString("C"),
                    InvoiceStatus = status.SafeHumanizeTitle(),
                    InvoiceId = invoice.Id,
                    InvoiceCreatedOn = invoice.CreatedOn.ToPacificTime().Date.Format("d"),
                    ProjectName = project.NameAndId,
                    Title = $"Invoice {invoice.Id} Has Been Processed.",
                    ButtonUrlForProject = $"{projectUrl}{project.Id}",
                    ButtonUrlForInvoice = $"{invoiceUrl}{project.Id}/{invoice.Id}",
                    PiName = project.PrincipalInvestigator.Name,
                }; 

                var emailBody = await RazorTemplateEngine.RenderAsync("/Views/Emails/Invoice/InvoiceCreated.cshtml", model);
                var textVersion = $"Invoice for project {model.ProjectName} has been processed.";
                await _notificationService.SendNotification(emailTo, null, emailBody, textVersion, "Harvest Notification - Invoice Processed");
            }
            catch (Exception ex)
            {
                Log.Error("Error emailing invoice done", ex);
                return false;
            }

            return true;
        }

        public async Task<bool> InvoiceError(Invoice invoice)
        {
            try
            {
                var project = await CheckForMissingDataForInvoice(invoice);

                var projectUrl = $"{_emailSettings.BaseUrl}/{project.Team.Slug}/Request/ChangeAccount/";
                var invoiceUrl = $"{_emailSettings.BaseUrl}/{project.Team.Slug}/Invoice/Details/";
                
                var emailTo = GetAllProjectEmails(project);
                var ccEmails = await FieldManagersEmails(project.TeamId);


                var model = new InvoiceErrorModel()
                {
                    InvoiceAmount = invoice.Total.ToString("C"),
                    InvoiceStatus = invoice.Status.SafeHumanizeTitle(),
                    InvoiceId = invoice.Id,
                    InvoiceCreatedOn = invoice.CreatedOn.ToPacificTime().Date.Format("d"),
                    ProjectName = project.NameAndId,
                    Title = $"Your project has one or more invalid accounts preventing Invoice {invoice.Id} from being processed.",
                    ButtonUrlForProject = $"{projectUrl}{project.Id}",
                    ButtonProjectText = "Update Accounts",
                    ButtonUrlForInvoice = $"{invoiceUrl}{project.Id}/{invoice.Id}",
                    PiName = project.PrincipalInvestigator.Name,
                    AccountsList = new List<AccountsInProjectModel>()
                };

                foreach (var projectAccount in project.Accounts)
                {
                    var account = await _financialService.IsValid(projectAccount.Number);
                    model.AccountsList.Add(new AccountsInProjectModel
                    {
                        Account = projectAccount.Number,
                        Message = account.IsValid ? "Valid" : account.Message
                    });
                }

                var emailBody = await RazorTemplateEngine.RenderAsync("/Views/Emails/Invoice/InvoiceErrors.cshtml", model);
                var textVersion = $"Invoice for project {model.ProjectName} has account Errors.";
                await _notificationService.SendNotification(emailTo, ccEmails, emailBody, textVersion, "Harvest Notification - Invoice Account Errors");
            }
            catch (Exception ex)
            {
                Log.Error("Error emailing invoice error", ex);
                return false;
            }

            return true;
        }

        public async Task<bool> CloseoutConfirmation(Project project, bool ccFieldManagers = true)
        {
            try
            {
                project = await CheckForMissingDataForProject(project);

                var projectUrl = $"{_emailSettings.BaseUrl}/{project.Team.Slug}/Project/Details/";
                var closeoutUrl = $"{_emailSettings.BaseUrl}/{project.Team.Slug}/Project/CloseoutConfirmation/";
                var emailTo = GetPiAndProjectEditorEmails(project);
                var ccEmails = ccFieldManagers ? await FieldManagersEmails(project.TeamId) : null;

                var model = new CloseoutConfirmationModel()
                {
                    PI = project.PrincipalInvestigator.NameAndEmail,
                    ProjectName = project.NameAndId,
                    ProjectStart = project.Start.ToPacificTime().Date.Format("d"),
                    ProjectEnd = project.End.ToPacificTime().Date.Format("d"),
                    ButtonUrl1 = $"{projectUrl}{project.Id}",
                    ButtonText1 = "View Project",
                    ButtonUrl2 = $"{closeoutUrl}{project.Id}",
                    ButtonText2 = "Approve Closeout",
                };

                var emailBody = await RazorTemplateEngine.RenderAsync("/Views/Emails/CloseoutConfirmation.cshtml", model);
                var textVersion = $"Project Closeout Confirmation Requested for {model.ProjectName}.";
                await _notificationService.SendNotification(emailTo, ccEmails, emailBody, textVersion, "Harvest Notification - Project Closeout Confirmation Requested");

            }
            catch (Exception ex)
            {
                Log.Error("Error emailing CloseoutConfirmation ", ex);
                return false;
            }

            return true;
        }

        public async Task<int> SendCloseoutNotifications()
        {
            var count = 0;
            var projectsAwaitingCloseout = await _dbContext.Projects.Include(a => a.PrincipalInvestigator).Include(a => a.Team).Where(a => a.IsActive && a.Status == Project.Statuses.PendingCloseoutApproval).ToArrayAsync();
            if (projectsAwaitingCloseout.Any())
            {
                foreach(var project in projectsAwaitingCloseout)
                {
                    if(await CloseoutConfirmation(project, false))
                    {
                        count++;
                    }
                }
            }

            return count;
        }

        public async Task<bool> ProjectClosed(Project project, bool isAutoCloseout)
        {
            try
            {
                project = await CheckForMissingDataForProject(project);

                var projectUrl = $"{_emailSettings.BaseUrl}/{project.Team.Slug}/Project/Details/";
                var emailTo = await FieldManagersEmails(project.TeamId);
                var ccEmails = GetPiAndProjectEditorEmails(project);

                var model = new ProjectClosedModel()
                {
                    PI = project.PrincipalInvestigator.NameAndEmail,
                    ProjectName = project.NameAndId,
                    ProjectStart = project.Start.ToPacificTime().Date.Format("d"),
                    ProjectEnd = project.End.ToPacificTime().Date.Format("d"),
                    ButtonUrl1 = $"{projectUrl}{project.Id}",
                    ButtonText1 = "View Project",
                    NotificationText = isAutoCloseout ? "your project as been closed-out automatically." : "or a Project Editor has approved the closeout of the project.",
                };

                var emailBody = await RazorTemplateEngine.RenderAsync("/Views/Emails/ProjectClosed.cshtml", model);
                var textVersion = $"Project {model.ProjectName} Closed.";
                await _notificationService.SendNotification(emailTo, ccEmails, emailBody, textVersion, "Harvest Notification - Project Closed");

            }
            catch (Exception ex)
            {
                Log.Error("Error emailing CloseoutConfirmation ", ex);
                return false;
            }

            return true;
        }

        /// <summary>
        /// Gets missing info if it wasn't included in the initial call
        /// </summary>
        /// <param name="invoice"></param>
        /// <returns></returns>
        private async Task<Project> CheckForMissingDataForInvoice(Invoice invoice)
        {
            var project = invoice.Project;
            if (project == null || project.PrincipalInvestigator == null || project.Accounts == null)
            {
                project = await _dbContext.Projects.AsNoTracking().Include(a => a.PrincipalInvestigator).Include(a => a.Accounts)
                    .SingleAsync(a => a.Id == invoice.ProjectId);
            }
            if (project.Team == null)
            {
                project.Team = await _dbContext.Teams.AsNoTracking().SingleAsync(a => a.Id == project.TeamId);
            }

            if(project.ProjectPermissions == null || project.ProjectPermissions.Count() <= 0)
            {
                project.ProjectPermissions = await _dbContext.ProjectPermissions.Include(a => a.User).AsNoTracking()
                    .Where(a => a.ProjectId == project.Id).ToListAsync();
            }

            return project;
        }

        private async Task<Project> CheckForMissingDataForProject(Project project)
        {
            if (project == null)
            {
                throw new Exception("Project is null");
            }

            if (project.PrincipalInvestigator == null || project.Accounts == null)
            {
                project = await _dbContext.Projects.AsNoTracking().Include(a => a.PrincipalInvestigator).Include(a => a.Accounts)
                    .SingleAsync(a => a.Id == project.Id);
            }

            if(project.Team == null)
            {
                project.Team = await _dbContext.Teams.AsNoTracking().SingleAsync(a => a.Id == project.TeamId);
            }

            //I don't know if the null check would find it, so populate this.
            if(project.ProjectPermissions == null || project.ProjectPermissions.Count() <= 0)
            {
                project.ProjectPermissions = await _dbContext.ProjectPermissions.Include(a => a.User).AsNoTracking()
                    .Where(a => a.ProjectId == project.Id).ToListAsync();
            }

            return project;
        }

        public async Task<bool> AdhocProjectCreated(Project project)
        {
            project = await CheckForMissingDataForProject(project);

            var url = $"{_emailSettings.BaseUrl}/{project.Team.Slug}/project/details/";

            var model = new NewFieldRequestModel()
            {
                PI = project.PrincipalInvestigator.NameAndEmail,
                ProjectName = project.NameAndId,
                ProjectStart = project.Start.ToPacificTime().Date.Format("d"),
                ProjectEnd = project.End.ToPacificTime().Date.Format("d"), //Not used in the email...
                CropType = project.CropType,
                Crops = project.Crop,
                Requirements = project.Requirements,
                ButtonUrl = $"{url}{project.Id}"
            };

            try
            {
                var emailBody = await RazorTemplateEngine.RenderAsync("/Views/Emails/AdhocProjectCreated.cshtml", model);

                await _notificationService.SendNotification(await FieldManagersEmails(project.TeamId), new[] { project.PrincipalInvestigator.Email }, emailBody, "An Ad-hoc project for billing purposes has been created", "Harvest Notification - Ad-hoc project created");
            }
            catch (Exception e)
            {
                Log.Error("Error trying to email Ad-hoc project", e);
                return false;
            }

            return true;
        }
    }
}
