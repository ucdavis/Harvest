using Harvest.Core.Domain;

namespace Harvest.Web.Models
{
    public class AdhocPostModel
    {
        public Project Project { get;set;}
        public Expense[] Expenses { get;set;}
    }
}
