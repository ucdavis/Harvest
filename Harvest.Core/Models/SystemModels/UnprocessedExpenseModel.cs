using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Harvest.Core.Models.SystemModels
{
    public class UnprocessedExpensesModel
    {
        public string ProjectName { get; set; }
        public string ProjectStatus { get; set; }
        public int ProjectId { get; set; }
        public string InvoiceStatus { get; set; }
        public int Id { get; set; } //Rate Id
        public string Description { get; set; }

        public decimal Total { get; set; }
        public bool IsPassthrough { get; set; }
        public string Account { get; set; }
        public string RateAccount { get; set; }
    }
}
