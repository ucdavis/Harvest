using Harvest.Core.Models;
using Harvest.Core.Utilities;
using Shouldly;
using Xunit;

namespace Test.TestsServices
{
    [Trait("Category", "ServiceTest")]
    public class ExpenseCalculationsTests
    {
        [Fact]
        public void CalculateExpenseTotalAddsMarkupBelowCap()
        {
            var total = ExpenseCalculations.CalculateExpenseTotal(450m, 1m, true);

            total.ShouldBe(540m);
        }

        [Fact]
        public void CalculateExpenseTotalCapsMarkupPerExpense()
        {
            var total = ExpenseCalculations.CalculateExpenseTotal(900m, 10m, true);

            total.ShouldBe(9200m);
        }

        [Fact]
        public void NormalizeQuoteDetailRecalculatesWorkItemsAndRollups()
        {
            var quote = new QuoteDetail
            {
                ProjectName = "Test Project",
                Acres = 0,
                AcreageRate = 0,
                Years = 1,
                Activities = new[]
                {
                    new Activity
                    {
                        Id = 1,
                        Name = "Activity 1",
                        Adjustment = 10m,
                        WorkItems = new[]
                        {
                            new WorkItem
                            {
                                Id = 1,
                                ActivityId = 1,
                                Type = "Other",
                                Rate = 100,
                                Quantity = 15,
                                Markup = true,
                            },
                            new WorkItem
                            {
                                Id = 2,
                                ActivityId = 1,
                                Type = "Labor",
                                Rate = 50,
                                Quantity = 2,
                                Markup = false,
                            },
                        },
                    },
                },
            };

            ExpenseCalculations.NormalizeQuoteDetail(quote);

            quote.Activities[0].WorkItems[0].Total.ShouldBe(1850d);
            quote.Activities[0].WorkItems[1].Total.ShouldBe(110d);
            quote.Activities[0].Total.ShouldBe(1960d);
            quote.OtherTotal.ShouldBe(1850d);
            quote.LaborTotal.ShouldBe(110d);
            quote.ActivitiesTotal.ShouldBe(1960d);
            quote.GrandTotal.ShouldBe(1960d);
        }
    }
}