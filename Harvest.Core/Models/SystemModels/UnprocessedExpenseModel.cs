using Harvest.Core.Domain;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Harvest.Core.Models.SystemModels
{
    public class UnprocessedExpensesModel
    {
        [DisplayName("Project")]
        public string ProjectName { get; set; }
        public string ProjectStatus { get; set; }
        public int ProjectId { get; set; }
        public string InvoiceStatus { get; set; }
        public int RateId { get; set; } //Rate Id
        public string Description { get; set; }

        public decimal Total { get; set; }
        [DisplayName("Pass")]
        public bool IsPassthrough { get; set; }
        public string Account { get; set; }
        public string RateAccount { get; set; }
        [DisplayName("Same")]
        public string IsSame { get; set; }

        [DisplayName("Team")]
        public string TeamSlug { get; set; }

        public static Expression<Func<Expense, UnprocessedExpensesModel>> Projection()
        {
            return a => new UnprocessedExpensesModel
            {
                ProjectName = a.Project.Name,
                ProjectStatus = a.Project.Status,
                ProjectId = a.ProjectId,
                InvoiceStatus = a.Invoice != null ? a.Invoice.Status : "Not Created",
                RateId = a.RateId,
                Description = a.Description,
                Total = a.Total,
                IsPassthrough = a.IsPassthrough,
                Account = a.Account,
                RateAccount = a.Rate.Account,
                IsSame = a.Rate.Account == a.Account ? "Yes" : "No",
                TeamSlug = a.Project.Team.Slug
            };
        }
    }
}
