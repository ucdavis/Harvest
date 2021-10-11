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
        public ExpenseService ExpenseService { get; set; }

        public List<Project> Projects { get; set; }
        public List<Expense> Expenses { get; set; }
        public Expense AddedExpense { get; set; } = null;

        public ExpenseServiceTests()
        {
            MockDbContext = new Mock<AppDbContext>(new DbContextOptions<AppDbContext>());
            MockProjectHistoryService = new Mock<IProjectHistoryService>();

            ExpenseService = new ExpenseService(MockDbContext.Object, MockProjectHistoryService.Object);
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
        }

        private void MockData()
        {
            MockDbContext.Setup(a => a.Projects).Returns(Projects.AsQueryable().MockAsyncDbSet().Object);
            MockDbContext.Setup(a => a.Expenses).Returns(Expenses.AsQueryable().MockAsyncDbSet().Object);
            //MockUserService.Setup(a => a.GetCurrentUser()).ReturnsAsync(() => null);
            //MockUserService.Setup(a => a.GetCurrentUser()).ReturnsAsync(CreateValidEntities.User(1));

            MockDbContext.Setup(a => a.Expenses.Add(It.IsAny<Expense>())).Callback<Expense>(a => AddedExpense = a);
            MockDbContext.Setup(a => a.Expenses.AddAsync(It.IsAny<Expense>(), It.IsAny<System.Threading.CancellationToken>())).Callback((Expense a, CancellationToken token) => AddedExpense = a);
            MockProjectHistoryService.Setup(a => a.AcreageExpenseCreated(It.IsAny<int>(), It.IsAny<Expense>()));
        }

        #endregion

        //[Fact]
        //public async Task CreateAcreageExpenseThrowsExceptionWhenLessThanCent()
        //{
        //    var rtValue = await Should.ThrowAsync<Exception>( () => ExpenseService.CreateAcreageExpense(1, 0.009m));
        //    rtValue.ShouldNotBeNull();
        //    rtValue.Message.ShouldBe("Amount must be >= 0.01");
        //}

        //#region theory
        //[Theory]
        //[InlineData(0.01)]
        //[InlineData(0.014)]
        //[InlineData(0.015)]
        //[InlineData(0.016)]
        //[InlineData(0.02)]
        //[InlineData(0.024)]
        //[InlineData(0.025)]
        //[InlineData(0.026)]
        //[InlineData(1000000.00)]
        //[InlineData(1000000.01)]
        //#endregion
        //public async Task CreateAcreageExpenseCreatesExpectedExpense(decimal amount)
        //{
        //    SetupData();
        //    MockData();
        //    AddedExpense.ShouldBeNull();

        //    await ExpenseService.CreateAcreageExpense(Projects[0].Id, amount);
        //    AddedExpense.ShouldNotBeNull();
        //    MockProjectHistoryService.Verify(a => a.AcreageExpenseCreated(Projects[0].Id, AddedExpense), times: Times.Once);
        //    MockDbContext.Verify(a => a.SaveChangesAsync(It.IsAny<CancellationToken>()), times: Times.Once);

        //    AddedExpense.Type.ShouldBe(Projects[0].AcreageRate.Type);
        //    AddedExpense.Description.ShouldBe(Projects[0].AcreageRate.Description);
        //    AddedExpense.Price.ShouldBe(Projects[0].AcreageRate.Price);
        //    AddedExpense.Quantity.ShouldBe((decimal)Projects[0].Acres);
        //    AddedExpense.Total.ShouldBe(Math.Round(amount, 2, MidpointRounding.ToZero));
        //    AddedExpense.ProjectId.ShouldBe(Projects[0].Id);
        //    AddedExpense.RateId.ShouldBe(Projects[0].AcreageRate.Id);
        //    AddedExpense.InvoiceId.ShouldBeNull();
        //    AddedExpense.CreatedOn.Date.ShouldBe(DateTime.UtcNow.Date); //This could fail on some unique runs
        //    AddedExpense.CreatedBy.ShouldBeNull();
        //    AddedExpense.Account.ShouldBe(Projects[0].AcreageRate.Account);
        //}

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

        [Fact(Skip = "TODO")]
        public void CreateYearlyAcreageExpenseReturnsEarlyIfAnyAcreageExpenseExistsWithinTheLastYear()
        {
            return;
        }

        [Fact(Skip = "TODO")]
        public void CreateChangeRequestAdjustmentTests()
        {
            return;
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
