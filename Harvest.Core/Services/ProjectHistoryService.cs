using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Principal;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Harvest.Core.Data;
using Harvest.Core.Domain;
using Harvest.Core.Extensions;
using Harvest.Core.Models;
using Harvest.Core.Models.History;
using Harvest.Core.Models.InvoiceModels;
using Harvest.Core.Utilities;

namespace Harvest.Core.Services
{
    public interface IProjectHistoryService
    {
        Task<ProjectHistory> AccountChanged(int projectId, IEnumerable<Account> accounts);
        Task<ProjectHistory> AcreageExpenseCreated(int projectId, Expense expense);
        Task<ProjectHistory> ExpensesCreated(int projectId, IEnumerable<Expense> expenses);
        Task<ProjectHistory> ExpenseDeleted(int projectId, Expense expense);
        Task<ProjectHistory> FinalInvoicePending(int projectId, Invoice invoice);
        Task<ProjectHistory> InvoiceCancelled(int projectId, Invoice invoice);
        Task<ProjectHistory> InvoiceCompleted(int projectId, Invoice invoice);
        Task<ProjectHistory> InvoiceCreated(int projectId, Invoice invoice);
        Task<ProjectHistory> MoveMoneyRequested(int projectId, Invoice invoice);
        Task<ProjectHistory> ProjectCloseoutInitiated(int projectId, Project project);
        Task<ProjectHistory> ProjectCompleted(int projectId, Project project);
        Task<ProjectHistory> ProjectRequestCanceled(int projectId, Project project);
        Task<ProjectHistory> ProjectFilesAttached(int projectId, IEnumerable<ProjectAttachment> attachments);
        Task<ProjectHistory> ProjectTotalRefreshed(int projectId, Project project);
        Task<ProjectHistory> QuoteApproved(int projectId, IEnumerable<Account> accounts);
        Task<ProjectHistory> QuoteRejected(int projectId, string reason);
        Task<ProjectHistory> QuoteSaved(int projectId, Quote quote);
        Task<ProjectHistory> QuoteSubmitted(int projectId, Quote quote);
        Task<ProjectHistory> RequestCreated(Project project);
        Task<ProjectHistory> TicketCreated(int projectId, Ticket ticket);
        Task<ProjectHistory> TicketFilesAttached(int projectId, IEnumerable<TicketAttachment> attachments);
        Task<ProjectHistory> TicketNotesUpdated(int projectId, Ticket ticket);
        Task<ProjectHistory> TicketReplyCreated(int projectId, TicketMessage message);
    }

    public class ProjectHistoryService : IProjectHistoryService
    {
        private readonly IUserService _userService;
        private readonly AppDbContext _dbContext;
        private readonly JsonSerializerOptions _jsonOptions;

        public ProjectHistoryService(IUserService userService, AppDbContext dbContext)
        {
            _userService = userService;
            _dbContext = dbContext;
            _jsonOptions = JsonOptions.Standard.WithGeoJson();
        }

