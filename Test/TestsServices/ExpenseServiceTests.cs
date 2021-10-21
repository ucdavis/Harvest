using Harvest.Core.Data;
using Harvest.Core.Domain;
using Harvest.Core.Extensions;
using Harvest.Core.Services;
using Microsoft.EntityFrameworkCore;
using Moq;
using Shouldly;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Test.Helpers;
using TestHelpers.Helpers;
using Xunit;


namespace Test.TestsServices
{
    [Trait("Category", "ServiceTest")]
    public class ExpenseServiceTests
    {
        public Mock<AppDbContext> MockDbContext { get; set; }
        public Mock<IProjectHistoryService> MockProjectHistoryService { get; set; }
        public Mock<IUserService> MockUserService { get; set; }
        public Mock<IDateTimeService> MockDateTimeService { get; set; }
        public ExpenseService ExpenseService { get; set; }

        public List<Project> Projects { get; set; }
        public List<Expense> Expenses { get; set; }

        public List<Expense> AddedExpenses { get; set; }

        public List<Rate> Rates { get; set; }

        public ExpenseServiceTests()
        {
            MockDbContext = new Mock<AppDbContext>(new DbContextOptions<AppDbContext>());
            MockProjectHistoryService = new Mock<IProjectHistoryService>();
            MockDateTimeService = new Mock<IDateTimeService>();

            ExpenseService = new ExpenseService(MockDbContext.Object, MockProjectHistoryService.Object, MockDateTimeService.Object);
        }

        #region Setups
        private void SetupData()
        {
            Projects = new List<Project>();
            for (int i = 0; i < 3; i++)
            {
                Projects.Add(CreateValidEntities.Project(i + 1));
            }
            Projects[1].IsActive = false;

            Expenses = new List<Expense>();
            for (int i = 0; i < 3; i++)
            {
                //Just assign to project not being used at the moment.
                Expenses.Add(CreateValidEntities.Expense(i+1, Projects[1].Id));
            }

            Rates = new List<Rate>();
            for (int i = 0; i < 5; i++)
            {
                //First 5 rates are acreage rates by default
                Rates.Add(CreateValidEntities.Rate(i+1));
            }
            Rates.Add(CreateValidEntities.Rate(6, Rate.Types.Equipment));
            Rates.Add(CreateValidEntities.Rate(7, Rate.Types.Labor));
            Rates.Add(CreateValidEntities.Rate(8, Rate.Types.Other));
            Rates[0].Account = "3-RRACRES-CNTRY-RAS5";
            Rates[0].Price = 3281.00m;


            var specificExpense = CreateValidEntities.Expense(9, Projects[0].Id);
            specificExpense.CreatedOn = new DateTime(2020, 01, 01).FromPacificTime();
            specificExpense.Type = Rate.Types.Acreage;
            specificExpense.Rate = Rates[0];

            Expenses.Add(specificExpense);

            var specificAdjustment = CreateValidEntities.Expense(10, Projects[0].Id);
            specificAdjustment.CreatedOn = new DateTime(2021, 01, 01).FromPacificTime();
            specificAdjustment.Type = Rate.Types.Adjustment; //This one should be ignored
            specificAdjustment.Rate = Rates[1];

            Expenses.Add(specificAdjustment);

            AddedExpenses = new List<Expense>();
        }

        private void MockData()
        {
            MockDbContext.Setup(a => a.Projects).Returns(Projects.AsQueryable().MockAsyncDbSet().Object);
            MockDbContext.Setup(a => a.Expenses).Returns(Expenses.AsQueryable().MockAsyncDbSet().Object);
            MockDbContext.Setup(a => a.Rates).Returns(Rates.AsQueryable().MockAsyncDbSet().Object);
            MockDateTimeService.Setup(a => a.DateTimeUtcNow()).Returns(DateTime.UtcNow);

            MockDbContext.Setup(a => a.Expenses.Add(It.IsAny<Expense>())).Callback<Expense>(a => AddedExpenses.Add(a));
            MockDbContext.Setup(a => a.Expenses.AddAsync(It.IsAny<Expense>(), It.IsAny<System.Threading.CancellationToken>())).Callback((Expense a, CancellationToken token) => AddedExpenses.Add(a));
            MockProjectHistoryService.Setup(a => a.AcreageExpenseCreated(It.IsAny<int>(), It.IsAny<Expense>()));
        }


