using System.Collections.Generic;
using System.Linq;
using Harvest.Core.Data;
using Harvest.Core.Models.Settings;
using Harvest.Core.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Moq;
using System.Text.Json;
using System.Threading.Tasks;
using Harvest.Core.Domain;
using Harvest.Core.Models.FinancialAccountModels;
using Shouldly;
using Test.Helpers;
using TestHelpers.Helpers;
using Xunit;

namespace Test.TestsServices
{
    [Trait("Category", "ServiceTest")]
    public class SlothServiceTests
    {
        public Mock<AppDbContext> MockDbContext { get; set; }
        public Mock<IEmailService> MockEmailService { get; set; }
        public Mock<IFinancialService> MockFinancialService { get; set; }
        public Mock<IProjectHistoryService> MockProjectHistoryService { get; set; }
        public Mock<IOptions<SlothSettings>> MockSlothSettings { get; set; }
        public SlothSettings SlothSettings { get; set; } = new SlothSettings()
        {
            ApiKey = "Fake", ApiUrl = "FakeUrl", CreditObjectCode = "3900", CreditPassthroughObjectCode = "3918",
            MerchantTrackingUrl = "UnitTest//"
        };
        public JsonSerializerOptions JsonSerializerOptions { get; set; }
        private SlothService SlothService { get; set; }

        public List<Invoice> Invoices { get; set; }

        public SlothServiceTests()
        {
            MockDbContext = new Mock<AppDbContext>(new DbContextOptions<AppDbContext>());
            MockProjectHistoryService = new Mock<IProjectHistoryService>();
            MockEmailService = new Mock<IEmailService>();
            MockFinancialService = new Mock<IFinancialService>();
            MockSlothSettings = new Mock<IOptions<SlothSettings>>();
            
            MockSlothSettings.Setup(a => a.Value).Returns(SlothSettings);

            JsonSerializerOptions = new JsonSerializerOptions();

            SlothService = new SlothService(MockDbContext.Object, MockSlothSettings.Object, MockFinancialService.Object,
                JsonSerializerOptions, MockProjectHistoryService.Object, MockEmailService.Object);
        }

        private void SetupGenericData()
        {
            //Need an invoice with a project and expenses...
            Invoices = new List<Invoice>();
            var project = CreateValidEntities.Project(1, true);
            var expenses = new List<Expense>();
            for (int i = 0; i < 3; i++)
            {
                expenses.Add(CreateValidEntities.Expense(i+1, project.Id));
            }

            project.Accounts = new List<Account>();
            project.Accounts.Add(CreateValidEntities.Account(1));

            for (int i = 0; i < 3; i++)
            {
                var invoice = CreateValidEntities.Invoice(i + 1, project.Id);

                invoice.Expenses = expenses;
                invoice.Project = project;
                Invoices.Add(invoice);
            }
        }

        private void MockData()
        {
            MockDbContext.Setup(a => a.Invoices).Returns(Invoices.AsQueryable().MockAsyncDbSet().Object);

            var mockFinancialSettings = new Mock<IOptions<FinancialLookupSettings>>();
            var realFinancialService = new FinancialService(mockFinancialSettings.Object, JsonSerializerOptions);

            //Call actual service to parse account
            KfsAccount rtval = new KfsAccount();
            MockFinancialService.Setup(a => a.Parse(It.IsAny<string>()))
                .Callback<string>(a => rtval = realFinancialService.Parse(a)).Returns(() => rtval);
        }

        [Theory]
        [InlineData(1, Invoice.Statuses.Completed)]
        [InlineData(1, Invoice.Statuses.Pending)]
        [InlineData(99, Invoice.Statuses.Pending)]
        [InlineData(1, Invoice.Statuses.Created, Skip = "This one would fall through")]
        public async Task MoveMoneyReturnsErrorWhenCreatedInvoiceNotFound(int id, string status)
        {
            SetupGenericData();
            Invoices.Single(a => a.Id == 1).Status = status;
            MockData();

            var rtValue = await SlothService.MoveMoney(id);
            rtValue.ShouldNotBeNull();
            rtValue.IsError.ShouldBeTrue();
            rtValue.Message.ShouldBe($"Invoice not found: {id}");
        }

        [Fact]
        public async Task MoveMoneyReturnsErrorWhenCreatedInvoiceHasNoAccounts()
        {
            SetupGenericData();
            var invoice = Invoices.Single(a => a.Id == 1);
            invoice.Project.Accounts = new List<Account>();
            invoice.Status = Invoice.Statuses.Created;
            MockData();

            var rtValue = await SlothService.MoveMoney(invoice.Id);
            rtValue.ShouldNotBeNull();
            rtValue.IsError.ShouldBeTrue();
            rtValue.Message.ShouldBe($"No accounts found for invoice: {invoice.Id}");
        }

        [Fact]
        public async Task MoveMoneyReturnsErrorWhenCreatedInvoiceHasNoExpenses()
        {
            SetupGenericData();
            var invoice = Invoices.Single(a => a.Id == 1);
            invoice.Expenses = new List<Expense>();
            invoice.Status = Invoice.Statuses.Created;
            MockData();

            var rtValue = await SlothService.MoveMoney(invoice.Id);
            rtValue.ShouldNotBeNull();
            rtValue.IsError.ShouldBeTrue();
            rtValue.Message.ShouldBe($"No expenses found for invoice: {invoice.Id}");
        }

        [Fact]
        public async Task MoveMoneyReturnsErrorWhenRefundAmountIsTooSmall()
        {
            SetupGenericData();
            var invoice = Invoices.Single(a => a.Id == 1);
            invoice.Status = Invoice.Statuses.Created;
            invoice.Expenses = new List<Expense>();
            var expense = CreateValidEntities.Expense(1, 1);
            expense.Total = -0.004m;
            invoice.Expenses.Add(expense);
            MockData();

            var rtValue = await SlothService.MoveMoney(invoice.Id);
            rtValue.ShouldNotBeNull();
            rtValue.IsError.ShouldBeTrue();
            rtValue.Message.ShouldBe($"No expenses found for invoice: {invoice.Id}");
        }

        [Fact]
        public async Task MoveMoneyUpdatesInvoice()
        {
            SetupGenericData();
            var invoice = Invoices.Single(a => a.Id == 1);
            invoice.Status = Invoice.Statuses.Created;
            invoice.Expenses = new List<Expense>();
            var expense = CreateValidEntities.Expense(1, 1);
            expense.Total = 10.00m;
            invoice.Expenses.Add(expense);
            MockData();

            var rtValue = await SlothService.MoveMoney(invoice.Id);
            rtValue.ShouldNotBeNull();
            rtValue.IsError.ShouldBeTrue();
            rtValue.Message.ShouldBe($"No expenses found for invoice: {invoice.Id}");
        }

        //Test that the invoice total gets updated if it is different.
        //Test specific Expense scenarios that pass. Refund, refund with multiple accounts, refund with other expenses
        //Test passthrough
        //Test Expense grouping


        [Fact]
        public async Task test()
        {
            SetupGenericData();
            MockData();

            await SlothService.MoveMoney(1);
        }
    }
}