        public Task<ProjectHistory> AccountChanged(int projectId, IEnumerable<Account> accounts) =>
            MakeHistory(projectId, nameof(AccountChanged), accounts.Select(a => new AccountHistoryModel(a)));
        public Task<ProjectHistory> AcreageExpenseCreated(int projectId, Expense expense) =>
            MakeHistory(projectId, nameof(AcreageExpenseCreated), new ExpenseModel(expense));
        public Task<ProjectHistory> ExpensesCreated(int projectId, IEnumerable<Expense> expenses) =>
            MakeHistory(projectId, nameof(ExpensesCreated), expenses.Select(e => new ExpenseModel(e)));
        public Task<ProjectHistory> ExpenseDeleted(int projectId, Expense expense) =>
            MakeHistory(projectId, nameof(ExpenseDeleted), new ExpenseModel(expense));
        public Task<ProjectHistory> FinalInvoicePending(int projectId, Invoice invoice) =>
            MakeHistory(projectId, nameof(FinalInvoicePending), new InvoiceModel(invoice));
        public Task<ProjectHistory> InvoiceCancelled(int projectId, Invoice invoice) =>
            MakeHistory(projectId, nameof(InvoiceCancelled), new InvoiceModel(invoice));
        public Task<ProjectHistory> InvoiceCompleted(int projectId, Invoice invoice) =>
            MakeHistory(projectId, nameof(InvoiceCompleted), new InvoiceModel(invoice));
        public Task<ProjectHistory> InvoiceCreated(int projectId, Invoice invoice) =>
            MakeHistory(projectId, nameof(InvoiceCreated), new InvoiceModel(invoice));
        public Task<ProjectHistory> MoveMoneyRequested(int projectId, Invoice invoice) =>
            MakeHistory(projectId, nameof(MoveMoneyRequested), new InvoiceModel(invoice));
        public Task<ProjectHistory> ProjectCloseoutInitiated(int projectId, Project project) =>
            MakeHistory(projectId, nameof(ProjectCloseoutInitiated), new ProjectHistoryModel(project));
        public Task<ProjectHistory> ProjectCompleted(int projectId, Project project) =>
            MakeHistory(projectId, nameof(ProjectCompleted), new ProjectHistoryModel(project));
        public Task<ProjectHistory> ProjectRequestCanceled(int projectId, Project project) =>
            MakeHistory(projectId, nameof(ProjectRequestCanceled), new ProjectHistoryModel(project));
        public Task<ProjectHistory> ProjectFilesAttached(int projectId, IEnumerable<ProjectAttachment> attachments) =>
            MakeHistory(projectId, nameof(ProjectFilesAttached), attachments.Select(a => new FileHistoryModel(a)));
        public Task<ProjectHistory> ProjectTotalRefreshed(int projectId, Project project) =>
            MakeHistory(projectId, nameof(ProjectTotalRefreshed), new ProjectTotalHistoryModel(project));
        public Task<ProjectHistory> QuoteSaved(int projectId, Quote quote) =>
            MakeHistory(projectId, nameof(QuoteSaved), quote.Text); // quote.Text is a preserialized QuoteDetail
        public Task<ProjectHistory> QuoteSubmitted(int projectId, Quote quote) =>
            MakeHistory(projectId, nameof(QuoteSubmitted), quote.Text); // quote.Text is a preserialized QuoteDetail
        public Task<ProjectHistory> RequestCreated(Project project) =>
            MakeHistory(project, nameof(RequestCreated), new ProjectHistoryModel(project));
        public Task<ProjectHistory> QuoteApproved(int projectId, IEnumerable<Account> accounts) =>
            MakeHistory(projectId, nameof(QuoteApproved), accounts.Select(a => new AccountHistoryModel(a)));
        public Task<ProjectHistory> QuoteRejected(int projectId, string reason) =>
            MakeHistory(projectId, nameof(QuoteRejected), reason);
        public Task<ProjectHistory> TicketCreated(int projectId, Ticket ticket) =>
            MakeHistory(projectId, nameof(TicketCreated), new TicketHistoryModel(ticket));
        public Task<ProjectHistory> TicketFilesAttached(int projectId, IEnumerable<TicketAttachment> attachments) =>
            MakeHistory(projectId, nameof(TicketFilesAttached), attachments.Select(a => new FileHistoryModel(a)));
        public Task<ProjectHistory> TicketNotesUpdated(int projectId, Ticket ticket) =>
            MakeHistory(projectId, nameof(TicketNotesUpdated), new TicketHistoryModel(ticket));
        public Task<ProjectHistory> TicketReplyCreated(int projectId, TicketMessage message) =>
            MakeHistory(projectId, nameof(TicketReplyCreated), new TicketMessageHistoryModel(message));


        private Task<ProjectHistory> MakeHistory(int projectId, string action, object detailsModel) =>
            MakeHistory(projectId, action, JsonSerializer.Serialize(detailsModel, _jsonOptions));

        private Task<ProjectHistory> MakeHistory(Project project, string action, object detailsModel) =>
            MakeHistory(project, action, JsonSerializer.Serialize(detailsModel, _jsonOptions));

        private async Task<ProjectHistory> MakeHistory(int projectId, string action, string detailsSerialized)
        {

            var projectHistory = new ProjectHistory
            {
                Action = action,
                Description = action.SplitCamelCase(),
                Details = detailsSerialized,
                ProjectId = projectId,
                ActionDate = DateTime.UtcNow,
                ActorId = (await _userService.GetCurrentUser())?.Id,
            };
            _dbContext.Add(projectHistory);
            return projectHistory;
        }

        private async Task<ProjectHistory> MakeHistory(Project project, string action, string detailsSerialized)
        {

            var projectHistory = new ProjectHistory
            {
                Action = action,
                Description = action.SplitCamelCase(),
                Details = detailsSerialized,
                Project = project,
                ActionDate = DateTime.UtcNow,
                ActorId = (await _userService.GetCurrentUser())?.Id,
            };
            _dbContext.Add(projectHistory);
            return projectHistory;
        }
    }
}
