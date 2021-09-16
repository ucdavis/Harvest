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
using System.Threading;
using System.Threading.Tasks;
using Harvest.Core.Extensions;
using Test.Helpers;
using TestHelpers.Helpers;
using Xunit;

namespace Test.TestsServices
{
    [Trait("Category", "ServiceTest")]
    public class InvoiceServiceTests
    {
        public Mock<AppDbContext> MockDbContext { get; set; }
        public Mock<IProjectHistoryService> MockProjectHistoryService { get; set; }
        public Mock<IEmailService> MockEmailService { get; set; }
        public Mock<IExpenseService> MockExpenseService { get; set; }
        public Mock<IOptions<DevSettings>> MockDevSettings { get; set; }
        public Mock<IDateTimeService> MockDateTimeService { get; set; }

        public List<Project> Projects { get; set; }
        public List<Expense> Expenses { get; set; }
        private InvoiceService InvoiceServ { get; set; }

        public InvoiceServiceTests()
        {
            MockDbContext = new Mock<AppDbContext>(new DbContextOptions<AppDbContext>());
            MockProjectHistoryService = new Mock<IProjectHistoryService>();
            MockEmailService = new Mock<IEmailService>();
            MockExpenseService = new Mock<IExpenseService>();
            MockDevSettings = new Mock<IOptions<DevSettings>>();
            MockDateTimeService = new Mock<IDateTimeService>();

            var devSet = new DevSettings {RecreateDb = false, NightlyInvoices = true, UseSql = false};

            MockDevSettings.Setup(a => a.Value).Returns(devSet);

            InvoiceServ = new InvoiceService(MockDbContext.Object, MockProjectHistoryService.Object, MockEmailService.Object,
                MockExpenseService.Object, MockDevSettings.Object, MockDateTimeService.Object);
        }


        [Theory]
        [InlineData(Project.Statuses.Active, false)]
        [InlineData(Project.Statuses.Requested, false)]
        [InlineData(Project.Statuses.Requested, true)]
        [InlineData(Project.Statuses.Canceled, false)]
        [InlineData(Project.Statuses.Canceled, true)]
        [InlineData(Project.Statuses.ChangeRequested, false)]
        [InlineData(Project.Statuses.ChangeRequested, true)]
        [InlineData(Project.Statuses.QuoteRejected, false)]
        [InlineData(Project.Statuses.QuoteRejected, true)]
        [InlineData(Project.Statuses.AwaitingCloseout, false)]
        [InlineData(Project.Statuses.AwaitingCloseout, true)]
        [InlineData(Project.Statuses.Completed, false)]
        [InlineData(Project.Statuses.Completed, true)]
        [InlineData(Project.Statuses.FinalInvoicePending, false)]
        [InlineData(Project.Statuses.FinalInvoicePending, true)]
        [InlineData(Project.Statuses.PendingAccountApproval, false)]
        [InlineData(Project.Statuses.PendingAccountApproval, true)]
        [InlineData(Project.Statuses.PendingApproval, false)]
        [InlineData(Project.Statuses.PendingApproval, true)]
        public async Task ProjectsThatDoNotRun(string status, bool active)
        {
            SetupData();
            Projects[1].IsActive = active;
            Projects[1].Status = status;
            MockData();

            Projects[1].IsActive.ShouldBe(active);
            Projects[1].Status.ShouldBe(status);

            var rtValue = await InvoiceServ.CreateInvoice(Projects[1].Id);
            rtValue.ShouldNotBeNull();
            rtValue.IsError.ShouldBeTrue();
            rtValue.Message.ShouldBe($"No active project found for given projectId: {Projects[1].Id}");
            
            MockDbContext.Verify(a => a.SaveChangesAsync(It.IsAny<CancellationToken>()), times: Times.Never);
        }

