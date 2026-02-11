using Harvest.Core.Domain;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Linq.Expressions;

namespace Harvest.Web.Models.ReportModels
{
    public class HistoricalRateActivityModelList
    {
        [DataType(DataType.Date)]
        public DateTime? Start { get; set; }
        [DataType(DataType.Date)]
        public DateTime? End { get; set; }
        public List<HistoricalRateActivityModel> HistoricalRates { get; set; }

        public string TeamName { get; set; }
    }
    public class HistoricalRateActivityModel
    {
        [Display(Name = "Id")]
        public int RateId { get; set; }
        [Display(Name = "Active")]
        public string IsActive { get; set; }
        public string Type { get; set; }
        public string Description { get; set; }
        public string Account { get; set; }
        public decimal Price { get; set; }
        [Display(Name = "Total Quantity")]
        public decimal TotalQuantity { get; set; }
        [Display(Name = "Total Amount")]
        public decimal TotalAmount { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="start">UTC</param>
        /// <param name="end">UTC</param>
        /// <returns></returns>
        public static Expression<Func<Rate, HistoricalRateActivityModel>> Projection(DateTime start, DateTime end)
        {
            return rate => new HistoricalRateActivityModel
            {
                RateId = rate.Id,
                IsActive = rate.IsActive ? "Yes" : "No",
                Type = rate.Type,
                Description = rate.Description,
                Account = rate.Account,
                Price = rate.Price,
                TotalQuantity = rate.Expenses.Where(a => a.CreatedOn >= start && a.CreatedOn <= end && a.Invoice != null && a.Invoice.Status != Invoice.Statuses.Cancelled).Sum(a => a.Quantity),
                TotalAmount = rate.Expenses.Where(a => a.CreatedOn >= start && a.CreatedOn <= end && a.Invoice != null && a.Invoice.Status != Invoice.Statuses.Cancelled).Sum(a => a.Total)
            };
        }
    }
}