        #endregion


        [Fact]
        public async Task CreateYearlyAcreageExpenseReturnsEarlyWhenNoAcres()
        {
            SetupData();
            Projects[0].Acres = 0;
            MockData();
            AddedExpenses.Count.ShouldBe(0);

            await ExpenseService.CreateYearlyAcreageExpense(Projects[0]);
            AddedExpenses.Count.ShouldBe(0);
            MockProjectHistoryService.Verify(a => a.AcreageExpenseCreated(It.IsAny<int>(), It.IsAny<Expense>()), times: Times.Never);
            MockDbContext.Verify(a => a.SaveChangesAsync(It.IsAny<CancellationToken>()), times: Times.Never);
            MockDbContext.Verify(a => a.SaveChanges(), times: Times.Never);
        }

        [Fact]
        public async Task CreateYearlyAcreageExpenseReturnsEarlyWhenAmountLessThanCent()
        {
            SetupData();
            Projects[0].Acres = 1;
            Projects[0].AcreageRate.Price = 0.0048m; //Yeah, shouldn't ever happen. 
            MockData();
            AddedExpenses.Count.ShouldBe(0);

            await ExpenseService.CreateYearlyAcreageExpense(Projects[0]);
            AddedExpenses.Count.ShouldBe(0);
            MockProjectHistoryService.Verify(a => a.AcreageExpenseCreated(It.IsAny<int>(), It.IsAny<Expense>()), times: Times.Never);
            MockDbContext.Verify(a => a.SaveChangesAsync(It.IsAny<CancellationToken>()), times: Times.Never);
            MockDbContext.Verify(a => a.SaveChanges(), times: Times.Never);
        }
        [Fact]
        public async Task CreateYearlyAcreageExpenseReturnsEarlyIfUnbilledAcreageExpenseExists() 
        {
            SetupData();
            Expenses[0].ProjectId = Projects[0].Id;
            Expenses[0].InvoiceId = null;
            Expenses[0].Rate.Type = Rate.Types.Acreage;
            MockData();
            AddedExpenses.Count.ShouldBe(0);

            await ExpenseService.CreateYearlyAcreageExpense(Projects[0]);
            AddedExpenses.Count.ShouldBe(0);
            MockProjectHistoryService.Verify(a => a.AcreageExpenseCreated(It.IsAny<int>(), It.IsAny<Expense>()), times: Times.Never);
            MockDbContext.Verify(a => a.SaveChangesAsync(It.IsAny<CancellationToken>()), times: Times.Never);
            MockDbContext.Verify(a => a.SaveChanges(), times: Times.Never);
        }

        [Theory]
        [InlineData(2020, 12, 01)]
        [InlineData(2020, 12, 02)]
        [InlineData(2020, 12, 03)]
        [InlineData(2020, 12, 04)]
        [InlineData(2021, 01, 01)]
        [InlineData(2021, 01, 02)]
        [InlineData(2021, 01, 03)]
        [InlineData(2021, 01, 04)]
        public async Task CreateYearlyAcreageExpenseReturnsEarlyIfAnyAcreageExpenseExistsWithinTheLastYear(int year, int month, int day)
        {
            var date = new DateTime(year, month, day).FromPacificTime();
            SetupData();
            Expenses[0].ProjectId = Projects[0].Id;
            Expenses[0].Rate.Type = Rate.Types.Acreage;
            Expenses[0].CreatedOn = new DateTime(2020,02,03).FromPacificTime(); //Create an expense 2020/02/03 (feb 3 is a Monday, first business day of the month
            Projects[0].Acres = 10;
            MockData();
            MockDateTimeService.Setup(a => a.DateTimeUtcNow()).Returns(date);
            AddedExpenses.Count.ShouldBe(0);

            var expenseServ = new ExpenseService(MockDbContext.Object, MockProjectHistoryService.Object,
                MockDateTimeService.Object);

            await expenseServ.CreateYearlyAcreageExpense(Projects[0]);
            AddedExpenses.Count.ShouldBe(0);
            MockProjectHistoryService.Verify(a => a.AcreageExpenseCreated(It.IsAny<int>(), It.IsAny<Expense>()), times: Times.Never);
            MockDbContext.Verify(a => a.SaveChangesAsync(It.IsAny<CancellationToken>()), times: Times.Never);
            MockDbContext.Verify(a => a.SaveChanges(), times: Times.Never);
        }