        [Theory]
        [InlineData(2020, 01, 04)]
        [InlineData(2020, 01, 05)]
        [InlineData(2020, 01, 11)]
        [InlineData(2020, 01, 12)]
        [InlineData(2020, 01, 18)]
        [InlineData(2020, 01, 19)]
        [InlineData(2020, 01, 25)]
        [InlineData(2020, 01, 26)]
        public async Task OnlyRunsWeekdays(int year, int month, int day)
        {
            var date = new DateTime(year, month, day).FromPacificTime();
            SetupData();
            MockData();
            var devSet = new DevSettings { RecreateDb = false, NightlyInvoices = false, UseSql = false };
            MockDevSettings.Setup(a => a.Value).Returns(devSet);
            Projects[0].IsActive.ShouldBe(true);;
            Projects[0].Status.ShouldBe(Project.Statuses.Active);
            MockDateTimeService.Setup(a => a.DateTimeUtcNow()).Returns(date);

            //Use this invoice because we changed the Dev Settings
            var invoiceServ = new InvoiceService(MockDbContext.Object, MockProjectHistoryService.Object, MockEmailService.Object,
                MockExpenseService.Object, MockDevSettings.Object, MockDateTimeService.Object);

            var rtValue = await invoiceServ.CreateInvoice(Projects[0].Id);

            rtValue.ShouldNotBeNull();
            rtValue.IsError.ShouldBeTrue();
            rtValue.Message.ShouldBe("Invoices can only be created Monday through Friday: 1");
            MockDbContext.Verify(a => a.SaveChangesAsync(It.IsAny<CancellationToken>()), times: Times.Never);
        }

        [Theory]
        [InlineData(2020, 01, 01)]
        [InlineData(2020, 01, 02)]
        [InlineData(2020, 01, 03)]
        [InlineData(2020, 01, 06)]
        [InlineData(2020, 01, 07)]
        [InlineData(2020, 01, 08)]
        [InlineData(2020, 01, 30)]
        [InlineData(2020, 01, 31)]
        public async Task DoesNotRunIfInvoiceInMonthExists(int year, int month, int day)
        {
            var date = new DateTime(year, month, day).FromPacificTime();
            SetupData();
            var invoices = new List<Invoice>();
            for (int i = 0; i < 3; i++)
            {
                var invoice = CreateValidEntities.Invoice(i + 1, Projects[i].Id);
                invoice.CreatedOn = new DateTime(2020, 01, 01); //Invoice created on first of month
                invoices.Add(invoice);
            }
            MockData();
            var devSet = new DevSettings { RecreateDb = false, NightlyInvoices = false, UseSql = false };
            MockDevSettings.Setup(a => a.Value).Returns(devSet);
            MockDbContext.Setup(a => a.Invoices).Returns(invoices.AsQueryable().MockAsyncDbSet().Object);


            Projects[0].IsActive.ShouldBe(true); ;
            Projects[0].Status.ShouldBe(Project.Statuses.Active);
            MockDateTimeService.Setup(a => a.DateTimeUtcNow()).Returns(date);

            //Use this invoice because we changed the Dev Settings
            var invoiceServ = new InvoiceService(MockDbContext.Object, MockProjectHistoryService.Object, MockEmailService.Object,
                MockExpenseService.Object, MockDevSettings.Object, MockDateTimeService.Object);

            var rtValue = await invoiceServ.CreateInvoice(Projects[0].Id);

            rtValue.ShouldNotBeNull();
            rtValue.IsError.ShouldBeTrue();
            rtValue.Message.ShouldBe("An invoice already exists for current month: 1");
            MockDbContext.Verify(a => a.SaveChangesAsync(It.IsAny<CancellationToken>()), times: Times.Never);
        }

