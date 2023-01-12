using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
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
    }
}