        [Theory]
        [InlineData(2021, 02, 01)]
        [InlineData(2021, 02, 02)]
        [InlineData(2021, 02, 03)]
        [InlineData(2021, 02, 04)]
        [InlineData(2021, 03, 01)] //Will run following month if it missed prior month
        [InlineData(2021, 03, 02)]
        [InlineData(2021, 03, 03)]
        [InlineData(2021, 03, 04)]
        public async Task CreateYearlyAcreageExpenseCreatesExpense(int year, int month, int day)
        {
            var date = new DateTime(year, month, day).FromPacificTime();
            SetupData();
            Expenses[0].ProjectId = Projects[0].Id;
            Expenses[0].Rate.Type = Rate.Types.Acreage;
            Expenses[0].CreatedOn = new DateTime(2020, 02, 03).FromPacificTime(); //Create an expense 2020/02/03 (feb 3 is a Monday, first business day of the month
            Projects[0].Acres = 10;
            MockData();
            MockDateTimeService.Setup(a => a.DateTimeUtcNow()).Returns(date);
            AddedExpenses.Count.ShouldBe(0);

            var expenseServ = new ExpenseService(MockDbContext.Object, MockProjectHistoryService.Object,
                MockDateTimeService.Object);

            await expenseServ.CreateYearlyAcreageExpense(Projects[0]);
            AddedExpenses.Count.ShouldBe(1);
            MockProjectHistoryService.Verify(a => a.AcreageExpenseCreated(It.IsAny<int>(), It.IsAny<Expense>()), times: Times.Once);
            MockDbContext.Verify(a => a.SaveChangesAsync(It.IsAny<CancellationToken>()), times: Times.Once);
            var addedExpense = AddedExpenses.First();

            addedExpense.Type.ShouldBe(Projects[0].AcreageRate.Type);
            addedExpense.Description.ShouldBe(Projects[0].AcreageRate.Description);
            addedExpense.Price.ShouldBe(Projects[0].AcreageRate.Price);
            addedExpense.Quantity.ShouldBe((decimal)Projects[0].Acres);
            addedExpense.Total.ShouldBe(11500.00m);
            addedExpense.ProjectId.ShouldBe(Projects[0].Id);
            addedExpense.RateId.ShouldBe(Projects[0].AcreageRate.Id);
            addedExpense.InvoiceId.ShouldBeNull();
            addedExpense.CreatedOn.Date.ShouldBe(date.Date); 
            addedExpense.CreatedBy.ShouldBeNull();
            addedExpense.Account.ShouldBe(Projects[0].AcreageRate.Account);
        }

        [Theory]
        [InlineData(2021, 01, 02)]
        [InlineData(2021, 01, 03)]
        [InlineData(2021, 01, 04)]
        [InlineData(2021, 01, 05)]
        [InlineData(2021, 02, 01)]
        [InlineData(2021, 02, 02)]
        public async Task CreateChangeRequestAdjustmentReturnsEarlyIfNoAcerageExpenseInAYear(int year, int month, int day)
        {
            SetupData();
            MockData();
            MockDateTimeService.Setup(a => a.DateTimeUtcNow()).Returns(new DateTime(year,month, day).FromPacificTime());
            AddedExpenses.Count.ShouldBe(0);

            var newQuote = CreateValidEntities.QuoteDetail();
            newQuote.AcreageRateId = 2;
            newQuote.Acres = 4;
            var originalQuote = CreateValidEntities.QuoteDetail();
            originalQuote.AcreageRateId = 1;
            originalQuote.Acres = 3;

            var expenseServ = new ExpenseService(MockDbContext.Object, MockProjectHistoryService.Object,
                MockDateTimeService.Object);

            await expenseServ.CreateChangeRequestAdjustmentMaybe(Projects[0], newQuote, originalQuote );
            AddedExpenses.Count.ShouldBe(0);
            MockProjectHistoryService.Verify(a => a.AcreageExpenseCreated(It.IsAny<int>(), It.IsAny<Expense>()), times: Times.Never);
            MockDbContext.Verify(a => a.SaveChangesAsync(It.IsAny<CancellationToken>()), times: Times.Never);
            MockDbContext.Verify(a => a.SaveChanges(), times: Times.Never);
        }

