using Harvest.Core.Domain;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Linq.Expressions;

namespace Harvest.Web.Models.ReportModels
{
    public class UnbilledExpensesReportModel
    {
        public List<UnbilledExpenseProjectReportRowModel> Projects { get; set; } = new();

        public string TeamName { get; set; }
    }

    public class UnbilledExpenseProjectReportRowModel
    {
        [Display(Name = "Project Id")]
        public int ProjectId { get; set; }

        [Display(Name = "Project Name")]
        public string ProjectName { get; set; }

        [Display(Name = "Total Unbilled Expenses")]
        public decimal TotalUnbilledExpenses { get; set; }

        [Display(Name = "Project Amount Already Billed")]
        public decimal ProjectAmountBilled { get; set; }

        [Display(Name = "Quote Amount")]
        public decimal QuoteAmount { get; set; }

        [Display(Name = "Remaining Quote")]
        public decimal RemainingQuote { get; set; }

        [Display(Name = "Exceeds Remaining Quote")]
        public bool WillExceedRemainingQuote { get; set; }

        public static Expression<Func<Project, UnbilledExpenseProjectReportRowModel>> Projection()
        {
            return project => new UnbilledExpenseProjectReportRowModel
            {
                ProjectId = project.Id,
                ProjectName = project.Name,
                TotalUnbilledExpenses = project.Expenses.Where(a => a.InvoiceId == null && a.Approved).Sum(a => a.Total),
                ProjectAmountBilled = project.Invoices.Where(a => a.Status != Invoice.Statuses.Cancelled).Sum(a => a.Total),
                QuoteAmount = project.QuoteTotal,
                RemainingQuote = project.QuoteTotal - project.Invoices.Where(a => a.Status != Invoice.Statuses.Cancelled).Sum(a => a.Total),
                WillExceedRemainingQuote = project.Expenses.Where(a => a.InvoiceId == null && a.Approved).Sum(a => a.Total) > (project.QuoteTotal - project.Invoices.Where(a => a.Status != Invoice.Statuses.Cancelled).Sum(a => a.Total))
            };
        }
    }
}
