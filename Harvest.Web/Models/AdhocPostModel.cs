using Harvest.Core.Domain;
using Harvest.Core.Models;

namespace Harvest.Web.Models
{
    public class AdhocPostModel
    {
        public Project Project { get; set; }
        public Expense[] Expenses { get; set; }
        public Account[] Accounts { get; set; }
        public QuoteDetail Quote { get;set;} //Maybe use this to create a quote?
    }
}