        //Rate changed, new rate has no acres
        [Theory]
        [InlineData(2020, 02, 01)]
        [InlineData(2020, 02, 02)]
        [InlineData(2020, 12, 01)]
        [InlineData(2020, 12, 31)]
        [InlineData(2021, 01, 01)]
        [InlineData(2021, 01, 02, Skip = "This one fails because it returns early (No expense within the year)")]
        public async Task CreateChangeRequestAdjustmentCreatesAdjustmentIfNewAcresIsZero(int year, int month, int day)
        {
            //Expense is created on new DateTime(2020, 01, 01).FromPacificTime()new DateTime(2020, 01, 01).FromPacificTime()
            SetupData();
            MockData();
            MockDateTimeService.Setup(a => a.DateTimeUtcNow()).Returns(new DateTime(year, month, day).FromPacificTime());
            AddedExpenses.Count.ShouldBe(0);

            var newQuote = CreateValidEntities.QuoteDetail();
            newQuote.AcreageRateId = null;
            newQuote.Acres = 0;
            var originalQuote = CreateValidEntities.QuoteDetail();
            originalQuote.AcreageRateId = 1;
            originalQuote.Acres = 3;

            var expenseServ = new ExpenseService(MockDbContext.Object, MockProjectHistoryService.Object,
                MockDateTimeService.Object);

            var oldRate = Rates.Single(a => a.Id == 1);

            await expenseServ.CreateChangeRequestAdjustmentMaybe(Projects[0], newQuote, originalQuote);
            AddedExpenses.Count.ShouldBe(1);
            MockProjectHistoryService.Verify(a => a.AcreageExpenseCreated(It.IsAny<int>(), It.IsAny<Expense>()), times: Times.Once);
            MockDbContext.Verify(a => a.SaveChangesAsync(It.IsAny<CancellationToken>()), times: Times.Once);
            //MockDbContext.Verify(a => a.SaveChanges(), times: Times.Never);
            var addedExpense = AddedExpenses.First();
            addedExpense.CreatedBy.ShouldBeNull();
            addedExpense.CreatedOn.ShouldBe(new DateTime(year, month, day).FromPacificTime());
            addedExpense.Type.ShouldBe(Rate.Types.Adjustment);
            addedExpense.RateId.ShouldBe(1); //The old rateId
            addedExpense.Account.ShouldBe(oldRate.Account);
            addedExpense.Description.ShouldBe($"Acreage Adjustment (Refund) -- {oldRate.Description}");
            addedExpense.Price.ShouldBe((oldRate.Price * -1));
            addedExpense.Quantity.ShouldBe(3);
            addedExpense.Total.ShouldBe(-9843.00m);
            addedExpense.ProjectId.ShouldBe(Projects[0].Id);
            addedExpense.InvoiceId.ShouldBeNull();

        }

