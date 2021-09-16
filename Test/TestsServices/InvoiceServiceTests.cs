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

            var devSet = new DevSettings {RecreateDb = false, NightlyInvoices = true, UseSql = false};

            MockDevSettings.Setup(a => a.Value).Returns(devSet);

            InvoiceServ = new InvoiceService(MockDbContext.Object, MockProjectHistoryService.Object, MockEmailService.Object,
                MockExpenseService.Object, MockDevSettings.Object);
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

        [Fact]
        public async Task ProjectRunsOutOfMoney()
        {
            SetupData();
            Projects[0].QuoteTotal = Expenses.Where(a => a.ProjectId == Projects[0].Id).Sum(a => a.Total) - 10.00m;
            MockData();
            Projects[0].IsActive.ShouldBe(true);
            Projects[0].Status.ShouldBe(Project.Statuses.Active);


            var rtValue = await InvoiceServ.CreateInvoice(Projects[0].Id);
            //TODO Check email sent if first day of month thing.

            rtValue.ShouldNotBeNull();
            rtValue.IsError.ShouldBeTrue();
            rtValue.Message.ShouldBe($"Project expenses exceed quote: {Projects[0].Id}, invoiceAmount: 4600.0000, quoteRemaining: 4590.0000");
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
            }
            catch (Exception e)
            {
                throw;
            }
        }
    }
}
