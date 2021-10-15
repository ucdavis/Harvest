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
        private void SetupData()
        {
            Projects = new List<Project>();
            for (int i = 0; i < 3; i++)
            {
                Projects.Add(CreateValidEntities.Project(i + 1));
            }
            Projects[1].IsActive = false;

            Expenses = new List<Expense>();
            for (int i = 0; i < 6; i++)
            {
                Expenses.Add(CreateValidEntities.Expense(i + 1, Projects[(i % 3)].Id)); //Use Mod to divide expenses between the 3 projects
            }

        }

        private void MockData()
        {

            MockDbContext.Setup(a => a.Projects).Returns(Projects.AsQueryable().MockAsyncDbSet().Object);
            MockDbContext.Setup(a => a.Expenses).Returns(Expenses.AsQueryable().MockAsyncDbSet().Object);
            MockDateTimeService.Setup(a => a.DateTimeUtcNow()).Returns(DateTime.UtcNow);
            MockEmailService.Setup(a => a.InvoiceExceedsQuote(It.IsAny<Project>(), It.IsAny<decimal>(), It.IsAny<decimal>())).ReturnsAsync(true);
            MockExpenseService.Setup(a => a.CreateYearlyAcreageExpense(Projects[0]));
            MockDateTimeService.Setup(a => a.DateTimeUtcNow()).Returns(new DateTime(2021, 01, 01));
        }

        /// <summary>
        /// Only Status of Active and Project.IsActive will run
        /// </summary>
        /// <param name="status"></param>
        /// <param name="active"></param>
        /// <returns></returns>
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
        public async Task ProjectThatDoNotRun(string status, bool active)
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

        /// <summary>
        /// Id not found
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task ProjectNotFound()
        {
            SetupData();
            MockData();

            var rtValue = await InvoiceServ.CreateInvoice(99);
            rtValue.ShouldNotBeNull();
            rtValue.IsError.ShouldBeTrue();
            rtValue.Message.ShouldBe($"No active project found for given projectId: 99");

            MockDbContext.Verify(a => a.SaveChangesAsync(It.IsAny<CancellationToken>()), times: Times.Never);
        }

        /// <summary>
        /// Test that it doesn't run on weekends
        /// </summary>
        /// <param name="year"></param>
        /// <param name="month"></param>
        /// <param name="day"></param>
        /// <returns></returns>
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

        /// <summary>
        /// This makes sure that an invoice is only created the month after the project starts.
        /// </summary>
        /// <param name="year"></param>
        /// <param name="month"></param>
        /// <param name="day"></param>
        /// <returns></returns>
        [Theory]
        [InlineData(2020, 01, 01)]
        [InlineData(2020, 01, 02)]
        [InlineData(2020, 01, 03)]
        [InlineData(2020, 01, 31)]
        public async Task DoesNotCreateInvoiceUntilFollowingMonthJan1(int year, int month, int day)
        {
            var date = new DateTime(year, month, day).FromPacificTime();
            SetupData();
            var invoices = new List<Invoice>();
            for (int i = 0; i < 3; i++)
            {
                var invoice = CreateValidEntities.Invoice(i + 1, Projects[1].Id);
                invoice.CreatedOn = new DateTime(2020, 01, 01); 
                invoices.Add(invoice);
            }
            MockDbContext.Setup(a => a.Invoices).Returns(invoices.AsQueryable().MockAsyncDbSet().Object);
            MockData();
            var devSet = new DevSettings { RecreateDb = false, NightlyInvoices = false, UseSql = false };
            MockDevSettings.Setup(a => a.Value).Returns(devSet);
            Projects[0].IsActive.ShouldBe(true); ;
            Projects[0].Status.ShouldBe(Project.Statuses.Active);
            Projects[0].Start = new DateTime(2020, 01, 01).Date.FromPacificTime();
            MockDateTimeService.Setup(a => a.DateTimeUtcNow()).Returns(date);

            //Use this invoice because we changed the Dev Settings
            var invoiceServ = new InvoiceService(MockDbContext.Object, MockProjectHistoryService.Object, MockEmailService.Object,
                MockExpenseService.Object, MockDevSettings.Object, MockDateTimeService.Object);

            var rtValue = await invoiceServ.CreateInvoice(Projects[0].Id);

            rtValue.ShouldNotBeNull();
            rtValue.IsError.ShouldBeTrue();
            rtValue.Message.ShouldBe("Project has not yet started in following month: 1");
            MockDbContext.Verify(a => a.SaveChangesAsync(It.IsAny<CancellationToken>()), times: Times.Never);
        }

        /// <summary>
        /// This makes sure that an invoice is only created the month after the project starts. (same as above except this one starts Feb 3 (The Monday)
        /// The 4th and 5th tests would work, because there isn't an invoice created yet.
        /// </summary>
        /// <param name="year"></param>
        /// <param name="month"></param>
        /// <param name="day"></param>
        /// <returns></returns>
        [Theory]
        [InlineData(2020, 02, 03)]
        [InlineData(2020, 02, 04)]
        [InlineData(2020, 02, 05)]
        public async Task DoesCreateInvoiceFollowingMonthJan1(int year, int month, int day)
        {
            var date = new DateTime(year, month, day).FromPacificTime();
            SetupData();
            var invoices = new List<Invoice>();
            for (int i = 0; i < 3; i++)
            {
                var invoice = CreateValidEntities.Invoice(i + 1, Projects[1].Id);
                invoice.CreatedOn = new DateTime(2020, 01, 01);
                invoices.Add(invoice);
            }
            MockDbContext.Setup(a => a.Invoices).Returns(invoices.AsQueryable().MockAsyncDbSet().Object);
            MockData();
            var devSet = new DevSettings { RecreateDb = false, NightlyInvoices = false, UseSql = false };
            MockDevSettings.Setup(a => a.Value).Returns(devSet);
            Projects[0].IsActive.ShouldBe(true); ;
            Projects[0].Status.ShouldBe(Project.Statuses.Active);
            Projects[0].Start = new DateTime(2020, 01, 01).Date.FromPacificTime();
            MockDateTimeService.Setup(a => a.DateTimeUtcNow()).Returns(date);
            Invoice addedInvoice = null;
            MockDbContext.Setup(a => a.Invoices.Add(It.IsAny<Invoice>())).Callback<Invoice>(r => addedInvoice = r);

            //Use this invoice because we changed the Dev Settings
            var invoiceServ = new InvoiceService(MockDbContext.Object, MockProjectHistoryService.Object, MockEmailService.Object,
                MockExpenseService.Object, MockDevSettings.Object, MockDateTimeService.Object);

            var rtValue = await invoiceServ.CreateInvoice(Projects[0].Id);

            rtValue.ShouldNotBeNull();
            rtValue.IsError.ShouldBeFalse();
            MockDbContext.Verify(a => a.SaveChangesAsync(It.IsAny<CancellationToken>()), times: Times.Once);

            MockDbContext.Verify(a => a.Invoices.Add(It.IsAny<Invoice>()), times: Times.Once);
            addedInvoice.ShouldNotBeNull();
            addedInvoice.Expenses.ShouldNotBeNull();
            addedInvoice.Expenses.Any().ShouldBe(true);
            addedInvoice.CreatedOn.Date.ShouldBe(new DateTime(year, month, day).Date);
            addedInvoice.ProjectId.ShouldBe(Projects[0].Id);
            addedInvoice.Status.ShouldBe(Invoice.Statuses.Created);
            addedInvoice.Total.ShouldBe(4600.00m);

            Projects[0].Status.ShouldBe(Project.Statuses.Active);
        }

        /// <summary>
        /// This one has the project start date of Jan 20, instead of Jan 1
        /// </summary>
        /// <param name="year"></param>
        /// <param name="month"></param>
        /// <param name="day"></param>
        /// <returns></returns>
        [Theory]
        [InlineData(2020, 01, 01)]
        [InlineData(2020, 01, 02)]
        [InlineData(2020, 01, 03)]
        [InlineData(2020, 01, 31)]
        public async Task DoesNotCreateInvoiceUntilFollowingMonthJan20(int year, int month, int day)
        {
            var date = new DateTime(year, month, day).FromPacificTime();
            SetupData();
            var invoices = new List<Invoice>();
            for (int i = 0; i < 3; i++)
            {
                var invoice = CreateValidEntities.Invoice(i + 1, Projects[1].Id);
                invoice.CreatedOn = new DateTime(2020, 01, 01);
                invoices.Add(invoice);
            }
            MockDbContext.Setup(a => a.Invoices).Returns(invoices.AsQueryable().MockAsyncDbSet().Object);
            MockData();
            var devSet = new DevSettings { RecreateDb = false, NightlyInvoices = false, UseSql = false };
            MockDevSettings.Setup(a => a.Value).Returns(devSet);
            Projects[0].IsActive.ShouldBe(true); ;
            Projects[0].Status.ShouldBe(Project.Statuses.Active);
            Projects[0].Start = new DateTime(2020, 01, 20).Date.FromPacificTime(); //Change start to 20th
            MockDateTimeService.Setup(a => a.DateTimeUtcNow()).Returns(date);

            //Use this invoice because we changed the Dev Settings
            var invoiceServ = new InvoiceService(MockDbContext.Object, MockProjectHistoryService.Object, MockEmailService.Object,
                MockExpenseService.Object, MockDevSettings.Object, MockDateTimeService.Object);

            var rtValue = await invoiceServ.CreateInvoice(Projects[0].Id);

            rtValue.ShouldNotBeNull();
            rtValue.IsError.ShouldBeTrue();
            rtValue.Message.ShouldBe("Project has not yet started in following month: 1");
            MockDbContext.Verify(a => a.SaveChangesAsync(It.IsAny<CancellationToken>()), times: Times.Never);
        }
        /// <summary>
        /// This makes sure that an invoice is only created the month after the project starts. Have Project Start date Jan 20
        /// The 4th and 5th tests would work, because there isn't an invoice created yet.
        /// </summary>
        /// <param name="year"></param>
        /// <param name="month"></param>
        /// <param name="day"></param>
        /// <returns></returns>
        [Theory]
        [InlineData(2020, 02, 03)]
        [InlineData(2020, 02, 04)]
        [InlineData(2020, 02, 05)]
        public async Task DoesCreateInvoiceFollowingMonthJan20(int year, int month, int day)
        {
            var date = new DateTime(year, month, day).FromPacificTime();
            SetupData();
            var invoices = new List<Invoice>();
            for (int i = 0; i < 3; i++)
            {
                var invoice = CreateValidEntities.Invoice(i + 1, Projects[1].Id);
                invoice.CreatedOn = new DateTime(2020, 01, 01);
                invoices.Add(invoice);
            }
            MockDbContext.Setup(a => a.Invoices).Returns(invoices.AsQueryable().MockAsyncDbSet().Object);
            MockData();
            var devSet = new DevSettings { RecreateDb = false, NightlyInvoices = false, UseSql = false };
            MockDevSettings.Setup(a => a.Value).Returns(devSet);
            Projects[0].IsActive.ShouldBe(true); ;
            Projects[0].Status.ShouldBe(Project.Statuses.Active);
            Projects[0].Start = new DateTime(2020, 01, 20).Date.FromPacificTime();
            MockDateTimeService.Setup(a => a.DateTimeUtcNow()).Returns(date);
            Invoice addedInvoice = null;
            MockDbContext.Setup(a => a.Invoices.Add(It.IsAny<Invoice>())).Callback<Invoice>(r => addedInvoice = r);

            //Use this invoice because we changed the Dev Settings
            var invoiceServ = new InvoiceService(MockDbContext.Object, MockProjectHistoryService.Object, MockEmailService.Object,
                MockExpenseService.Object, MockDevSettings.Object, MockDateTimeService.Object);

            var rtValue = await invoiceServ.CreateInvoice(Projects[0].Id);

            rtValue.ShouldNotBeNull();
            rtValue.IsError.ShouldBeFalse();
            MockDbContext.Verify(a => a.SaveChangesAsync(It.IsAny<CancellationToken>()), times: Times.Once);

            MockDbContext.Verify(a => a.Invoices.Add(It.IsAny<Invoice>()), times: Times.Once);
            addedInvoice.ShouldNotBeNull();
            addedInvoice.Expenses.ShouldNotBeNull();
            addedInvoice.Expenses.Any().ShouldBe(true);
            addedInvoice.CreatedOn.Date.ShouldBe(new DateTime(year, month, day).Date);
            addedInvoice.ProjectId.ShouldBe(Projects[0].Id);
            addedInvoice.Status.ShouldBe(Invoice.Statuses.Created);
            addedInvoice.Total.ShouldBe(4600.00m);

            Projects[0].Status.ShouldBe(Project.Statuses.Active);
        }

        /// <summary>
        /// Chck that a project past the end date sets the Awaiting Closeout status
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task GetsPastFollowingMonthAndSetsUpAwaitingCloseout()
        {
            //Kind of hacky. Setting the end date to 10 ish dates after the start so we can test that this runs in the first of the next month, but then we await closeout
            var date = new DateTime(2020, 02, 03).FromPacificTime();
            SetupData();
            var invoices = new List<Invoice>();
            for (int i = 0; i < 3; i++)
            {
                var invoice = CreateValidEntities.Invoice(i + 1, Projects[1].Id);
                invoice.CreatedOn = new DateTime(2020, 01, 20);
                invoices.Add(invoice);
            }
            MockDbContext.Setup(a => a.Invoices).Returns(invoices.AsQueryable().MockAsyncDbSet().Object);
            MockData();
            var devSet = new DevSettings { RecreateDb = false, NightlyInvoices = false, UseSql = false };
            MockDevSettings.Setup(a => a.Value).Returns(devSet);
            Projects[0].IsActive.ShouldBe(true); ;
            Projects[0].Status.ShouldBe(Project.Statuses.Active);
            Projects[0].Start = new DateTime(2020, 01, 01).Date.FromPacificTime();
            Projects[0].End = new DateTime(2020, 02, 02).Date.FromPacificTime();
            MockDateTimeService.Setup(a => a.DateTimeUtcNow()).Returns(date);

            //Use this invoice because we changed the Dev Settings
            var invoiceServ = new InvoiceService(MockDbContext.Object, MockProjectHistoryService.Object, MockEmailService.Object,
                MockExpenseService.Object, MockDevSettings.Object, MockDateTimeService.Object);

            var rtValue = await invoiceServ.CreateInvoice(Projects[0].Id);

            rtValue.ShouldNotBeNull();
            rtValue.IsError.ShouldBeTrue();
            rtValue.Message.ShouldBe("Can't create invoice for project past its end date: 1");
            MockDbContext.Verify(a => a.SaveChangesAsync(It.IsAny<CancellationToken>()), times: Times.Once);
            Projects[0].Status.ShouldBe(Project.Statuses.AwaitingCloseout);
        }

        /// <summary>
        /// Make sure expense service is called
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task CreateMonthlyAcreageExpenseCalledWhenFirstInvoiceOfTheMonth()
        {
            var date = new DateTime(2020, 02, 03).FromPacificTime();
            SetupData();
            var invoices = new List<Invoice>();
            for (int i = 0; i < 3; i++)
            {
                var invoice = CreateValidEntities.Invoice(i + 1, Projects[1].Id);
                invoice.CreatedOn = new DateTime(2020, 01, 20);
                invoices.Add(invoice);
            }
            MockDbContext.Setup(a => a.Invoices).Returns(invoices.AsQueryable().MockAsyncDbSet().Object);
            MockData();
            var devSet = new DevSettings { RecreateDb = false, NightlyInvoices = false, UseSql = false };
            MockDevSettings.Setup(a => a.Value).Returns(devSet);
            Projects[0].IsActive.ShouldBe(true); ;
            Projects[0].Status.ShouldBe(Project.Statuses.Active);
            Projects[0].Start = new DateTime(2020, 01, 01).Date.FromPacificTime();
            Projects[0].End = new DateTime(2021, 02, 02).Date.FromPacificTime();
            Projects[0].QuoteTotal = 100.00m; //Just set it low so it will exit early.
            MockDateTimeService.Setup(a => a.DateTimeUtcNow()).Returns(date);

            //Use this invoice because we changed the Dev Settings
            var invoiceServ = new InvoiceService(MockDbContext.Object, MockProjectHistoryService.Object, MockEmailService.Object,
                MockExpenseService.Object, MockDevSettings.Object, MockDateTimeService.Object);

            var rtValue = await invoiceServ.CreateInvoice(Projects[0].Id);

            MockExpenseService.Verify(a => a.CreateYearlyAcreageExpense(Projects[0]), Times.Once); 

            rtValue.ShouldNotBeNull();
            rtValue.IsError.ShouldBeTrue();
            rtValue.Message.ShouldBe("Project expenses exceed quote: 1, invoiceAmount: 4600.0000, quoteRemaining: 100.00"); //Because I'm not testing further than this...
            MockDbContext.Verify(a => a.SaveChangesAsync(It.IsAny<CancellationToken>()), times: Times.Never);
        }

        /// <summary>
        /// Make sure invoice is only created once per month and that the expense service isn't called
        /// </summary>
        /// <param name="year"></param>
        /// <param name="month"></param>
        /// <param name="day"></param>
        /// <returns></returns>
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
            MockExpenseService.Verify(a => a.CreateYearlyAcreageExpense(Projects[0]), Times.Never);

            MockEmailService.Verify(a => a.InvoiceCreated(It.IsAny<Invoice>()), Times.Never);
        }

        /// <summary>
        /// Make sue email service is not called when not the first business day
        /// </summary>
        /// <param name="year"></param>
        /// <param name="month"></param>
        /// <param name="day"></param>
        /// <returns></returns>
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
        /// <summary>
        /// Make sure email service is called on first business day of the month when the expense is too big
        /// </summary>
        /// <param name="year"></param>
        /// <param name="month"></param>
        /// <param name="day"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Doesn't generate invoice if no unbilled expenses and not closeout.
        /// This is assuming the expense service doesn't bill acreage
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task WhenNoUnbilledExpensesAndNotCloseout()
        {
            //This can happen if there is no acreage fee
            SetupData();
            foreach (var expense in Expenses.Where(a => a.ProjectId == Projects[0].Id))
            {
                //Set the expenses as billed
                expense.Invoice = CreateValidEntities.Invoice(9 + expense.Id, Projects[0].Id);
            }
            MockData();
            Projects[0].IsActive.ShouldBe(true);
            Projects[0].Status.ShouldBe(Project.Statuses.Active);


            var rtValue = await InvoiceServ.CreateInvoice(Projects[0].Id, false);
            rtValue.ShouldNotBeNull();
            rtValue.IsError.ShouldBeTrue();
            rtValue.Message.ShouldBe("No unbilled expenses found for project: 1");
            MockDbContext.Verify(a => a.SaveChangesAsync(It.IsAny<CancellationToken>()), times: Times.Never);
        }

        /// <summary>
        /// No unbilled expenses and it is close out
        /// Creates a completed invoice and sets project to completed
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task WhenNoUnbilledExpensesAndCloseout()
        {
            //This can happen if there is no acreage fee
            SetupData();
            foreach (var expense in Expenses.Where(a => a.ProjectId == Projects[0].Id))
            {
                //Set the expenses as billed
                expense.Invoice = CreateValidEntities.Invoice(9 + expense.Id, Projects[0].Id);
            }
            MockData();
            Projects[0].IsActive.ShouldBe(true);
            Projects[0].Status.ShouldBe(Project.Statuses.Active);
            Invoice addedInvoice = null;
            MockDbContext.Setup(a => a.Invoices.Add(It.IsAny<Invoice>())).Callback<Invoice>(r => addedInvoice = r);


            var rtValue = await InvoiceServ.CreateInvoice(Projects[0].Id, true);
            rtValue.ShouldNotBeNull();
            rtValue.IsError.ShouldBeFalse();
            MockDbContext.Verify(a => a.SaveChangesAsync(It.IsAny<CancellationToken>()), times: Times.Once);

            MockDbContext.Verify(a => a.Invoices.Add(It.IsAny<Invoice>()), times: Times.Once);
            addedInvoice.ShouldNotBeNull();
            addedInvoice.Expenses.ShouldNotBeNull();
            addedInvoice.Expenses.Any().ShouldBe(false);
            addedInvoice.CreatedOn.Date.ShouldBe(new DateTime(2021,01,01).Date);
            addedInvoice.ProjectId.ShouldBe(Projects[0].Id);
            addedInvoice.Status.ShouldBe(Invoice.Statuses.Completed);
            addedInvoice.Total.ShouldBe(0);

            Projects[0].Status.ShouldBe(Project.Statuses.Completed);
        }

        /// <summary>
        /// When there are unbilled expenses and close out
        /// Expect a pending invoice and project status to have final invoice pending
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task WhenUnbilledExpensesAndCloseout()
        {
            SetupData();
            MockData();
            Projects[0].IsActive.ShouldBe(true);
            Projects[0].Status.ShouldBe(Project.Statuses.Active);
            Invoice addedInvoice = null;
            MockDbContext.Setup(a => a.Invoices.Add(It.IsAny<Invoice>())).Callback<Invoice>(r => addedInvoice = r);


            var rtValue = await InvoiceServ.CreateInvoice(Projects[0].Id, true);
            rtValue.ShouldNotBeNull();
            rtValue.IsError.ShouldBeFalse();
            MockDbContext.Verify(a => a.SaveChangesAsync(It.IsAny<CancellationToken>()), times: Times.Once);

            MockDbContext.Verify(a => a.Invoices.Add(It.IsAny<Invoice>()), times: Times.Once);
            addedInvoice.ShouldNotBeNull();
            addedInvoice.Expenses.ShouldNotBeNull();
            addedInvoice.Expenses.Any().ShouldBe(true);
            addedInvoice.CreatedOn.Date.ShouldBe(new DateTime(2021, 01, 01).Date);
            addedInvoice.ProjectId.ShouldBe(Projects[0].Id);
            addedInvoice.Status.ShouldBe(Invoice.Statuses.Created);
            addedInvoice.Total.ShouldBe(4600.00m);

            Projects[0].Status.ShouldBe(Project.Statuses.FinalInvoicePending);
        }

        /// <summary>
        /// Test the dev override for creating nightly invoices
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task InvoiceCreatedForActiveProjectWithNightlySetting()
        {
            SetupData();
            MockData();
            Invoice addedInvoice = null;
            MockDbContext.Setup(a => a.Invoices.Add(It.IsAny<Invoice>())).Callback<Invoice>(r => addedInvoice = r);
            Projects[0].IsActive.ShouldBe(true);
            Projects[0].Status.ShouldBe(Project.Statuses.Active);


            var rtValue = await InvoiceServ.CreateInvoice(Projects[0].Id);
            rtValue.ShouldNotBeNull();
            rtValue.IsError.ShouldBeFalse();
            MockDbContext.Verify(a => a.SaveChangesAsync(It.IsAny<CancellationToken>()), times: Times.Once);

            MockDbContext.Verify(a => a.Invoices.Add(It.IsAny<Invoice>()), times: Times.Once);
            addedInvoice.ShouldNotBeNull();
            addedInvoice.Expenses.ShouldNotBeNull();
            addedInvoice.Expenses.Any().ShouldBe(true);
            addedInvoice.CreatedOn.Date.ShouldBe(new DateTime(2021, 01, 01).Date);
            addedInvoice.ProjectId.ShouldBe(Projects[0].Id);
            addedInvoice.Status.ShouldBe(Invoice.Statuses.Created);
            addedInvoice.Total.ShouldBe(4600.00m);

            Projects[0].Status.ShouldBe(Project.Statuses.Active);
        }

        /// <summary>
        /// Test invoice gets created first business day of month when no dev override
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task InvoiceCreatedForActiveProjectWithOutNightlySetting()
        {
            SetupData();
            var invoices = new List<Invoice>();
            for (int i = 0; i < 3; i++)
            {
                var invoice = CreateValidEntities.Invoice(i + 1, Projects[i].Id);
                invoice.CreatedOn = new DateTime(2020, 01, 01); //Invoice created on first of month
                invoices.Add(invoice);
            }

            var ignoredExpense = CreateValidEntities.Expense(95, Projects[0].Id);
            ignoredExpense.Approved = false;
            Expenses.Add(ignoredExpense);
            ignoredExpense = CreateValidEntities.Expense(95, Projects[0].Id);
            ignoredExpense.Invoice = CreateValidEntities.Invoice(88, Projects[0].Id);
            Expenses.Add(ignoredExpense);

            MockData();
            MockDateTimeService.Setup(a => a.DateTimeUtcNow()).Returns(new DateTime(2021, 10, 01));
            var devSet = new DevSettings { RecreateDb = false, NightlyInvoices = false, UseSql = false };
            MockDevSettings.Setup(a => a.Value).Returns(devSet);
            MockDbContext.Setup(a => a.Invoices).Returns(invoices.AsQueryable().MockAsyncDbSet().Object);
            Invoice addedInvoice = null;
            MockDbContext.Setup(a => a.Invoices.Add(It.IsAny<Invoice>())).Callback<Invoice>(r => addedInvoice = r);
            Projects[0].IsActive.ShouldBe(true);
            Projects[0].Status.ShouldBe(Project.Statuses.Active);

            var invoiceServ = new InvoiceService(MockDbContext.Object, MockProjectHistoryService.Object, MockEmailService.Object,
                MockExpenseService.Object, MockDevSettings.Object, MockDateTimeService.Object);
            var rtValue = await invoiceServ.CreateInvoice(Projects[0].Id);
            rtValue.ShouldNotBeNull();
            rtValue.IsError.ShouldBeFalse();
            MockDbContext.Verify(a => a.SaveChangesAsync(It.IsAny<CancellationToken>()), times: Times.Once);
            MockExpenseService.Verify(a => a.CreateYearlyAcreageExpense(It.IsAny<Project>()),Times.Once);

            MockDbContext.Verify(a => a.Invoices.Add(It.IsAny<Invoice>()), times: Times.Once);
            addedInvoice.ShouldNotBeNull();
            addedInvoice.Expenses.ShouldNotBeNull();
            addedInvoice.Expenses.Any().ShouldBe(true);
            addedInvoice.Expenses.Count.ShouldBe(2);
            addedInvoice.Expenses[0].Description.ShouldBe("Description1");
            addedInvoice.Expenses[1].Description.ShouldBe("Description4");

            addedInvoice.CreatedOn.Date.ShouldBe(new DateTime(2021, 10, 01).Date);
            addedInvoice.ProjectId.ShouldBe(Projects[0].Id);
            addedInvoice.Status.ShouldBe(Invoice.Statuses.Created);
            addedInvoice.Total.ShouldBe(4600.00m);
            addedInvoice.Expenses.Sum(a => a.Total).ShouldBe(4600.00m);

            Projects[0].Status.ShouldBe(Project.Statuses.Active);
        }

        [Fact]
        public async Task CreateInvoiceCallsEmailService()
        {
            SetupData();
            var invoices = new List<Invoice>();
            for (int i = 0; i < 3; i++)
            {
                var invoice = CreateValidEntities.Invoice(i + 1, Projects[i].Id);
                invoice.CreatedOn = new DateTime(2020, 01, 01); //Invoice created on first of month
                invoices.Add(invoice);
            }

            var ignoredExpense = CreateValidEntities.Expense(95, Projects[0].Id);
            ignoredExpense.Approved = false;
            Expenses.Add(ignoredExpense);
            ignoredExpense = CreateValidEntities.Expense(95, Projects[0].Id);
            ignoredExpense.Invoice = CreateValidEntities.Invoice(88, Projects[0].Id);
            Expenses.Add(ignoredExpense);

            MockData();
            MockDateTimeService.Setup(a => a.DateTimeUtcNow()).Returns(new DateTime(2021, 10, 01));
            var devSet = new DevSettings { RecreateDb = false, NightlyInvoices = false, UseSql = false };
            MockDevSettings.Setup(a => a.Value).Returns(devSet);
            MockDbContext.Setup(a => a.Invoices).Returns(invoices.AsQueryable().MockAsyncDbSet().Object);
            Invoice addedInvoice = null;
            MockDbContext.Setup(a => a.Invoices.Add(It.IsAny<Invoice>())).Callback<Invoice>(r => addedInvoice = r);
            Projects[0].IsActive.ShouldBe(true);
            Projects[0].Status.ShouldBe(Project.Statuses.Active);

            var invoiceServ = new InvoiceService(MockDbContext.Object, MockProjectHistoryService.Object, MockEmailService.Object,
                MockExpenseService.Object, MockDevSettings.Object, MockDateTimeService.Object);
            var rtValue = await invoiceServ.CreateInvoice(Projects[0].Id);
            rtValue.ShouldNotBeNull();
            rtValue.IsError.ShouldBeFalse();
            MockDbContext.Verify(a => a.SaveChangesAsync(It.IsAny<CancellationToken>()), times: Times.Once);
            MockExpenseService.Verify(a => a.CreateYearlyAcreageExpense(It.IsAny<Project>()), Times.Once);

            MockDbContext.Verify(a => a.Invoices.Add(It.IsAny<Invoice>()), times: Times.Once);
            
            Projects[0].Status.ShouldBe(Project.Statuses.Active);

            MockEmailService.Verify(a => a.InvoiceCreated(addedInvoice), times: Times.Once);
        }

        [Fact]
        public async Task CreateInvoicesWithOneProject()
        {
            SetupData();
            var invoices = new List<Invoice>();
            for (int i = 0; i < 3; i++)
            {
                var invoice = CreateValidEntities.Invoice(i + 1, Projects[i].Id);
                invoice.CreatedOn = new DateTime(2020, 01, 01); //Invoice created on first of month
                invoices.Add(invoice);
            }

            foreach (var project in Projects)
            {
                project.IsActive = false;
            }

            Projects[0].IsActive = true;

            var ignoredExpense = CreateValidEntities.Expense(95, Projects[0].Id);
            ignoredExpense.Approved = false;
            Expenses.Add(ignoredExpense);
            ignoredExpense = CreateValidEntities.Expense(95, Projects[0].Id);
            ignoredExpense.Invoice = CreateValidEntities.Invoice(88, Projects[0].Id);
            Expenses.Add(ignoredExpense);

            MockData();
            MockDateTimeService.Setup(a => a.DateTimeUtcNow()).Returns(new DateTime(2021, 10, 01));
            var devSet = new DevSettings { RecreateDb = false, NightlyInvoices = false, UseSql = false };
            MockDevSettings.Setup(a => a.Value).Returns(devSet);
            MockDbContext.Setup(a => a.Invoices).Returns(invoices.AsQueryable().MockAsyncDbSet().Object);
            Invoice addedInvoice = null;
            MockDbContext.Setup(a => a.Invoices.Add(It.IsAny<Invoice>())).Callback<Invoice>(r => addedInvoice = r);
            Projects[0].IsActive.ShouldBe(true);
            Projects[0].Status.ShouldBe(Project.Statuses.Active);

            var invoiceServ = new InvoiceService(MockDbContext.Object, MockProjectHistoryService.Object, MockEmailService.Object,
                MockExpenseService.Object, MockDevSettings.Object, MockDateTimeService.Object);

            //Note, just calling the create for all active projects.... Otherwise same as previous test
            var rtValue = await invoiceServ.CreateInvoices();
            
            
            rtValue.ShouldBe(1);
            MockDbContext.Verify(a => a.SaveChangesAsync(It.IsAny<CancellationToken>()), times: Times.Once);
            MockExpenseService.Verify(a => a.CreateYearlyAcreageExpense(It.IsAny<Project>()), Times.Once);

            MockDbContext.Verify(a => a.Invoices.Add(It.IsAny<Invoice>()), times: Times.Once);
            addedInvoice.ShouldNotBeNull();
            addedInvoice.Expenses.ShouldNotBeNull();
            addedInvoice.Expenses.Any().ShouldBe(true);
            addedInvoice.Expenses.Count.ShouldBe(2);
            addedInvoice.Expenses[0].Description.ShouldBe("Description1");
            addedInvoice.Expenses[1].Description.ShouldBe("Description4");

            addedInvoice.CreatedOn.Date.ShouldBe(new DateTime(2021, 10, 01).Date);
            addedInvoice.ProjectId.ShouldBe(Projects[0].Id);
            addedInvoice.Status.ShouldBe(Invoice.Statuses.Created);
            addedInvoice.Total.ShouldBe(4600.00m);
            addedInvoice.Expenses.Sum(a => a.Total).ShouldBe(4600.00m);

            Projects[0].Status.ShouldBe(Project.Statuses.Active);
        }

        [Theory]
        [InlineData(Invoice.Statuses.Completed)]
        [InlineData(Invoice.Statuses.Pending)]
        public async Task GetCreatedInvoicesReturnsBasedOnStatus(string statusesNotReturned)
        {
            var invoices = new List<Invoice>();
            invoices.Add(CreateValidEntities.Invoice(1, 99));
            invoices.Add(CreateValidEntities.Invoice(2, 99));

            invoices[0].Status = statusesNotReturned;
            invoices[1].Status = Invoice.Statuses.Created;

            MockDbContext.Setup(a => a.Invoices).Returns(invoices.AsQueryable().MockAsyncDbSet().Object);

            var rtValue = await InvoiceServ.GetCreatedInvoiceIds();
            rtValue.ShouldNotBeNull();
            rtValue.Count.ShouldBe(1);
            rtValue[0].ShouldBe(2);

        }

    }
}