        //Rate changed, new rate has acres
        [Theory]
        [InlineData(2020, 02, 01)]
        [InlineData(2020, 02, 02)]
        [InlineData(2020, 12, 01)]
        [InlineData(2020, 12, 31)]
        [InlineData(2021, 01, 01)]
        public async Task CreateChangeRequestAdjustmentCreatesAdjustmentAndReplacementFeeIfAcresNotZero(int year, int month, int day)
        {
            //Expense is created on new DateTime(2020, 01, 01).FromPacificTime()new DateTime(2020, 01, 01).FromPacificTime()
            SetupData();
            MockData();
            MockDateTimeService.Setup(a => a.DateTimeUtcNow()).Returns(new DateTime(year, month, day).FromPacificTime());
            AddedExpenses.Count.ShouldBe(0);

            var newQuote = CreateValidEntities.QuoteDetail();
            newQuote.AcreageRateId = 2;
            newQuote.Acres = 3;
            var originalQuote = CreateValidEntities.QuoteDetail();
            originalQuote.AcreageRateId = 1;
            originalQuote.Acres = 3;

            var expenseServ = new ExpenseService(MockDbContext.Object, MockProjectHistoryService.Object,
                MockDateTimeService.Object);

            var oldRate = Rates.Single(a => a.Id == 1);
            var newRate = Rates.Single(a => a.Id == 2);

            await expenseServ.CreateChangeRequestAdjustmentMaybe(Projects[0], newQuote, originalQuote);
            AddedExpenses.Count.ShouldBe(2);
            MockProjectHistoryService.Verify(a => a.AcreageExpenseCreated(It.IsAny<int>(), It.IsAny<Expense>()), times: Times.Exactly(2));
            MockDbContext.Verify(a => a.SaveChangesAsync(It.IsAny<CancellationToken>()), times: Times.Once);
            MockProjectHistoryService.Verify(a => a.AcreageExpenseCreated(It.IsAny<int>(), AddedExpenses[0]), times: Times.Once);
            MockProjectHistoryService.Verify(a => a.AcreageExpenseCreated(It.IsAny<int>(), AddedExpenses[1]), times: Times.Once);
            //MockDbContext.Verify(a => a.SaveChanges(), times: Times.Never);
            var addedExpense = AddedExpenses.First();
            addedExpense.CreatedBy.ShouldBeNull();
            addedExpense.CreatedOn.ShouldBe(new DateTime(year, month, day).FromPacificTime());
            addedExpense.Type.ShouldBe(Rate.Types.Adjustment);
            addedExpense.RateId.ShouldBe(1); //The old rateId
            addedExpense.Account.ShouldBe(oldRate.Account);
            addedExpense.Description.ShouldBe($"Acreage Adjustment (Refund) -- {oldRate.Description}");
            addedExpense.Price.ShouldBe((oldRate.Price * -1));
            addedExpense.Quantity.ShouldBe(3);
            addedExpense.Total.ShouldBe(-9843.00m);
            addedExpense.ProjectId.ShouldBe(Projects[0].Id);
            addedExpense.InvoiceId.ShouldBeNull();

            //Verify second added expense
            addedExpense = AddedExpenses[1];
            addedExpense.CreatedBy.ShouldBeNull();
            addedExpense.CreatedOn.ShouldBe(new DateTime(year, month, day).FromPacificTime());
            addedExpense.Type.ShouldBe(Rate.Types.Adjustment);
            addedExpense.RateId.ShouldBe(2); //The old rateId
            addedExpense.Account.ShouldBe(newRate.Account);
            addedExpense.Description.ShouldBe($"Acreage Adjustment -- {newRate.Description}");
            addedExpense.Price.ShouldBe((newRate.Price));
            addedExpense.Quantity.ShouldBe(3);
            addedExpense.Total.ShouldBe(3450.00m);
            addedExpense.ProjectId.ShouldBe(Projects[0].Id);
            addedExpense.InvoiceId.ShouldBeNull();

        }

        //Test when acres reduced
        [Theory]
        [InlineData(2020, 02, 01)]
        [InlineData(2020, 02, 02)]
        [InlineData(2020, 12, 01)]
        [InlineData(2020, 12, 31)]
        [InlineData(2021, 01, 01)]
        public async Task CreateChangeRequestAdjustmentCreatesAdjustmentWhenAcresReduced(int year, int month, int day)
        {
            //Expense is created on new DateTime(2020, 01, 01).FromPacificTime()new DateTime(2020, 01, 01).FromPacificTime()
            SetupData();
            MockData();
            MockDateTimeService.Setup(a => a.DateTimeUtcNow()).Returns(new DateTime(year, month, day).FromPacificTime());
            AddedExpenses.Count.ShouldBe(0);

            var newQuote = CreateValidEntities.QuoteDetail();
            newQuote.AcreageRateId = 1;
            newQuote.Acres = 2.23456; //Reduce just a little
            var originalQuote = CreateValidEntities.QuoteDetail();
            originalQuote.AcreageRateId = 1;
            originalQuote.Acres = 3;

            var expenseServ = new ExpenseService(MockDbContext.Object, MockProjectHistoryService.Object,
                MockDateTimeService.Object);

            var rate = Rates.Single(a => a.Id == 1);


            await expenseServ.CreateChangeRequestAdjustmentMaybe(Projects[0], newQuote, originalQuote);
            AddedExpenses.Count.ShouldBe(1);
            MockProjectHistoryService.Verify(a => a.AcreageExpenseCreated(It.IsAny<int>(), It.IsAny<Expense>()), times: Times.Exactly(1));
            MockDbContext.Verify(a => a.SaveChangesAsync(It.IsAny<CancellationToken>()), times: Times.Once);
            MockProjectHistoryService.Verify(a => a.AcreageExpenseCreated(It.IsAny<int>(), AddedExpenses[0]), times: Times.Once);
            //MockDbContext.Verify(a => a.SaveChanges(), times: Times.Never);
            var addedExpense = AddedExpenses.First();
            addedExpense.CreatedBy.ShouldBeNull();
            addedExpense.CreatedOn.ShouldBe(new DateTime(year, month, day).FromPacificTime());
            addedExpense.Type.ShouldBe(Rate.Types.Adjustment);
            addedExpense.RateId.ShouldBe(1); //The old rateId
            addedExpense.Account.ShouldBe(rate.Account);
            addedExpense.Description.ShouldBe($"Acreage Adjustment (Partial Refund) -- {rate.Description}");
            addedExpense.Price.ShouldBe((rate.Price * -1));
            addedExpense.Quantity.ShouldBe(-0.76544m);
            addedExpense.Total.ShouldBe(-2511.40m); //-3281.00 * 0.76544 = -2511.40864 -- Rounds by chopping
            addedExpense.ProjectId.ShouldBe(Projects[0].Id);
            addedExpense.InvoiceId.ShouldBeNull();
        }

