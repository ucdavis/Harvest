using Harvest.Core.Domain;
using System;
using System.Linq.Expressions;

namespace Harvest.Core.Models
{
    public class RatesModel
    {

        public int Id { get; set; }
        public decimal Price { get; set; }
        public string Unit { get; set; }
        public string Type { get; set; }
        public string Description { get; set; }
        public bool IsPassthrough { get; set; }

        public static Expression<Func<Rate, RatesModel>> Projection()
        {
            return r => new RatesModel
            {
                Id = r.Id,
                Price = r.Price,
                Unit = r.Unit,
                Type = r.Type,
                Description = r.Description,
                IsPassthrough = r.IsPassthrough
            };
        }
    }
}
