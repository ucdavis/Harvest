using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using Harvest.Core.Data;
using Harvest.Core.Models.Settings;
using Harvest.Core.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Moq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Harvest.Core.Domain;
using Harvest.Core.Models.FinancialAccountModels;
using Harvest.Core.Utilities;
using Microsoft.AspNetCore.Http;
using Moq.Protected;
using Shouldly;
using Test.Helpers;
using TestHelpers.Helpers;
using Xunit;

namespace Test.TestsServices
{
    [Trait("Category", "ServiceTest")]
    public class SlothServiceTests
    {
        public Mock<HttpMessageHandler> MockMessageHandler { get; set; }
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
            MockMessageHandler = new Mock<HttpMessageHandler>(MockBehavior.Default);
            MockDbContext = new Mock<AppDbContext>(new DbContextOptions<AppDbContext>());
            MockProjectHistoryService = new Mock<IProjectHistoryService>();
            MockEmailService = new Mock<IEmailService>();
            MockFinancialService = new Mock<IFinancialService>();
            MockSlothSettings = new Mock<IOptions<SlothSettings>>();
            
            MockSlothSettings.Setup(a => a.Value).Returns(SlothSettings);

            JsonSerializerOptions = JsonOptions.Standard.WithStandard().WithGeoJson();

            MockMessageHandler.Protected().Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                // prepare the expected response of the mocked http call
                .ReturnsAsync(new HttpResponseMessage()
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent("{\r\n  \"id\": \"dfb39437-e604-4518-bc72-af89ea933684\",\r\n  \"status\": \"PendingApproval\",\r\n  \"sourceName\": \"Harvest Recharge\",\r\n  \"sourceType\": \"Recharge\",\r\n  \"merchantTrackingNumber\": \"1006\",\r\n  \"merchantTrackingUrl\": \"https://localhost:44308/Invoice/Details/6/1006\",\r\n  \"originCode\": \"CP\",\r\n  \"documentNumber\": \"000000254\",\r\n  \"documentType\": \"GLIB\",\r\n  \"kfsTrackingNumber\": \"0000000192\",\r\n  \"transactionDate\": \"2021-10-22T20:40:31.8700177Z\",\r\n  \"transfers\": [\r\n    {\r\n      \"id\": \"6dea5ce6-e2dc-4870-9ad9-08a7f47eb94a\",\r\n      \"amount\": 92.00,\r\n      \"chart\": \"3\",\r\n      \"account\": \"CRU9033\",\r\n      \"objectCode\": \"RAS5\",\r\n      \"description\": \"Proj: xxx Inv: 1006\",\r\n      \"direction\": \"Debit\",\r\n      \"fiscalYear\": 2022,\r\n      \"fiscalPeriod\": 4\r\n    },\r\n    {\r\n      \"id\": \"e56518c5-fb99-402d-ace7-4317d29aef94\",\r\n      \"amount\": 92.00,\r\n      \"chart\": \"3\",\r\n      \"account\": \"RRACRES\",\r\n      \"subAccount\": \"CNTRY\",\r\n      \"objectCode\": \"3900\",\r\n      \"description\": \"Proj: xxx Inv: 1006\",\r\n      \"direction\": \"Credit\",\r\n      \"fiscalYear\": 2022,\r\n      \"fiscalPeriod\": 4\r\n    }\r\n  ],\r\n  \"isReversal\": false,\r\n  \"hasReversal\": false,\r\n  \"statusEvents\": [\r\n    {\r\n      \"id\": \"3efaebae-a2de-4dc1-8be7-2cef5f9624fc\",\r\n      \"status\": \"PendingApproval\",\r\n      \"eventDate\": \"2021-10-22T20:40:45.5049293Z\",\r\n      \"eventDetails\": \"File: TransactionsController.cs, Member: Post, Line: 211\",\r\n      \"transactionId\": \"dfb39437-e604-4518-bc72-af89ea933684\"\r\n    }\r\n  ]\r\n}"), //Change to sloth content
                })
                .Verifiable();
            var httpClient = new HttpClient(MockMessageHandler.Object)
            {
                BaseAddress = new Uri("http://test.com/"),
            };

            SlothService = new SlothService(MockDbContext.Object, MockSlothSettings.Object, MockFinancialService.Object,
                JsonSerializerOptions, MockProjectHistoryService.Object, MockEmailService.Object, httpClient);
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


            var isValidRtn = new AccountValidationModel();
            MockFinancialService.Setup(a => a.IsValid(It.IsAny<string>()))
                .Callback<string>(a => rtval = realFinancialService.Parse(a)).Returns(async () => await SetValidAccount(rtval));

        }

        private Task<AccountValidationModel> SetValidAccount(KfsAccount kfsAccount)
        {
            var rtValue = new AccountValidationModel();
            rtValue.KfsAccount = kfsAccount;
            rtValue.IsValid = true;
            return Task.FromResult(rtValue);
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
        public async Task MoveMoneyReturnsErrorWhenAmountResultsInZeroTransfers()
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
            rtValue.Message.ShouldBe($"No Transfers Generated for invoice: {invoice.Id}");
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

            invoice.Transfers.ShouldBeNull();
            invoice.Status.ShouldBe(Invoice.Statuses.Created);

            var rtValue = await SlothService.MoveMoney(invoice.Id);
            rtValue.ShouldNotBeNull();
            rtValue.IsError.ShouldBeFalse();
            MockProjectHistoryService.Verify(a => a.MoveMoneyRequested(It.IsAny<int>(), It.IsAny<Invoice>()), times: Times.Once);
            MockDbContext.Verify(a => a.SaveChangesAsync(It.IsAny<CancellationToken>()), times: Times.Once);
            MockDbContext.Verify(a => a.SaveChanges(), times: Times.Never);

            invoice.Transfers.Count.ShouldBe(2);
            invoice.Status.ShouldBe(Invoice.Statuses.Pending);
            invoice.KfsTrackingNumber.ShouldBe("0000000192");

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