        //Test when acres increased 
        [Theory]
        [InlineData(2020, 02, 01)]
        [InlineData(2020, 02, 02)]
        [InlineData(2020, 12, 01)]
        [InlineData(2020, 12, 31)]
        [InlineData(2021, 01, 01)]
        public async Task CreateChangeRequestAdjustmentCreatesAdjustmentWhenAcresIncreased(int year, int month, int day)
        {
            //Expense is created on new DateTime(2020, 01, 01).FromPacificTime()new DateTime(2020, 01, 01).FromPacificTime()
            SetupData();
            MockData();
            MockDateTimeService.Setup(a => a.DateTimeUtcNow()).Returns(new DateTime(year, month, day).FromPacificTime());
            AddedExpenses.Count.ShouldBe(0);

            var newQuote = CreateValidEntities.QuoteDetail();
            newQuote.AcreageRateId = 1;
            newQuote.Acres = 4.23456; //increase just a little
            var originalQuote = CreateValidEntities.QuoteDetail();
            originalQuote.AcreageRateId = 1;
            originalQuote.Acres = 3;

            var expenseServ = new ExpenseService(MockDbContext.Object, MockProjectHistoryService.Object,
                MockDateTimeService.Object);

            var rate = Rates.Single(a => a.Id == 1);


            await expenseServ.CreateChangeRequestAdjustmentMaybe(Projects[0], newQuote, originalQuote);
            AddedExpenses.Count.ShouldBe(1);
            MockProjectHistoryService.Verify(a => a.AcreageExpenseCreated(It.IsAny<int>(), It.IsAny<Expense>()), times: Times.Exactly(1));
            MockDbContext.Verify(a => a.SaveChangesAsync(It.IsAny<CancellationToken>()), times: Times.Once);
            MockProjectHistoryService.Verify(a => a.AcreageExpenseCreated(It.IsAny<int>(), AddedExpenses[0]), times: Times.Once);
            //MockDbContext.Verify(a => a.SaveChanges(), times: Times.Never);
            var addedExpense = AddedExpenses.First();
            addedExpense.CreatedBy.ShouldBeNull();
            addedExpense.CreatedOn.ShouldBe(new DateTime(year, month, day).FromPacificTime());
            addedExpense.Type.ShouldBe(Rate.Types.Adjustment);
            addedExpense.RateId.ShouldBe(1); //The old rateId
            addedExpense.Account.ShouldBe(rate.Account);
            addedExpense.Description.ShouldBe($"Acreage Adjustment -- {rate.Description}");
            addedExpense.Price.ShouldBe((rate.Price));
            addedExpense.Quantity.ShouldBe(1.23456m);
            addedExpense.Total.ShouldBe(4050.59m); //3281.00 * 1.23456 = 4050.59136 -- Rounds by chopping
            addedExpense.ProjectId.ShouldBe(Projects[0].Id);
            addedExpense.InvoiceId.ShouldBeNull();
        }

