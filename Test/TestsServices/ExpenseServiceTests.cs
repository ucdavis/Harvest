using Harvest.Core.Data;
using Harvest.Core.Domain;
using Harvest.Core.Models.Settings;
using Harvest.Core.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Moq;
using Shouldly;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Harvest.Core.Extensions;
using Harvest.Core.Models;
using Serilog;
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
        public Expense AddedExpense { get; set; } = null;

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
        }

        private void MockData()
        {
            MockDbContext.Setup(a => a.Projects).Returns(Projects.AsQueryable().MockAsyncDbSet().Object);
            MockDbContext.Setup(a => a.Expenses).Returns(Expenses.AsQueryable().MockAsyncDbSet().Object);
            MockDbContext.Setup(a => a.Rates).Returns(Rates.AsQueryable().MockAsyncDbSet().Object);
            MockDateTimeService.Setup(a => a.DateTimeUtcNow()).Returns(DateTime.UtcNow);

            MockDbContext.Setup(a => a.Expenses.Add(It.IsAny<Expense>())).Callback<Expense>(a => AddedExpense = a);
            MockDbContext.Setup(a => a.Expenses.AddAsync(It.IsAny<Expense>(), It.IsAny<System.Threading.CancellationToken>())).Callback((Expense a, CancellationToken token) => AddedExpense = a);
            MockProjectHistoryService.Setup(a => a.AcreageExpenseCreated(It.IsAny<int>(), It.IsAny<Expense>()));
        }

        #endregion


        [Fact]
        public async Task CreateYearlyAcreageExpenseReturnsEarlyWhenNoAcres()
        {
            SetupData();
            Projects[0].Acres = 0;
            MockData();
            AddedExpense.ShouldBeNull();

            await ExpenseService.CreateYearlyAcreageExpense(Projects[0]);
            AddedExpense.ShouldBeNull();
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
            AddedExpense.ShouldBeNull();

            await ExpenseService.CreateYearlyAcreageExpense(Projects[0]);
            AddedExpense.ShouldBeNull();
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
            AddedExpense.ShouldBeNull();

            await ExpenseService.CreateYearlyAcreageExpense(Projects[0]);
            AddedExpense.ShouldBeNull();
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
            AddedExpense.ShouldBeNull();

            var expenseServ = new ExpenseService(MockDbContext.Object, MockProjectHistoryService.Object,
                MockDateTimeService.Object);

            await expenseServ.CreateYearlyAcreageExpense(Projects[0]);
            AddedExpense.ShouldBeNull();
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
            AddedExpense.ShouldBeNull();

            var expenseServ = new ExpenseService(MockDbContext.Object, MockProjectHistoryService.Object,
                MockDateTimeService.Object);

            await expenseServ.CreateYearlyAcreageExpense(Projects[0]);
            AddedExpense.ShouldNotBeNull();
            MockProjectHistoryService.Verify(a => a.AcreageExpenseCreated(It.IsAny<int>(), It.IsAny<Expense>()), times: Times.Once);
            MockDbContext.Verify(a => a.SaveChangesAsync(It.IsAny<CancellationToken>()), times: Times.Once);

            AddedExpense.Type.ShouldBe(Projects[0].AcreageRate.Type);
            AddedExpense.Description.ShouldBe(Projects[0].AcreageRate.Description);
            AddedExpense.Price.ShouldBe(Projects[0].AcreageRate.Price);
            AddedExpense.Quantity.ShouldBe((decimal)Projects[0].Acres);
            AddedExpense.Total.ShouldBe(11500.00m);
            AddedExpense.ProjectId.ShouldBe(Projects[0].Id);
            AddedExpense.RateId.ShouldBe(Projects[0].AcreageRate.Id);
            AddedExpense.InvoiceId.ShouldBeNull();
            AddedExpense.CreatedOn.Date.ShouldBe(date.Date); 
            AddedExpense.CreatedBy.ShouldBeNull();
            AddedExpense.Account.ShouldBe(Projects[0].AcreageRate.Account);
        }

        //re-write these for the new method        [Theory]
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
            AddedExpense.ShouldBeNull();

            var newQuote = CreateValidEntities.QuoteDetail();
            newQuote.AcreageRateId = 2;
            newQuote.Acres = 4;
            var originalQuote = CreateValidEntities.QuoteDetail();
            originalQuote.AcreageRateId = 1;
            originalQuote.Acres = 3;

            var expenseServ = new ExpenseService(MockDbContext.Object, MockProjectHistoryService.Object,
                MockDateTimeService.Object);

            await expenseServ.CreateChangeRequestAdjustmentMaybe(Projects[0], newQuote, originalQuote );
            AddedExpense.ShouldBeNull();
            MockProjectHistoryService.Verify(a => a.AcreageExpenseCreated(It.IsAny<int>(), It.IsAny<Expense>()), times: Times.Never);
            MockDbContext.Verify(a => a.SaveChangesAsync(It.IsAny<CancellationToken>()), times: Times.Never);
            MockDbContext.Verify(a => a.SaveChanges(), times: Times.Never);
        }

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
            AddedExpense.ShouldBeNull();

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
            AddedExpense.ShouldNotBeNull();
            MockProjectHistoryService.Verify(a => a.AcreageExpenseCreated(It.IsAny<int>(), It.IsAny<Expense>()), times: Times.Once);
            MockDbContext.Verify(a => a.SaveChangesAsync(It.IsAny<CancellationToken>()), times: Times.Once);
            //MockDbContext.Verify(a => a.SaveChanges(), times: Times.Never);
            AddedExpense.CreatedBy.ShouldBeNull();
            AddedExpense.CreatedOn.ShouldBe(new DateTime(year, month, day).FromPacificTime());
            AddedExpense.Type.ShouldBe(Rate.Types.Adjustment);
            AddedExpense.RateId.ShouldBe(1); //The old rateId
            AddedExpense.Account.ShouldBe(oldRate.Account);
            AddedExpense.Description.ShouldBe($"Acreage Adjustment (Refund) -- {oldRate.Description}");
            AddedExpense.Price.ShouldBe((oldRate.Price * -1));
        }

        //[Theory]
        //[InlineData(0.01, 11.50)]
        //[InlineData(0.02, 23.00)]
        //[InlineData(1, 1150.00)]
        //[InlineData(2, 2300.00)]
        //[InlineData(0.001, 1.15)]
        //[InlineData(0.00001, 0.01)]
        //public async Task CreateChangeRequestAdjustmentTests(decimal acres, decimal expectedAmount)
        //{
        //    SetupData();
        //    MockData();
        //    MockDateTimeService.Setup(a => a.DateTimeUtcNow()).Returns(DateTime.UtcNow);
        //    AddedExpense.ShouldBeNull();

        //    var expenseServ = new ExpenseService(MockDbContext.Object, MockProjectHistoryService.Object,
        //        MockDateTimeService.Object);

        //    await expenseServ.CreateChangeRequestAdjustment(Projects[0], acres);
        //    AddedExpense.ShouldNotBeNull();
        //    MockProjectHistoryService.Verify(a => a.AcreageExpenseCreated(It.IsAny<int>(), It.IsAny<Expense>()), times: Times.Once);
        //    MockDbContext.Verify(a => a.SaveChangesAsync(It.IsAny<CancellationToken>()), times: Times.Once);

        //    AddedExpense.Type.ShouldBe("Adjustment");
        //    AddedExpense.Description.ShouldBe($"Acreage Adjustment -- {Projects[0].AcreageRate.Description}");
        //    AddedExpense.Price.ShouldBe( Projects[0].AcreageRate.Price);
        //    AddedExpense.Quantity.ShouldBe(acres);
        //    AddedExpense.Total.ShouldBe(expectedAmount);
        //    AddedExpense.ProjectId.ShouldBe(Projects[0].Id);
        //    AddedExpense.RateId.ShouldBe(Projects[0].AcreageRate.Id);
        //    AddedExpense.InvoiceId.ShouldBeNull();
        //    AddedExpense.CreatedOn.Date.ShouldBe(DateTime.UtcNow.Date); //Don't really care about mocking it here.
        //    AddedExpense.CreatedBy.ShouldBeNull();
        //    AddedExpense.Account.ShouldBe(Projects[0].AcreageRate.Account);
        //}


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
            AddedExpense.ShouldBeNull();

            await ExpenseService.CreateYearlyAcreageExpense(Projects[0]);
            AddedExpense.ShouldNotBeNull();
            MockProjectHistoryService.Verify(a => a.AcreageExpenseCreated(It.IsAny<int>(), It.IsAny<Expense>()), times: Times.Once);
            MockDbContext.Verify(a => a.SaveChangesAsync(It.IsAny<CancellationToken>()), times: Times.Once);

            AddedExpense.Type.ShouldBe(Projects[0].AcreageRate.Type);
            AddedExpense.Description.ShouldBe(Projects[0].AcreageRate.Description);
            AddedExpense.Price.ShouldBe(Projects[0].AcreageRate.Price);
            AddedExpense.Quantity.ShouldBe((decimal)Projects[0].Acres);
            AddedExpense.Total.ShouldBe(expectedAmount);
            AddedExpense.ProjectId.ShouldBe(Projects[0].Id);
            AddedExpense.RateId.ShouldBe(Projects[0].AcreageRate.Id);
            AddedExpense.InvoiceId.ShouldBeNull();
            AddedExpense.CreatedOn.Date.ShouldBe(DateTime.UtcNow.Date); //This could fail on some unique runs
            AddedExpense.CreatedBy.ShouldBeNull();
            AddedExpense.Account.ShouldBe(Projects[0].AcreageRate.Account);
        }
    }
}
