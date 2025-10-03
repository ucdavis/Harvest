using Harvest.Core.Domain;
using System;
using System.Linq.Expressions;

namespace Harvest.Web.Models.MobileModels
{
    public class RecentExpensesModel
    {
        //public int Id { get; set; }

        public string Type { get; set; }
        public string Activity { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public decimal Quantity { get; set; }
        //public bool Markup { get; set; }
        //public decimal Total { get; set; }
        //public Guid? UniqueId { get; set; }
        //public DateTime CreatedOn { get; set; }
        public int ProjectId { get; set; }
        public int RateId { get; set; }
        //public string ProjectName { get; set; }





        public static Expression<Func<Expense, RecentExpensesModel>> Projection()
        {
            return e => new RecentExpensesModel
            {
                //Id = e.Id,
                Type = e.Type,
                Activity = e.Activity,
                Description = e.Description,
                Price = e.Price,
                Quantity = e.Quantity,
                ProjectId = e.ProjectId,
                RateId = e.RateId,

            };
        }
    }
}