        //Test old rate was null/no acres and acres added
        [Theory]
        [InlineData(2020, 02, 01)]
        [InlineData(2020, 02, 02)]
        [InlineData(2020, 12, 01)]
        [InlineData(2020, 12, 31)]
        [InlineData(2021, 01, 01)]
        public async Task CreateChangeRequestAdjustmentCreatesAdjustmentWhenAcresIncreasedFromZero(int year, int month, int day)
        {
            //Expense is created on new DateTime(2020, 01, 01).FromPacificTime()new DateTime(2020, 01, 01).FromPacificTime()
            SetupData();
            MockData();
            MockDateTimeService.Setup(a => a.DateTimeUtcNow()).Returns(new DateTime(year, month, day).FromPacificTime());
            AddedExpenses.Count.ShouldBe(0);

            var newQuote = CreateValidEntities.QuoteDetail();
            newQuote.AcreageRateId = 1;
            newQuote.Acres = 4.23456; //increase just a little
            var originalQuote = CreateValidEntities.QuoteDetail();
            originalQuote.AcreageRateId = null;
            originalQuote.Acres = 0;

            var expenseServ = new ExpenseService(MockDbContext.Object, MockProjectHistoryService.Object,
                MockDateTimeService.Object);

            var rate = Rates.Single(a => a.Id == 1);


            await expenseServ.CreateChangeRequestAdjustmentMaybe(Projects[0], newQuote, originalQuote);
            AddedExpenses.Count.ShouldBe(1);
            MockProjectHistoryService.Verify(a => a.AcreageExpenseCreated(It.IsAny<int>(), It.IsAny<Expense>()), times: Times.Exactly(1));
            MockDbContext.Verify(a => a.SaveChangesAsync(It.IsAny<CancellationToken>()), times: Times.Once);
            MockProjectHistoryService.Verify(a => a.AcreageExpenseCreated(It.IsAny<int>(), AddedExpenses[0]), times: Times.Once);
            //MockDbContext.Verify(a => a.SaveChanges(), times: Times.Never);
            var addedExpense = AddedExpenses.First();
            addedExpense.CreatedBy.ShouldBeNull();
            addedExpense.CreatedOn.ShouldBe(new DateTime(year, month, day).FromPacificTime());
            addedExpense.Type.ShouldBe(Rate.Types.Adjustment);
            addedExpense.RateId.ShouldBe(1); //The old rateId
            addedExpense.Account.ShouldBe(rate.Account);
            addedExpense.Description.ShouldBe($"Acreage Adjustment -- {rate.Description}");
            addedExpense.Price.ShouldBe((rate.Price));
            addedExpense.Quantity.ShouldBe(4.23456m);
            addedExpense.Total.ShouldBe(13893.59m); //3281.00 * 4.23456 = 13893.59136 
            addedExpense.ProjectId.ShouldBe(Projects[0].Id);
            addedExpense.InvoiceId.ShouldBeNull();
        }



        //Test nothing happened: rate is null
        [Theory]
        [InlineData(2020, 02, 01)]
        [InlineData(2020, 02, 02)]
        [InlineData(2020, 12, 01)]
        [InlineData(2020, 12, 31)]
        [InlineData(2021, 01, 01)]
        public async Task CreateChangeRequestAdjustmentNothingHappensBothRatesNull(int year, int month, int day)
        {
            //Expense is created on new DateTime(2020, 01, 01).FromPacificTime()new DateTime(2020, 01, 01).FromPacificTime()
            SetupData();
            MockData();
            MockDateTimeService.Setup(a => a.DateTimeUtcNow()).Returns(new DateTime(year, month, day).FromPacificTime());
            AddedExpenses.Count.ShouldBe(0);

            var newQuote = CreateValidEntities.QuoteDetail();
            newQuote.AcreageRateId = null;
            newQuote.Acres = 0;
            var originalQuote = CreateValidEntities.QuoteDetail();
            originalQuote.AcreageRateId = null;
            originalQuote.Acres = 0;

            var expenseServ = new ExpenseService(MockDbContext.Object, MockProjectHistoryService.Object,
                MockDateTimeService.Object);

            await expenseServ.CreateChangeRequestAdjustmentMaybe(Projects[0], newQuote, originalQuote);
            AddedExpenses.Count.ShouldBe(0);
            MockProjectHistoryService.Verify(a => a.AcreageExpenseCreated(It.IsAny<int>(), It.IsAny<Expense>()), times: Times.Never);
            MockDbContext.Verify(a => a.SaveChangesAsync(It.IsAny<CancellationToken>()), times: Times.Never);
            MockDbContext.Verify(a => a.SaveChanges(), times: Times.Never);
        }

