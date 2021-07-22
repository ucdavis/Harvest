using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Harvest.Core.Domain;

namespace Harvest.Core.Models.History
{
    public class RateHistoryModel
    {
        public static RateHistoryModel CreateFrom(Rate rate)
        {
            if (rate == null)
            {
                return null;
            }

            return new RateHistoryModel
            {
                Id = rate.Id,
                IsActive = rate.IsActive,
                Type = rate.Type,
                Description = rate.Description,
                Unit = rate.Unit,
                BillingUnit = rate.BillingUnit,
                Account = rate.Account,
                Price = rate.Price,
                EffectiveOn = rate.EffectiveOn,
                CreatedById = rate.CreatedById,
                UpdatedById = rate.UpdatedById,
                CreatedOn = rate.CreatedOn,
                UpdatedOn = rate.UpdatedOn
            };
        }

        public int Id { get; set; }
        public bool IsActive { get; set; } = true;
        public string Type { get; set; }
        public string Description { get; set; }
        public string Unit { get; set; }
        public string BillingUnit { get; set; }
        public string Account { get; set; }
        public decimal Price { get; set; }
        public DateTime? EffectiveOn { get; set; }
        public int CreatedById { get; set; }
        public int UpdatedById { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime UpdatedOn { get; set; }

    }
}
