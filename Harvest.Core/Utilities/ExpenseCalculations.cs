using System;
using Harvest.Core.Domain;
using Harvest.Core.Models;

namespace Harvest.Core.Utilities
{
    public static class ExpenseCalculations
    {
        public const decimal MarkupRate = 0.20m;
        public const decimal MarkupCap = 1000m;

        public static decimal CalculateExpenseTotal(decimal rate, decimal quantity, bool applyMarkup)
        {
            var baseTotal = rate * quantity;
            return RoundCurrency(baseTotal + CalculateMarkupAmount(baseTotal, applyMarkup));
        }

        public static void NormalizeQuoteDetail(QuoteDetail quoteDetail)
        {
            if (quoteDetail == null)
            {
                throw new ArgumentNullException(nameof(quoteDetail));
            }

            decimal activitiesTotal = 0;
            decimal laborTotal = 0;
            decimal equipmentTotal = 0;
            decimal otherTotal = 0;

            foreach (var activity in quoteDetail.Activities ?? Array.Empty<Activity>())
            {
                if (activity == null)
                {
                    continue;
                }

                decimal activityTotal = 0;

                foreach (var workItem in activity.WorkItems ?? Array.Empty<WorkItem>())
                {
                    if (workItem == null)
                    {
                        continue;
                    }

                    var workItemTotal = CalculateWorkItemTotal(workItem, activity.Adjustment);
                    workItem.Total = (double)workItemTotal;
                    activityTotal += workItemTotal;

                    switch (workItem.Type)
                    {
                        case Rate.Types.Labor:
                            laborTotal += workItemTotal;
                            break;
                        case Rate.Types.Equipment:
                            equipmentTotal += workItemTotal;
                            break;
                        case Rate.Types.Other:
                            otherTotal += workItemTotal;
                            break;
                    }
                }

                var roundedActivityTotal = RoundCurrency(activityTotal);
                activity.Total = (double)roundedActivityTotal;
                activitiesTotal += roundedActivityTotal;
            }

            var acreageTotal = RoundCurrency(
                (decimal)quoteDetail.AcreageRate *
                (decimal)quoteDetail.Acres *
                quoteDetail.Years);
            var roundedActivitiesTotal = RoundCurrency(activitiesTotal);
            var roundedLaborTotal = RoundCurrency(laborTotal);
            var roundedEquipmentTotal = RoundCurrency(equipmentTotal);
            var roundedOtherTotal = RoundCurrency(otherTotal);
            var grandTotal = RoundCurrency(acreageTotal + roundedActivitiesTotal);

            quoteDetail.AcreageTotal = (double)acreageTotal;
            quoteDetail.ActivitiesTotal = (double)roundedActivitiesTotal;
            quoteDetail.LaborTotal = (double)roundedLaborTotal;
            quoteDetail.EquipmentTotal = (double)roundedEquipmentTotal;
            quoteDetail.OtherTotal = (double)roundedOtherTotal;
            quoteDetail.GrandTotal = (double)grandTotal;
        }

        private static decimal RoundCurrency(decimal value)
        {
            return Math.Round(value, 2, MidpointRounding.AwayFromZero);
        }

        private static decimal CalculateMarkupAmount(decimal baseTotal, bool applyMarkup)
        {
            if (!applyMarkup || baseTotal <= 0)
            {
                return 0;
            }

            return RoundCurrency(Math.Min(baseTotal, MarkupCap) * MarkupRate);
        }

        private static decimal CalculateWorkItemTotal(WorkItem workItem, decimal adjustment)
        {
            if (workItem == null)
            {
                throw new ArgumentNullException(nameof(workItem));
            }

            var adjustedRate = (decimal)workItem.Rate + (((decimal)workItem.Rate * adjustment) / 100m);
            var baseTotal = adjustedRate * (decimal)workItem.Quantity;
            return RoundCurrency(baseTotal + CalculateMarkupAmount(baseTotal, workItem.Markup));
        }
    }
}