        [Theory]
        [InlineData(2020, 02, 01)]
        [InlineData(2020, 02, 02)]
        [InlineData(2020, 02, 04)]
        [InlineData(2020, 02, 05)]
        [InlineData(2020, 02, 06)]
        public async Task ProjectRunsOutOfMoneyDoesNotSendEmail(int year, int month, int day)
        {
            //Feb 3, 2020 is the first business day of the month
            var date = new DateTime(year, month, day).FromPacificTime();
            SetupData();
            Projects[0].QuoteTotal = Expenses.Where(a => a.ProjectId == Projects[0].Id).Sum(a => a.Total) - 10.00m;
            MockData();
            Projects[0].IsActive.ShouldBe(true);
            Projects[0].Status.ShouldBe(Project.Statuses.Active);
            MockDateTimeService.Setup(a => a.DateTimeUtcNow()).Returns(date); 

            var rtValue = await InvoiceServ.CreateInvoice(Projects[0].Id);

            rtValue.ShouldNotBeNull();
            rtValue.IsError.ShouldBeTrue();
            rtValue.Message.ShouldBe($"Project expenses exceed quote: {Projects[0].Id}, invoiceAmount: 4600.0000, quoteRemaining: 4590.0000");
            MockEmailService.Verify(a => a.InvoiceExceedsQuote(It.IsAny<Project>(), It.IsAny<decimal>(), It.IsAny<decimal>()), Times.Never);
            MockDbContext.Verify(a => a.SaveChangesAsync(It.IsAny<CancellationToken>()), times: Times.Never);
        }
        [Theory]
        [InlineData(2020, 01, 01)]
        [InlineData(2020, 02, 03)]
        [InlineData(2020, 03, 02)]
        [InlineData(2020, 04, 01)]
        [InlineData(2020, 05, 01)]
        [InlineData(2020, 06, 01)]
        [InlineData(2020, 08, 03)]
        public async Task ProjectRunsOutOfMoneyOnFirstBusinessDayOfMonth(int year, int month, int day)
        {
            var date = new DateTime(year, month, day).FromPacificTime();
            SetupData();
            Projects[0].QuoteTotal = Expenses.Where(a => a.ProjectId == Projects[0].Id).Sum(a => a.Total) - 10.00m;
            MockData();
            Projects[0].IsActive.ShouldBe(true);
            Projects[0].Status.ShouldBe(Project.Statuses.Active);
            MockDateTimeService.Setup(a => a.DateTimeUtcNow()).Returns(date);

            var rtValue = await InvoiceServ.CreateInvoice(Projects[0].Id);

            rtValue.ShouldNotBeNull();
            rtValue.IsError.ShouldBeTrue();
            rtValue.Message.ShouldBe($"Project expenses exceed quote: {Projects[0].Id}, invoiceAmount: 4600.0000, quoteRemaining: 4590.0000");
            MockEmailService.Verify(a => a.InvoiceExceedsQuote(Projects[0], 4600.00m, 4590.00m), Times.Once);
            MockDbContext.Verify(a => a.SaveChangesAsync(It.IsAny<CancellationToken>()), times: Times.Never);
        }

        [Fact]
        public async Task ActiveProjectBlahBlah()
        {

            SetupData();
            MockData();
            Projects[0].IsActive.ShouldBe(true);
            Projects[0].Status.ShouldBe(Project.Statuses.Active);


            var rtValue = await InvoiceServ.CreateInvoice(Projects[0].Id);
        }
        

        private void SetupData()
        {
            Projects = new List<Project>();
            for (int i = 0; i < 3; i++)
            {
                Projects.Add(CreateValidEntities.Project(i+1));
            }
            Projects[1].IsActive = false;

            Expenses = new List<Expense>();
            for (int i = 0; i < 6; i++)
            {
                Expenses.Add(CreateValidEntities.Expense(i+1, Projects[(i%3)].Id)); //Use Mod to divide expenses between the 3 projects
            }

        }

        private void MockData()
        {

            try
            {
                MockDbContext.Setup(a => a.Projects).Returns(Projects.AsQueryable().MockAsyncDbSet().Object);
                MockDbContext.Setup(a => a.Expenses).Returns(Expenses.AsQueryable().MockAsyncDbSet().Object);
                MockDateTimeService.Setup(a => a.DateTimeUtcNow()).Returns(DateTime.UtcNow);
                MockEmailService.Setup(a => a.InvoiceExceedsQuote(It.IsAny<Project>(), It.IsAny<decimal>(),It.IsAny<decimal>())).ReturnsAsync(true);
            }
            catch (Exception e)
            {
                throw;
            }
        }
    }
}
