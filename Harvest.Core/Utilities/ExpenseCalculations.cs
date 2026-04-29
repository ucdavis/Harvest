using System;
using Harvest.Core.Domain;
using Harvest.Core.Models;

namespace Harvest.Core.Utilities
{
    public static class ExpenseCalculations
    {
        public const decimal MarkupRate = 0.20m;
        public const decimal MarkupCap = 1000m;

        public static decimal CalculateMarkupAmount(decimal baseTotal, bool applyMarkup)
        {
            if (!applyMarkup || baseTotal <= 0)
            {
                return 0;
            }

            return RoundCurrency(Math.Min(baseTotal, MarkupCap) * MarkupRate);
        }

        public static decimal CalculateExpenseTotal(decimal rate, decimal quantity, bool applyMarkup)
        {
            var baseTotal = rate * quantity;
            return RoundCurrency(baseTotal + CalculateMarkupAmount(baseTotal, applyMarkup));
        }

        public static double CalculateWorkItemTotal(WorkItem workItem, decimal adjustment)
        {
            if (workItem == null)
            {
                throw new ArgumentNullException(nameof(workItem));
            }

            var adjustedRate = (decimal)workItem.Rate + (((decimal)workItem.Rate * adjustment) / 100m);
            var baseTotal = adjustedRate * (decimal)workItem.Quantity;
            return (double)RoundCurrency(baseTotal + CalculateMarkupAmount(baseTotal, workItem.Markup));
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

                    workItem.Total = CalculateWorkItemTotal(workItem, activity.Adjustment);
                    activityTotal += (decimal)workItem.Total;

                    switch (workItem.Type)
                    {
                        case Rate.Types.Labor:
                            laborTotal += (decimal)workItem.Total;
                            break;
                        case Rate.Types.Equipment:
                            equipmentTotal += (decimal)workItem.Total;
                            break;
                        case Rate.Types.Other:
                            otherTotal += (decimal)workItem.Total;
                            break;
                    }
                }

                activity.Total = (double)RoundCurrency(activityTotal);
                activitiesTotal += (decimal)activity.Total;
            }

            quoteDetail.AcreageTotal = (double)RoundCurrency(
                (decimal)quoteDetail.AcreageRate *
                (decimal)quoteDetail.Acres *
                quoteDetail.Years);
            quoteDetail.ActivitiesTotal = (double)RoundCurrency(activitiesTotal);
            quoteDetail.LaborTotal = (double)RoundCurrency(laborTotal);
            quoteDetail.EquipmentTotal = (double)RoundCurrency(equipmentTotal);
            quoteDetail.OtherTotal = (double)RoundCurrency(otherTotal);
            quoteDetail.GrandTotal = (double)RoundCurrency(
                (decimal)quoteDetail.AcreageTotal +
                (decimal)quoteDetail.ActivitiesTotal);
        }

        private static decimal RoundCurrency(decimal value)
        {
            return Math.Round(value, 2, MidpointRounding.AwayFromZero);
        }
    }
}