        //Test nothing happened: rate is same, acres don't change
        [Theory]
        [InlineData(2020, 02, 01)]
        [InlineData(2020, 02, 02)]
        [InlineData(2020, 12, 01)]
        [InlineData(2020, 12, 31)]
        [InlineData(2021, 01, 01)]
        public async Task CreateChangeRequestAdjustmentNothingHappensBothRatesSame(int year, int month, int day)
        {
            //Expense is created on new DateTime(2020, 01, 01).FromPacificTime()new DateTime(2020, 01, 01).FromPacificTime()
            SetupData();
            MockData();
            MockDateTimeService.Setup(a => a.DateTimeUtcNow()).Returns(new DateTime(year, month, day).FromPacificTime());
            AddedExpenses.Count.ShouldBe(0);

            var newQuote = CreateValidEntities.QuoteDetail();
            newQuote.AcreageRateId = 1;
            newQuote.Acres = 1;
            var originalQuote = CreateValidEntities.QuoteDetail();
            originalQuote.AcreageRateId = 1;
            originalQuote.Acres = 1;

            var expenseServ = new ExpenseService(MockDbContext.Object, MockProjectHistoryService.Object,
                MockDateTimeService.Object);

            await expenseServ.CreateChangeRequestAdjustmentMaybe(Projects[0], newQuote, originalQuote);
            AddedExpenses.Count.ShouldBe(0);
            MockProjectHistoryService.Verify(a => a.AcreageExpenseCreated(It.IsAny<int>(), It.IsAny<Expense>()), times: Times.Never);
            MockDbContext.Verify(a => a.SaveChangesAsync(It.IsAny<CancellationToken>()), times: Times.Never);
            MockDbContext.Verify(a => a.SaveChanges(), times: Times.Never);
        }

        #region theory
        [Theory]
        [InlineData(2.0, 1200.00, 2400.00)]
        [InlineData(1.0, 1200.00, 1200.00)]
        [InlineData(1.0, 12.00, 12.00)]
        [InlineData(1.0, 1.00, 1.00)]
        [InlineData(1.014, 1.00, 1.01)]
        [InlineData(1.015, 1.00, 1.01)]
        [InlineData(1.016, 1.00, 1.01)]
        [InlineData(1.02, 1.00, 1.02)]
        [InlineData(1.025, 1.00, 1.02)] 
        [InlineData(1.03, 1.00, 1.03)]
        [InlineData(1.08, 1.00, 1.08)]
        [InlineData(1.09, 1.00, 1.09)]
        #endregion
        public async Task CreateMonthlyAcreageExpenseReturnsExpectedValues(double acres, decimal price, decimal expectedAmount)
        {
            //This is using normal rounding. If we chop instead then these tests will need updating
            SetupData();
            Projects[0].Acres = acres;
            Projects[0].AcreageRate.Price = price;
            MockData();
            AddedExpenses.Count.ShouldBe(0);

            await ExpenseService.CreateYearlyAcreageExpense(Projects[0]);
            AddedExpenses.Count.ShouldBe(1);
            MockProjectHistoryService.Verify(a => a.AcreageExpenseCreated(It.IsAny<int>(), It.IsAny<Expense>()), times: Times.Once);
            MockDbContext.Verify(a => a.SaveChangesAsync(It.IsAny<CancellationToken>()), times: Times.Once);
            var addedExpense = AddedExpenses.First();
            addedExpense.Type.ShouldBe(Projects[0].AcreageRate.Type);
            addedExpense.Description.ShouldBe(Projects[0].AcreageRate.Description);
            addedExpense.Price.ShouldBe(Projects[0].AcreageRate.Price);
            addedExpense.Quantity.ShouldBe((decimal)Projects[0].Acres);
            addedExpense.Total.ShouldBe(expectedAmount);
            addedExpense.ProjectId.ShouldBe(Projects[0].Id);
            addedExpense.RateId.ShouldBe(Projects[0].AcreageRate.Id);
            addedExpense.InvoiceId.ShouldBeNull();
            addedExpense.CreatedOn.Date.ShouldBe(DateTime.UtcNow.Date); //This could fail on some unique runs
            addedExpense.CreatedBy.ShouldBeNull();
            addedExpense.Account.ShouldBe(Projects[0].AcreageRate.Account);
        }
    }
}
