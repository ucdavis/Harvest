using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Harvest.Core.Data;
using Harvest.Core.Domain;
using Harvest.Core.Models.Settings;
using Harvest.Core.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.VisualBasic.CompilerServices;
using Moq;
using Shouldly;
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

        public InvoiceServiceTests()
        {
            MockDbContext = new Mock<AppDbContext>(new DbContextOptions<AppDbContext>());
            MockProjectHistoryService = new Mock<IProjectHistoryService>();
            MockEmailService = new Mock<IEmailService>();
            MockExpenseService = new Mock<IExpenseService>();
            MockDevSettings = new Mock<IOptions<DevSettings>>();

            var devSet = new DevSettings {RecreateDb = false, NightlyInvoices = true, UseSql = false};

            MockDevSettings.Setup(a => a.Value).Returns(devSet);
            
        }
        [Fact]
        public async Task TestSample()
        {
            var xxx = "1";
            xxx.ShouldBe("1");


            SetupData();


            var invoiceServ = new InvoiceService(MockDbContext.Object, MockProjectHistoryService.Object, MockEmailService.Object,
                MockExpenseService.Object, MockDevSettings.Object);

            await invoiceServ.CreateInvoice(1);

        }
        [Fact]
        public async Task TestInactiveProject()
        {
            SetupData();
            Projects[1].IsActive = false;
            MockData();

            Projects[1].IsActive.ShouldBeFalse();
            Projects[1].Status.ShouldBe(Project.Statuses.Active);

            var invoiceServ = new InvoiceService(MockDbContext.Object, MockProjectHistoryService.Object, MockEmailService.Object,
                MockExpenseService.Object, MockDevSettings.Object);
            var rtValue = await invoiceServ.CreateInvoice(Projects[1].Id);
            rtValue.ShouldNotBeNull();
            rtValue.IsError.ShouldBeTrue();
            rtValue.Message.ShouldBe($"No active project found for given projectId: {Projects[1].Id}");
        }

        [Fact]
        public async Task TestRequestedProject()
        {
            SetupData();
            Projects[1].IsActive = true;
            Projects[1].Status = Project.Statuses.Requested;
            MockData();

            Projects[1].IsActive.ShouldBeTrue();
            Projects[1].Status.ShouldBe(Project.Statuses.Requested);

            var invoiceServ = new InvoiceService(MockDbContext.Object, MockProjectHistoryService.Object, MockEmailService.Object,
                MockExpenseService.Object, MockDevSettings.Object);
            var rtValue = await invoiceServ.CreateInvoice(Projects[1].Id);
            rtValue.ShouldNotBeNull();
            rtValue.IsError.ShouldBeTrue();
            rtValue.Message.ShouldBe($"No active project found for given projectId: {Projects[1].Id}");
        }

        private void SetupData()
        {
            Projects = new List<Project>();
            for (int i = 0; i < 3; i++)
            {
                Projects.Add(CreateValidEntities.Project(i));
            }
            Projects[1].IsActive = false;

            Expenses = new List<Expense>();
            for (int i = 0; i < 2; i++)
            {
                Expenses.Add(CreateValidEntities.Expense(i, Projects[0].Id));
                Expenses.Add(CreateValidEntities.Expense(i, Projects[1].Id));
                Expenses.Add(CreateValidEntities.Expense(i, Projects[2].Id));
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
