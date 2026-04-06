using Harvest.Core.Domain;
using Harvest.Core.Extensions;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Linq.Expressions;

namespace Harvest.Web.Models.ReportModels
{
    public class UnbilledExpensesReportModel
    {
        public List<UnbilledExpenseReportRowModel> Expenses { get; set; } = new();

        public string TeamName { get; set; }

        public decimal GrandTotal => Expenses.Sum(a => a.Total);

        public List<UnbilledExpenseProjectGroupModel> ProjectGroups => Expenses
            .GroupBy(a => new
            {
                a.ProjectId,
                a.ProjectName,
                a.ProjectAmountBilled,
                a.QuoteAmount,
                a.RemainingQuote,
                a.ProjectUnbilledTotal,
                a.WillExceedRemainingQuote
            })
            .Select(a => new UnbilledExpenseProjectGroupModel
            {
                ProjectId = a.Key.ProjectId,
                ProjectName = a.Key.ProjectName,
                ProjectAmountBilled = a.Key.ProjectAmountBilled,
                QuoteAmount = a.Key.QuoteAmount,
                RemainingQuote = a.Key.RemainingQuote,
                ProjectUnbilledTotal = a.Key.ProjectUnbilledTotal,
                WillExceedRemainingQuote = a.Key.WillExceedRemainingQuote,
                Expenses = a.OrderBy(e => e.CreatedOn).ThenBy(e => e.ExpenseId).ToList()
            })
            .OrderBy(a => a.ProjectName)
            .ThenBy(a => a.ProjectId)
            .ToList();
    }

    public class UnbilledExpenseProjectGroupModel
    {
        public int ProjectId { get; set; }
        public string ProjectName { get; set; }
        public decimal ProjectAmountBilled { get; set; }
        public decimal QuoteAmount { get; set; }
        public decimal RemainingQuote { get; set; }
        public decimal ProjectUnbilledTotal { get; set; }
        public bool WillExceedRemainingQuote { get; set; }
        public List<UnbilledExpenseReportRowModel> Expenses { get; set; } = new();
    }

    public class UnbilledExpenseReportRowModel
    {
        [Display(Name = "Project Id")]
        public int ProjectId { get; set; }

        [Display(Name = "Project Name")]
        public string ProjectName { get; set; }

        [Display(Name = "Expense Id")]
        public int ExpenseId { get; set; }

        [Display(Name = "Created")]
        [DataType(DataType.Date)]
        public DateTime CreatedOn { get; set; }

        [Display(Name = "Approved")]
        public string IsApproved { get; set; }

        [Display(Name = "Type")]
        public string ExpenseType { get; set; }

        public string Activity { get; set; }

        public string Description { get; set; }

        public string Account { get; set; }

        [Display(Name = "Entered By")]
        public string CreatedByName { get; set; }

        public decimal Quantity { get; set; }

        [Display(Name = "Rate")]
        public decimal Price { get; set; }

        [Display(Name = "Expense Total")]
        public decimal Total { get; set; }

        [Display(Name = "Project Amount Already Billed")]
        public decimal ProjectAmountBilled { get; set; }

        [Display(Name = "Quote Amount")]
        public decimal QuoteAmount { get; set; }

        [Display(Name = "Remaining Quote")]
        public decimal RemainingQuote { get; set; }

        [Display(Name = "Project Unbilled Total")]
        public decimal ProjectUnbilledTotal { get; set; }

        [Display(Name = "Exceeds Remaining Quote")]
        public bool WillExceedRemainingQuote { get; set; }

        public static Expression<Func<Expense, UnbilledExpenseReportRowModel>> Projection()
        {
            return expense => new UnbilledExpenseReportRowModel
            {
                ProjectId = expense.ProjectId,
                ProjectName = expense.Project.Name,
                ExpenseId = expense.Id,
                CreatedOn = expense.CreatedOn.ToPacificTime(),
                IsApproved = expense.Approved ? "Yes" : "No",
                ExpenseType = expense.Type,
                Activity = expense.Activity,
                Description = expense.Description,
                Account = expense.Account,
                CreatedByName = expense.CreatedBy != null ? expense.CreatedBy.FirstName + " " + expense.CreatedBy.LastName : string.Empty,
                Quantity = expense.Quantity,
                Price = expense.Price,
                Total = expense.Total,
                ProjectAmountBilled = expense.Project.Invoices.Where(a => a.Status != Invoice.Statuses.Cancelled).Sum(a => a.Total),
                QuoteAmount = expense.Project.QuoteTotal,
                RemainingQuote = expense.Project.QuoteTotal - expense.Project.Invoices.Where(a => a.Status != Invoice.Statuses.Cancelled).Sum(a => a.Total),
                ProjectUnbilledTotal = expense.Project.Expenses.Where(a => a.InvoiceId == null).Sum(a => a.Total),
                WillExceedRemainingQuote = expense.Project.Expenses.Where(a => a.InvoiceId == null).Sum(a => a.Total) > (expense.Project.QuoteTotal - expense.Project.Invoices.Where(a => a.Status != Invoice.Statuses.Cancelled).Sum(a => a.Total))
            };
        }
    }
}
