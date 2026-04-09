using Harvest.Core.Domain;
using Harvest.Core.Extensions;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;

namespace Harvest.Web.Models.ReportModels
{
    public class UnbilledExpensesReportModel
    {
        public List<UnbilledExpenseRowModel> Expenses { get; set; } = new();

        public string TeamName { get; set; }
        public string Slug { get; set; }
    }

    public class UnbilledExpenseRowModel
    {
        [Display(Name = "Project Id")]
        public int ProjectId { get; set; }

        [Display(Name = "Project Name")]
        public string ProjectName { get; set; }

        public string Activity { get; set; }

        public string Type { get; set; }

        public string Description { get; set; }

        public decimal Quantity { get; set; }

        public bool Markup { get; set; }

        [Display(Name = "Pass Through")]
        public bool IsPassthrough { get; set; }

        public string Account { get; set; }

        [Display(Name = "Rate")]
        public string RateName { get; set; }

        [Display(Name = "Unit Rate")]
        public decimal Price { get; set; }

        public decimal Total { get; set; }

        public DateTime CreatedOn { get; set; }

        [Display(Name = "Entered On")]
        [DisplayFormat(DataFormatString = "{0:g}")]
        public DateTime CreatedOnLocal { get; set; }

        [Display(Name = "Entered By")]
        public string CreatedByName { get; set; }

        public bool Approved { get; set; }

        [Display(Name = "Approved By")]
        public string ApprovedByName { get; set; }

        public DateTime? ApprovedOn { get; set; }

        [Display(Name = "Approved On")]
        [DisplayFormat(DataFormatString = "{0:g}")]
        public DateTime? ApprovedOnLocal { get; set; }

        public static Expression<Func<Expense, UnbilledExpenseRowModel>> Projection()
        {
            return expense => new UnbilledExpenseRowModel
            {
                ProjectId = expense.ProjectId,
                ProjectName = expense.Project != null ? expense.Project.Name : string.Empty,
                Activity = expense.Activity,
                Type = expense.Type,
                Description = expense.Description,
                Quantity = expense.Quantity,
                Markup = expense.Markup,
                IsPassthrough = expense.IsPassthrough,
                Account = expense.Account,
                RateName = expense.Rate != null ? expense.Rate.Description : string.Empty,
                Price = expense.Price,
                Total = expense.Total,
                CreatedOn = expense.CreatedOn,
                CreatedOnLocal = expense.CreatedOn,
                CreatedByName = expense.CreatedBy != null ? expense.CreatedBy.Name : string.Empty,
                Approved = expense.Approved,
                ApprovedByName = expense.ApprovedBy != null ? expense.ApprovedBy.Name : string.Empty,
                ApprovedOn = expense.ApprovedOn,
                ApprovedOnLocal = expense.ApprovedOn
            };
        }
    }
}
