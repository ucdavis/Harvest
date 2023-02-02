using System;
using Harvest.Core.Data;
using Harvest.Core.Domain;
using Harvest.Core.Models.FinancialAccountModels;
using Harvest.Core.Models.Settings;
using Harvest.Core.Services;
using Harvest.Core.Utilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Moq;
using Moq.Protected;
using Shouldly;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Harvest.Core.Models.SlothModels;
using Test.Helpers;
using TestHelpers.Helpers;
using Xunit;
using AggieEnterpriseApi.Validation;

namespace Test.TestsServices
{
    [Trait("Category", "ServiceTest")]
    public class AeSlothServiceTests
    {
        public Mock<HttpMessageHandler> MockMessageHandler { get; set; }
        public Mock<AppDbContext> MockDbContext { get; set; }
        public Mock<IEmailService> MockEmailService { get; set; }
        public Mock<IFinancialService> MockFinancialService { get; set; }
        public Mock<IProjectHistoryService> MockProjectHistoryService { get; set; }
        public Mock<IOptions<SlothSettings>> MockSlothSettings { get; set; }
        public Mock<IOptions<AggieEnterpriseOptions>> MockAeSettings { get; set; }

        public Mock<IAggieEnterpriseService> MockAeService { get; set; }
        public SlothSettings SlothSettings { get; set; } = new SlothSettings()
        {
            ApiKey = "Fake",
            ApiUrl = "http://sloth-fake.ucdavis.edu/",
            CreditObjectCode = "3900",
            CreditPassthroughObjectCode = "3918",
            MerchantTrackingUrl = "UnitTest//"
        };

        public AggieEnterpriseOptions AeOptions { get;set; } = new AggieEnterpriseOptions()
        {
            UseCoA = true,
            GraphQlUrl = "http://fake.ucdavis.edu/graphql",
            Token = "Fake",
            NormalCoaNaturalAccount ="770006",
            PassthroughCoaNaturalAccount = "770002"
        };
        public JsonSerializerOptions JsonSerializerOptions { get; set; }
        private SlothService SlothService { get; set; }

        public List<Invoice> Invoices { get; set; }

        public AeSlothServiceTests()
        {
            var httpClientFactory = BasicSetup(out var httpClient, HttpStatusCode.OK);

            SlothService = new SlothService(MockDbContext.Object, MockSlothSettings.Object, MockFinancialService.Object,
                JsonSerializerOptions, MockProjectHistoryService.Object, MockEmailService.Object, httpClientFactory.Object, MockAeSettings.Object, MockAeService.Object);
        }

        private Mock<IHttpClientFactory> BasicSetup(out HttpClient httpClient, HttpStatusCode statusCode, bool noContent = false, string status = "PendingApproval")
        {
            MockMessageHandler = new Mock<HttpMessageHandler>(MockBehavior.Default);
            MockDbContext = new Mock<AppDbContext>(new DbContextOptions<AppDbContext>());
            MockProjectHistoryService = new Mock<IProjectHistoryService>();
            MockEmailService = new Mock<IEmailService>();
            MockFinancialService = new Mock<IFinancialService>();
            MockSlothSettings = new Mock<IOptions<SlothSettings>>();
            MockAeSettings = new Mock<IOptions<AggieEnterpriseOptions>>();
            

            MockSlothSettings.Setup(a => a.Value).Returns(SlothSettings);
            MockAeSettings.Setup(a => a.Value).Returns(AeOptions);

            MockAeService = new Mock<IAggieEnterpriseService>();

            JsonSerializerOptions = JsonOptions.Standard.WithStandard().WithGeoJson();

            //Possible other values to replace: Id, KfsTrackingNumber
            var content =
                $"{{\r\n  \"id\": \"dfb39437-e604-4518-bc72-af89ea933684\",\r\n  \"status\": \"{status}\",\r\n  \"sourceName\": \"Harvest Recharge\",\r\n  \"sourceType\": \"Recharge\",\r\n  \"merchantTrackingNumber\": \"1006\",\r\n  \"merchantTrackingUrl\": \"https://localhost:44308/Invoice/Details/6/1006\",\r\n  \"originCode\": \"CP\",\r\n  \"documentNumber\": \"000000254\",\r\n  \"documentType\": \"GLIB\",\r\n  \"kfsTrackingNumber\": \"0000000192\",\r\n  \"transactionDate\": \"2021-10-22T20:40:31.8700177Z\",\r\n  \"transfers\": [\r\n    {{\r\n      \"id\": \"6dea5ce6-e2dc-4870-9ad9-08a7f47eb94a\",\r\n      \"amount\": 92.00,\r\n      \"chart\": \"3\",\r\n      \"account\": \"CRU9033\",\r\n      \"objectCode\": \"RAS5\",\r\n      \"description\": \"Proj: xxx Inv: 1006\",\r\n      \"direction\": \"Debit\",\r\n      \"fiscalYear\": 2022,\r\n      \"fiscalPeriod\": 4\r\n    }},\r\n    {{\r\n      \"id\": \"e56518c5-fb99-402d-ace7-4317d29aef94\",\r\n      \"amount\": 92.00,\r\n      \"chart\": \"3\",\r\n      \"account\": \"RRACRES\",\r\n      \"subAccount\": \"CNTRY\",\r\n      \"objectCode\": \"3900\",\r\n      \"description\": \"Proj: xxx Inv: 1006\",\r\n      \"direction\": \"Credit\",\r\n      \"fiscalYear\": 2022,\r\n      \"fiscalPeriod\": 4\r\n    }}\r\n  ],\r\n  \"isReversal\": false,\r\n  \"hasReversal\": false,\r\n  \"statusEvents\": [\r\n    {{\r\n      \"id\": \"3efaebae-a2de-4dc1-8be7-2cef5f9624fc\",\r\n      \"status\": \"PendingApproval\",\r\n      \"eventDate\": \"2021-10-22T20:40:45.5049293Z\",\r\n      \"eventDetails\": \"File: TransactionsController.cs, Member: Post, Line: 211\",\r\n      \"transactionId\": \"dfb39437-e604-4518-bc72-af89ea933684\"\r\n    }}\r\n  ]\r\n}}";

            MockMessageHandler.Protected().Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                // prepare the expected response of the mocked http call
                .ReturnsAsync(new HttpResponseMessage()
                {
                    StatusCode = statusCode,
                    Content = noContent ? null : new StringContent(content), //sloth content
                })
                .Verifiable();

            httpClient = new HttpClient(MockMessageHandler.Object);

            // create a moq of IHttpClientFactory to return a httpclient
            var httpClientFactory = new Mock<IHttpClientFactory>();
            httpClientFactory.Setup(a => a.CreateClient(It.IsAny<string>())).Returns(httpClient);
            return httpClientFactory;
        }

        private void SetupGenericData()
        {
            //Need an invoice with a project and expenses...
            Invoices = new List<Invoice>();
            var project = CreateValidEntities.Project(1, true);
            var expenses = new List<Expense>();
            for (int i = 0; i < 3; i++)
            {
                expenses.Add(CreateValidEntities.Expense(i + 1, project.Id, true));
            }

            project.Accounts = new List<Account>();
            project.Accounts.Add(CreateValidEntities.Account(1, acctNumber: $"KP0953010U-301001-ADNO001-{AeOptions.NormalCoaNaturalAccount}"));

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

            //Mock the Aggie Enterprise Service IsAccountValid
            MockAeService.Setup(a => a.IsAccountValid(It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>())).ReturnsAsync((string fss, bool a, bool b) => SetValidAccount(fss).Result);

            var realAeService = new AggieEnterpriseService(MockAeSettings.Object);

            //Call the real Aggie Enterprise Service ReplaceNaturalAccount
            MockAeService.Setup(a => a.ReplaceNaturalAccount(It.IsAny<string>(), It.IsAny<string>())).Returns((string fss, string na) => realAeService.ReplaceNaturalAccount(fss, na));

            MockAeService.Setup(a => a.ConvertKfsAccount("3-APSFB55")).ReturnsAsync("K30APSFB55-TASK01-APLS002-770006");
        }
        
        private Task<AccountValidationModel> SetValidAccount(string coa, bool setIsValid = true)
        {
            var rtValue = new AccountValidationModel();
            rtValue.KfsAccount = null;
            rtValue.FinancialSegmentString = coa;
            rtValue.CoaChartType = FinancialChartStringType.Ppm;
            rtValue.IsValid = setIsValid;
            if (!setIsValid)
            {
                rtValue.Messages.Add("Fake Message");                
            }
            return Task.FromResult(rtValue);
        }

        #region MoveMoney tests
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
            var expense = CreateValidEntities.Expense(1, 1, true);
            expense.Total = -0.004m;
            invoice.Expenses.Add(expense);
            MockData();

            var rtValue = await SlothService.MoveMoney(invoice.Id);
            rtValue.ShouldNotBeNull();
            rtValue.IsError.ShouldBeTrue();
            rtValue.Message.ShouldBe($"No Transfers Generated for invoice: {invoice.Id}");
        }

        [Fact]
        public async Task MoveMoneyReturnsErrorWhenDebitAccountInvalid()
        {
            SetupGenericData();
            var invoice = Invoices.Single(a => a.Id == 1);
            invoice.Status = Invoice.Statuses.Created;
            invoice.Expenses = new List<Expense>();
            var expense = CreateValidEntities.Expense(1, 1, true);
            expense.Total = 10.00m;
            invoice.Expenses.Add(expense);
            MockData();

            MockAeService.Setup(a => a.IsAccountValid(It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>())).ReturnsAsync(SetValidAccount("KP0953010U-301001-ADNO001-532303", false).Result);

            invoice.Transfers.ShouldBeNull();
            invoice.Status.ShouldBe(Invoice.Statuses.Created);

            var rtValue = await SlothService.MoveMoney(invoice.Id);
            rtValue.ShouldNotBeNull();
            rtValue.IsError.ShouldBeTrue();
            rtValue.Message.ShouldBe("Account KP0953010U-301001-ADNO001-770006 is not a valid Aggie Enterprise CoA");
        }

        [Fact]
        public async Task MoveMoneyRefundCreditAdjustmentWorks()
        {
            SetupGenericData();
            var invoice = Invoices.Single(a => a.Id == 1);
            invoice.Status = Invoice.Statuses.Created;
            invoice.Expenses = new List<Expense>();
            var expense = CreateValidEntities.Expense(1, 1, true);
            expense.Total = -1.00m;
            invoice.Expenses.Add(expense);
            invoice.Project.Accounts = new List<Account>();
            invoice.Project.Accounts.Add(CreateValidEntities.Account(1, $"KP0953010U-301001-ADNO001-{AeOptions.NormalCoaNaturalAccount}"));
            invoice.Project.Accounts.Add(CreateValidEntities.Account(2, $"KP0953010U-301001-ADNO002-{AeOptions.NormalCoaNaturalAccount}"));
            invoice.Project.Accounts.Add(CreateValidEntities.Account(3, $"KP0953010U-301001-ADNO003-{AeOptions.PassthroughCoaNaturalAccount}"));
            invoice.Project.Accounts.Add(CreateValidEntities.Account(4, $"KP0953010U-301001-ADNO004-{AeOptions.PassthroughCoaNaturalAccount}"));

            invoice.Project.Accounts[0].Percentage = 98m;
            invoice.Project.Accounts[1].Percentage = 1m;
            invoice.Project.Accounts[2].Percentage = 0.5m; //These last two have a zero amount so are ignored. The extra 1% gets added to the account above
            invoice.Project.Accounts[3].Percentage = 0.5m;

            MockData();

            invoice.Transfers.ShouldBeNull();
            invoice.Status.ShouldBe(Invoice.Statuses.Created);

            var rtValue = await SlothService.MoveMoney(invoice.Id);

            rtValue.ShouldNotBeNull();
            rtValue.IsError.ShouldBeFalse();
            MockProjectHistoryService.Verify(a => a.MoveMoneyRequested(It.IsAny<int>(), It.IsAny<Invoice>()), times: Times.Once);
            MockDbContext.Verify(a => a.SaveChangesAsync(It.IsAny<CancellationToken>()), times: Times.Once);
            MockDbContext.Verify(a => a.SaveChanges(), times: Times.Never);

            invoice.Transfers.ShouldNotBeNull();
            invoice.Transfers.Count.ShouldBe(3);
            invoice.Transfers.Count(a => a.IsProjectAccount).ShouldBe(2);
            invoice.Transfers.Single(a => a.Account == invoice.Project.Accounts[0].Number).Total.ShouldBe(0.98m);
            invoice.Transfers.Single(a => a.Account == invoice.Project.Accounts[1].Number).Total.ShouldBe(0.02m);
            invoice.Transfers.Single(a => a.Account == expense.Account).Total.ShouldBe(1m);
            invoice.Status.ShouldBe(Invoice.Statuses.Pending);
            invoice.KfsTrackingNumber.ShouldBe("0000000192");
        }

        [Fact]
        public async Task MoveMoneyUpdatesInvoice()
        {
            SetupGenericData();
            var invoice = Invoices.Single(a => a.Id == 1);
            invoice.Status = Invoice.Statuses.Created;
            invoice.Expenses = new List<Expense>();
            var expense = CreateValidEntities.Expense(1, 1, true);
            expense.Total = 10.00m;
            invoice.Expenses.Add(expense);
            MockData();

            invoice.Transfers.ShouldBeNull();
            invoice.Status.ShouldBe(Invoice.Statuses.Created);
            invoice.Project.ChargedTotal.ShouldBe(5000m);

            var rtValue = await SlothService.MoveMoney(invoice.Id);
            rtValue.ShouldNotBeNull();
            rtValue.IsError.ShouldBeFalse();
            MockProjectHistoryService.Verify(a => a.MoveMoneyRequested(It.IsAny<int>(), It.IsAny<Invoice>()), times: Times.Once);
            MockDbContext.Verify(a => a.SaveChangesAsync(It.IsAny<CancellationToken>()), times: Times.Once);
            MockDbContext.Verify(a => a.SaveChanges(), times: Times.Never);

            invoice.Transfers.Count.ShouldBe(2);
            invoice.Status.ShouldBe(Invoice.Statuses.Pending);
            invoice.KfsTrackingNumber.ShouldBe("0000000192");
            invoice.Project.ChargedTotal.ShouldBe(5010m);

        }

        //Test that the invoice total gets updated if it is different.
        [Fact]
        public async Task MoveMoneyUpdatesProjectTotalAndFixesInvoiceTotal()
        {
            SetupGenericData();
            var invoice = Invoices.Single(a => a.Id == 1);
            invoice.Status = Invoice.Statuses.Created;
            invoice.Expenses = new List<Expense>();
            var expense = CreateValidEntities.Expense(1, 1, true);
            expense.Total = 990.00m;
            invoice.Expenses.Add(expense);
            invoice.Project.ChargedTotal = 5.00m;
            invoice.Total = 1.00m;
            MockData();

            invoice.Transfers.ShouldBeNull();
            invoice.Status.ShouldBe(Invoice.Statuses.Created);

            var rtValue = await SlothService.MoveMoney(invoice.Id);
            rtValue.ShouldNotBeNull();
            rtValue.IsError.ShouldBeFalse();
            MockProjectHistoryService.Verify(a => a.MoveMoneyRequested(It.IsAny<int>(), It.IsAny<Invoice>()), times: Times.Once);
            MockDbContext.Verify(a => a.SaveChangesAsync(It.IsAny<CancellationToken>()), times: Times.Once);
            MockDbContext.Verify(a => a.SaveChanges(), times: Times.Never);

            invoice.Transfers.ShouldNotBeNull();
            invoice.Transfers.Count.ShouldBe(2);
            invoice.Project.ChargedTotal.ShouldBe(995.00m);
            invoice.Total.ShouldBe(990.00m);
            invoice.Status.ShouldBe(Invoice.Statuses.Pending);
            invoice.KfsTrackingNumber.ShouldBe("0000000192");

        }

        //Test when expense has a pass through account and a normal account
        [Fact]
        public async Task MoveMoneyWhenPassThrough()
        {
            SetupGenericData();
            var invoice = Invoices.Single(a => a.Id == 1);
            invoice.Status = Invoice.Statuses.Created;
            invoice.Expenses = new List<Expense>();
            var expense = CreateValidEntities.Expense(1, 1);
            expense.Account = "3110-13U20-ADNO003-775001-64-000-0000000000-000000-0000-000000-000000";
            expense.Total = 990.00m;
            expense.IsPassthrough = true;
            invoice.Expenses.Add(expense);

            expense = CreateValidEntities.Expense(1, 1);
            expense.Account = "3110-13U20-ADNO003-775001-64-000-0000000000-000000-0000-000000-000000";
            expense.Total = 100.00m;
            expense.IsPassthrough = true;
            invoice.Expenses.Add(expense);

            expense = CreateValidEntities.Expense(1, 1);
            expense.Account = "3110-13U20-ADNO002-775001-64-000-0000000000-000000-0000-000000-000000";
            expense.Total = 50.00m;
            expense.IsPassthrough = true;
            invoice.Expenses.Add(expense);

            expense = CreateValidEntities.Expense(1, 1); //Same as first expense except not passthrough and object code different (For KFS this was true, but for AE it will have a different account
            expense.Account = "3110-13U20-ADNO003-410003-64-000-0000000000-000000-0000-000000-000000";
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

            invoice.Transfers.ShouldNotBeNull();
            invoice.Transfers.Count.ShouldBe(5);
            invoice.Transfers.Count(a => a.IsProjectAccount).ShouldBe(2);
            invoice.Project.ChargedTotal.ShouldBe(6150.00m);
            invoice.Total.ShouldBe(1150.00m);
            invoice.Status.ShouldBe(Invoice.Statuses.Pending);
            invoice.KfsTrackingNumber.ShouldBe("0000000192");

        }

        [Fact]
        public async Task MoveMoneyReturnsStatusCodeNotFound()
        {
            var httpClientFactory = BasicSetup(out var httpClient, HttpStatusCode.NotFound);

            var slothService = new SlothService(MockDbContext.Object, MockSlothSettings.Object, MockFinancialService.Object,
                JsonSerializerOptions, MockProjectHistoryService.Object, MockEmailService.Object, httpClientFactory.Object, MockAeSettings.Object, MockAeService.Object);

            SetupGenericData();
            var invoice = Invoices.Single(a => a.Id == 1);
            invoice.Status = Invoice.Statuses.Created;
            invoice.Expenses = new List<Expense>();
            var expense = CreateValidEntities.Expense(1, 1, true);
            expense.Total = 10.00m;
            invoice.Expenses.Add(expense);
            MockData();

            invoice.Transfers.ShouldBeNull();
            invoice.Status.ShouldBe(Invoice.Statuses.Created);

            var rtValue = await slothService.MoveMoney(invoice.Id);
            rtValue.ShouldNotBeNull();
            rtValue.IsError.ShouldBeTrue();
            rtValue.Message.ShouldStartWith("Sloth Response didn't have a success code for moneyTransfer id 1: {\r\n  \"id\":");
            MockProjectHistoryService.Verify(a => a.MoveMoneyRequested(It.IsAny<int>(), It.IsAny<Invoice>()), times: Times.Never);
            MockDbContext.Verify(a => a.SaveChangesAsync(It.IsAny<CancellationToken>()), times: Times.Never);
            MockDbContext.Verify(a => a.SaveChanges(), times: Times.Never);

            invoice.Transfers.ShouldBeNull();
            invoice.Status.ShouldBe(Invoice.Statuses.Created);
            invoice.KfsTrackingNumber.ShouldBeNull();
        }

        [Fact]
        public async Task MoveMoneyReturnsStatusCodeNoContent()
        {
            var httpClientFactory = BasicSetup(out var httpClient, HttpStatusCode.NoContent, true);

            var slothService = new SlothService(MockDbContext.Object, MockSlothSettings.Object, MockFinancialService.Object,
                JsonSerializerOptions, MockProjectHistoryService.Object, MockEmailService.Object, httpClientFactory.Object, MockAeSettings.Object, MockAeService.Object);

            SetupGenericData();
            var invoice = Invoices.Single(a => a.Id == 1);
            invoice.Status = Invoice.Statuses.Created;
            invoice.Expenses = new List<Expense>();
            var expense = CreateValidEntities.Expense(1, 1, true);
            expense.Total = 10.00m;
            invoice.Expenses.Add(expense);
            MockData();

            invoice.Transfers.ShouldBeNull();
            invoice.Status.ShouldBe(Invoice.Statuses.Created);

            var rtValue = await slothService.MoveMoney(invoice.Id);
            rtValue.ShouldNotBeNull();
            rtValue.IsError.ShouldBeTrue();
            rtValue.Message.ShouldBe("Sloth Response didn't have a success code for moneyTransfer id 1: ");
            MockProjectHistoryService.Verify(a => a.MoveMoneyRequested(It.IsAny<int>(), It.IsAny<Invoice>()), times: Times.Never);
            MockDbContext.Verify(a => a.SaveChangesAsync(It.IsAny<CancellationToken>()), times: Times.Never);
            MockDbContext.Verify(a => a.SaveChanges(), times: Times.Never);

            invoice.Transfers.ShouldBeNull();
            invoice.Status.ShouldBe(Invoice.Statuses.Created);
            invoice.KfsTrackingNumber.ShouldBeNull();
        }

        [Fact]
        public async Task MoveMoneyReturnsStatusCodeBadRequest()
        {
            var httpClientFactory = BasicSetup(out var httpClient, HttpStatusCode.BadRequest);

            var slothService = new SlothService(MockDbContext.Object, MockSlothSettings.Object, MockFinancialService.Object,
                JsonSerializerOptions, MockProjectHistoryService.Object, MockEmailService.Object, httpClientFactory.Object, MockAeSettings.Object, MockAeService.Object);

            SetupGenericData();
            var invoice = Invoices.Single(a => a.Id == 1);
            invoice.Status = Invoice.Statuses.Created;
            invoice.Expenses = new List<Expense>();
            var expense = CreateValidEntities.Expense(1, 1, true);
            expense.Total = 10.00m;
            invoice.Expenses.Add(expense);
            MockData();

            invoice.Transfers.ShouldBeNull();
            invoice.Status.ShouldBe(Invoice.Statuses.Created);

            var rtValue = await slothService.MoveMoney(invoice.Id);
            rtValue.ShouldNotBeNull();
            rtValue.IsError.ShouldBeTrue();
            rtValue.Message.ShouldStartWith("Sloth Response Bad Request for moneyTransfer id 1: {\r\n  \"id\":");
            MockProjectHistoryService.Verify(a => a.MoveMoneyRequested(It.IsAny<int>(), It.IsAny<Invoice>()), times: Times.Never);
            MockDbContext.Verify(a => a.SaveChangesAsync(It.IsAny<CancellationToken>()), times: Times.Never);
            MockDbContext.Verify(a => a.SaveChanges(), times: Times.Never);

            invoice.Transfers.ShouldBeNull();
            invoice.Status.ShouldBe(Invoice.Statuses.Created);
            invoice.KfsTrackingNumber.ShouldBeNull();
        }

        //Test specific Expense scenarios that pass. Refund, refund with multiple accounts, refund with other expenses
        [Fact]
        public async Task MoveMoneyUpdatesWhenThereIsARefund()
        {
            SetupGenericData();
            var invoice = Invoices.Single(a => a.Id == 1);
            invoice.Status = Invoice.Statuses.Created;
            invoice.Expenses = new List<Expense>();
            var expense = CreateValidEntities.Expense(1, 1, true);
            expense.Total = -10.00m;
            invoice.Expenses.Add(expense);
            MockData();

            invoice.Transfers.ShouldBeNull();
            invoice.Status.ShouldBe(Invoice.Statuses.Created);
            invoice.Project.ChargedTotal.ShouldBe(5000m);

            var rtValue = await SlothService.MoveMoney(invoice.Id);
            rtValue.ShouldNotBeNull();
            rtValue.IsError.ShouldBeFalse();
            MockProjectHistoryService.Verify(a => a.MoveMoneyRequested(It.IsAny<int>(), It.IsAny<Invoice>()), times: Times.Once);
            MockDbContext.Verify(a => a.SaveChangesAsync(It.IsAny<CancellationToken>()), times: Times.Once);
            MockDbContext.Verify(a => a.SaveChanges(), times: Times.Never);

            invoice.Transfers.Count.ShouldBe(2);
            invoice.Transfers.ShouldAllBe(a => a.Total == 10);
            invoice.Transfers.Single(a => a.IsProjectAccount).Type.ShouldBe("Credit");
            invoice.Transfers.Single(a => !a.IsProjectAccount).Type.ShouldBe("Debit");
            invoice.Status.ShouldBe(Invoice.Statuses.Pending);
            invoice.KfsTrackingNumber.ShouldBe("0000000192");
            invoice.Project.ChargedTotal.ShouldBe(4990m); //It subtracted the $10 refund.
        }

        [Fact]
        public async Task MoveMoneyUpdatesWhenThereIsArefundWithTwoPiAccounts()
        {
            SetupGenericData();
            var invoice = Invoices.Single(a => a.Id == 1);
            invoice.Status = Invoice.Statuses.Created;
            invoice.Expenses = new List<Expense>();
            var expense = CreateValidEntities.Expense(1, 1);
            expense.Total = -10.00m;
            invoice.Expenses.Add(expense);
            invoice.Project.Accounts.Add(CreateValidEntities.Account(77, acctNumber: $"KP0953010U-301077-ADNO001-{AeOptions.NormalCoaNaturalAccount}"));
            invoice.Project.Accounts[0].Percentage = 75m;
            invoice.Project.Accounts[1].Percentage = 25m;
            MockData();

            invoice.Transfers.ShouldBeNull();
            invoice.Status.ShouldBe(Invoice.Statuses.Created);
            invoice.Project.ChargedTotal.ShouldBe(5000m);

            var rtValue = await SlothService.MoveMoney(invoice.Id);
            rtValue.ShouldNotBeNull();
            rtValue.IsError.ShouldBeFalse();
            MockProjectHistoryService.Verify(a => a.MoveMoneyRequested(It.IsAny<int>(), It.IsAny<Invoice>()), times: Times.Once);
            MockDbContext.Verify(a => a.SaveChangesAsync(It.IsAny<CancellationToken>()), times: Times.Once);
            MockDbContext.Verify(a => a.SaveChanges(), times: Times.Never);

            invoice.Transfers.Count.ShouldBe(3);
            //invoice.Transfers.ShouldAllBe(a => a.Total == 10);
            invoice.Transfers.Single(a => a.IsProjectAccount && a.Account == invoice.Project.Accounts[0].Number).Type.ShouldBe("Credit");
            invoice.Transfers.Single(a => a.IsProjectAccount && a.Account == invoice.Project.Accounts[0].Number).Total.ShouldBe(7.5m);
            invoice.Transfers.Single(a => a.IsProjectAccount && a.Account == invoice.Project.Accounts[1].Number).Total.ShouldBe(2.5m);
            invoice.Transfers.Single(a => !a.IsProjectAccount).Type.ShouldBe("Debit");
            invoice.Transfers.Single(a => !a.IsProjectAccount).Total.ShouldBe(10m);
            invoice.Status.ShouldBe(Invoice.Statuses.Pending);
            invoice.KfsTrackingNumber.ShouldBe("0000000192");
            invoice.Project.ChargedTotal.ShouldBe(4990m); //It subtracted the $10 refund.
        }

        [Fact]
        public async Task MoveMoneyUpdatesWhenThereIsAValidKfsProjectAccount()
        {
            SetupGenericData();
            var invoice = Invoices.Single(a => a.Id == 1);
            invoice.Status = Invoice.Statuses.Created;
            invoice.Expenses = new List<Expense>();
            var expense = CreateValidEntities.Expense(1, 1);
            expense.Total = -10.00m;
            invoice.Expenses.Add(expense);
            invoice.Project.Accounts.Add(CreateValidEntities.Account(77, acctNumber: "3-APSFB55")); //This gets converted to K30APSFB55-TASK01-APLS002-770006 with the mock, but the service would do the same thing.
            invoice.Project.Accounts[0].Percentage = 75m;
            invoice.Project.Accounts[1].Percentage = 25m;
            MockData();

            invoice.Transfers.ShouldBeNull();
            invoice.Status.ShouldBe(Invoice.Statuses.Created);
            invoice.Project.ChargedTotal.ShouldBe(5000m);

            var rtValue = await SlothService.MoveMoney(invoice.Id);
            rtValue.ShouldNotBeNull();
            rtValue.IsError.ShouldBeFalse();
            MockProjectHistoryService.Verify(a => a.MoveMoneyRequested(It.IsAny<int>(), It.IsAny<Invoice>()), times: Times.Once);
            MockDbContext.Verify(a => a.SaveChangesAsync(It.IsAny<CancellationToken>()), times: Times.Once);
            MockDbContext.Verify(a => a.SaveChanges(), times: Times.Never);

            invoice.Transfers.Count.ShouldBe(3);
            //invoice.Transfers.ShouldAllBe(a => a.Total == 10);
            invoice.Transfers.Single(a => a.IsProjectAccount && a.Account == invoice.Project.Accounts[0].Number).Type.ShouldBe("Credit");
            invoice.Transfers.Single(a => a.IsProjectAccount && a.Account == invoice.Project.Accounts[0].Number).Total.ShouldBe(7.5m);
            invoice.Transfers.Single(a => a.IsProjectAccount && a.Account == invoice.Project.Accounts[1].Number).Total.ShouldBe(2.5m);
            invoice.Transfers.Single(a => !a.IsProjectAccount).Type.ShouldBe("Debit");
            invoice.Transfers.Single(a => !a.IsProjectAccount).Total.ShouldBe(10m);
            invoice.Status.ShouldBe(Invoice.Statuses.Pending);
            invoice.KfsTrackingNumber.ShouldBe("0000000192");
            invoice.Project.ChargedTotal.ShouldBe(4990m); //It subtracted the $10 refund.
        }

        [Fact]
        public async Task MoveMoneyUpdatesWhenThereIsArefundWithTwoPiAccountsAndTwoRefundAccounts()
        {
            SetupGenericData();
            var invoice = Invoices.Single(a => a.Id == 1);
            invoice.Status = Invoice.Statuses.Created;
            invoice.Expenses = new List<Expense>();
            var expense = CreateValidEntities.Expense(1, 1, true);
            expense.Total = -10.00m;
            invoice.Expenses.Add(expense);
            invoice.Project.Accounts.Add(CreateValidEntities.Account(77, acctNumber: $"KP0953010U-301077-ADNO001-{AeOptions.NormalCoaNaturalAccount}"));
            invoice.Project.Accounts[0].Percentage = 75m;
            invoice.Project.Accounts[1].Percentage = 25m;

            expense = CreateValidEntities.Expense(2, 1, true);
            expense.Total = -20m;
            expense.Account = "3110-13U20-ADNO004-410003-64-000-0000000000-000000-0000-000000-000000"; //Diff from the one defaulted in the first expense
            invoice.Expenses.Add(expense);
            MockData();

            invoice.Transfers.ShouldBeNull();
            invoice.Status.ShouldBe(Invoice.Statuses.Created);
            invoice.Project.ChargedTotal.ShouldBe(5000m);

            var rtValue = await SlothService.MoveMoney(invoice.Id);
            rtValue.ShouldNotBeNull();
            rtValue.IsError.ShouldBeFalse();
            MockProjectHistoryService.Verify(a => a.MoveMoneyRequested(It.IsAny<int>(), It.IsAny<Invoice>()), times: Times.Once);
            MockDbContext.Verify(a => a.SaveChangesAsync(It.IsAny<CancellationToken>()), times: Times.Once);
            MockDbContext.Verify(a => a.SaveChanges(), times: Times.Never);

            invoice.Transfers.Count.ShouldBe(4);
            //2 for the project accounts and 2 for the expense accounts. This is different from the KFS way, but is correct
            var creditAccounts1 = invoice.Transfers
                .Where(a => a.IsProjectAccount && a.Account.StartsWith(invoice.Project.Accounts[0].Number.Substring(0,20))).ToArray();
            var creditAccounts2 = invoice.Transfers
                .Where(a => a.IsProjectAccount && a.Account.StartsWith(invoice.Project.Accounts[1].Number.Substring(0, 20))).ToArray();
            creditAccounts1.Length.ShouldBe(1);
            creditAccounts1.ShouldAllBe(a => a.Type == "Credit");
            creditAccounts1[0].Total.ShouldBe(22.5m);

            creditAccounts2.Length.ShouldBe(1);
            creditAccounts2.ShouldAllBe(a => a.Type == "Credit");
            creditAccounts2[0].Total.ShouldBe(7.5m);

            var debitAccounts = invoice.Transfers.Where(a => !a.IsProjectAccount).ToArray();
            debitAccounts.Length.ShouldBe(2);
            debitAccounts.ShouldAllBe(a => a.Type == "Debit");
            debitAccounts[0].Total.ShouldBe(10m);
            debitAccounts[0].Account.ShouldBe("3110-13U20-ADNO003-410003-64-000-0000000000-000000-0000-000000-000000");
            debitAccounts[1].Total.ShouldBe(20m);
            debitAccounts[1].Account.ShouldBe("3110-13U20-ADNO004-410003-64-000-0000000000-000000-0000-000000-000000");

            invoice.Status.ShouldBe(Invoice.Statuses.Pending);
            invoice.KfsTrackingNumber.ShouldBe("0000000192");
            invoice.Project.ChargedTotal.ShouldBe(4970m);
        }

        [Fact]
        public async Task MoveMoneyUpdatesWhenThereIsArefundWithTwoPiAccountsAndTwoRefundAccountsAndANormalExpense()
        {
            SetupGenericData();
            var invoice = Invoices.Single(a => a.Id == 1);
            invoice.Status = Invoice.Statuses.Created;
            invoice.Expenses = new List<Expense>();
            var expense = CreateValidEntities.Expense(1, 1, true);
            expense.Total = -10.00m;
            invoice.Expenses.Add(expense);
            invoice.Project.Accounts.Add(CreateValidEntities.Account(77, acctNumber: $"KP0953010U-301077-ADNO001-{AeOptions.NormalCoaNaturalAccount}"));
            invoice.Project.Accounts[0].Percentage = 75m;
            invoice.Project.Accounts[1].Percentage = 25m;

            expense = CreateValidEntities.Expense(2, 1);
            expense.Total = -20m;
            expense.Account = "3110-13U20-ADNO005-410003-64-000-0000000000-000000-0000-000000-000000";
            invoice.Expenses.Add(expense);

            expense = CreateValidEntities.Expense(3, 1);
            expense.Total = 100m;
            expense.Account = "3110-13U20-ADNO006-410003-64-000-0000000000-000000-0000-000000-000000";
            invoice.Expenses.Add(expense);
            MockData();

            invoice.Transfers.ShouldBeNull();
            invoice.Status.ShouldBe(Invoice.Statuses.Created);
            invoice.Project.ChargedTotal.ShouldBe(5000m);

            var rtValue = await SlothService.MoveMoney(invoice.Id);
            rtValue.ShouldNotBeNull();
            rtValue.IsError.ShouldBeFalse();
            MockProjectHistoryService.Verify(a => a.MoveMoneyRequested(It.IsAny<int>(), It.IsAny<Invoice>()), times: Times.Once);
            MockDbContext.Verify(a => a.SaveChangesAsync(It.IsAny<CancellationToken>()), times: Times.Once);
            MockDbContext.Verify(a => a.SaveChanges(), times: Times.Never);

            invoice.Transfers.Count.ShouldBe(7);

            var creditPiAccounts1 = invoice.Transfers
                .Where(a => a.IsProjectAccount && a.Type == "Credit" && a.Account.StartsWith(invoice.Project.Accounts[0].Number.Substring(0, 20))).ToArray();
            var creditPiAccounts2 = invoice.Transfers
                .Where(a => a.IsProjectAccount && a.Type == "Credit" && a.Account.StartsWith(invoice.Project.Accounts[1].Number.Substring(0, 20))).ToArray();

            var debitPiAccounts1 = invoice.Transfers
                .Where(a => a.IsProjectAccount && a.Type == "Debit").ToArray();

            creditPiAccounts1.Length.ShouldBe(1);
            creditPiAccounts1[0].Total.ShouldBe(22.5m);

            creditPiAccounts2.Length.ShouldBe(1);
            creditPiAccounts2[0].Total.ShouldBe(7.5m);

            debitPiAccounts1.Length.ShouldBe(2);
            debitPiAccounts1[0].Total.ShouldBe(75m);
            debitPiAccounts1[0].Account.ShouldStartWith(invoice.Project.Accounts[0].Number.Substring(0,21));
            debitPiAccounts1[1].Total.ShouldBe(25m);
            debitPiAccounts1[1].Account.ShouldStartWith(invoice.Project.Accounts[1].Number.Substring(0,21));

            var debitHarvestAccounts = invoice.Transfers.Where(a => !a.IsProjectAccount && a.Type == "Debit").ToArray();
            debitHarvestAccounts.Length.ShouldBe(2);
            debitHarvestAccounts[0].Total.ShouldBe(10m);
            debitHarvestAccounts[0].Account.ShouldBe("3110-13U20-ADNO003-410003-64-000-0000000000-000000-0000-000000-000000");
            debitHarvestAccounts[1].Total.ShouldBe(20m);
            debitHarvestAccounts[1].Account.ShouldBe("3110-13U20-ADNO005-410003-64-000-0000000000-000000-0000-000000-000000");

            var creditHarvestAccounts = invoice.Transfers.Where(a => !a.IsProjectAccount && a.Type == "Credit").ToArray();
            creditHarvestAccounts.Length.ShouldBe(1);
            creditHarvestAccounts[0].Total.ShouldBe(100m);
            creditHarvestAccounts[0].Account.ShouldBe("3110-13U20-ADNO006-410003-64-000-0000000000-000000-0000-000000-000000");


            invoice.Status.ShouldBe(Invoice.Statuses.Pending);
            invoice.KfsTrackingNumber.ShouldBe("0000000192");
            invoice.Project.ChargedTotal.ShouldBe(5070m);
        }

        //Test passthrough
        [Fact]
        public async Task MoveMoneyUpdatesWhenThereArePassthroughAccountsWithTwoPiAccounts()
        {
            SetupGenericData();
            var invoice = Invoices.Single(a => a.Id == 1);
            invoice.Status = Invoice.Statuses.Created;
            invoice.Expenses = new List<Expense>();
            var expense = CreateValidEntities.Expense(1, 1);
            expense.Total = 10.00m;
            expense.IsPassthrough = true;
            expense.Account = "3110-13U20-ADNO003-775001-64-000-0000000000-000000-0000-000000-000000";
            invoice.Expenses.Add(expense);

            expense = CreateValidEntities.Expense(2, 1);
            expense.Total = 20.00m;
            expense.IsPassthrough = true;
            expense.Account = "3110-13U20-ADNO004-775001-64-000-0000000000-000000-0000-000000-000000";
            invoice.Expenses.Add(expense);

            //expense = CreateValidEntities.Expense(4, 1); //Next test will add this expense, and with grouping, total transfers remains the same
            //expense.Total = 100.00m;
            //expense.IsPassthrough = true;
            //expense.Account = "3-APSNFLP--80RS";
            //invoice.Expenses.Add(expense);

            expense = CreateValidEntities.Expense(3, 1);
            expense.Total = 5.00m;
            expense.IsPassthrough = true;
            expense.Account = "3110-13U20-ADNO005-775001-64-000-0000000000-000000-0000-000000-000000";
            invoice.Expenses.Add(expense);

            invoice.Project.Accounts.Add(CreateValidEntities.Account(77, acctNumber: $"KP0953010U-301077-ADNO001-{AeOptions.NormalCoaNaturalAccount}"));
            invoice.Project.Accounts[0].Percentage = 75m;
            invoice.Project.Accounts[1].Percentage = 25m;
            MockData();

            invoice.Transfers.ShouldBeNull();
            invoice.Status.ShouldBe(Invoice.Statuses.Created);
            invoice.Project.ChargedTotal.ShouldBe(5000m);

            var rtValue = await SlothService.MoveMoney(invoice.Id);
            rtValue.ShouldNotBeNull();
            rtValue.IsError.ShouldBeFalse();
            MockProjectHistoryService.Verify(a => a.MoveMoneyRequested(It.IsAny<int>(), It.IsAny<Invoice>()), times: Times.Once);
            MockDbContext.Verify(a => a.SaveChangesAsync(It.IsAny<CancellationToken>()), times: Times.Once);
            MockDbContext.Verify(a => a.SaveChanges(), times: Times.Never);

            invoice.Transfers.Count.ShouldBe(5);

            var piAccounts = invoice.Transfers.Where(a => a.IsProjectAccount).ToArray();
            piAccounts.Length.ShouldBe(2);
            piAccounts.ShouldAllBe(a => a.Type == "Debit");
            piAccounts[0].Total.ShouldBe(26.25m);
            piAccounts[0].Account.ShouldBe("KP0953010U-301001-ADNO001-770002");

            piAccounts[1].Total.ShouldBe(8.75m);
            piAccounts[1].Account.ShouldBe("KP0953010U-301077-ADNO001-770002");

            var harvestAccounts = invoice.Transfers.Where(a => !a.IsProjectAccount).ToArray();
            harvestAccounts.Length.ShouldBe(3);
            harvestAccounts.ShouldAllBe(a => a.Type == "Credit");

            harvestAccounts[0].Total.ShouldBe(10m);
            harvestAccounts[0].Account.ShouldBe("3110-13U20-ADNO003-775001-64-000-0000000000-000000-0000-000000-000000");

            harvestAccounts[1].Total.ShouldBe(20m);
            harvestAccounts[1].Account.ShouldBe("3110-13U20-ADNO004-775001-64-000-0000000000-000000-0000-000000-000000");

            harvestAccounts[2].Total.ShouldBe(5m);
            harvestAccounts[2].Account.ShouldBe("3110-13U20-ADNO005-775001-64-000-0000000000-000000-0000-000000-000000");

            invoice.Status.ShouldBe(Invoice.Statuses.Pending);
            invoice.KfsTrackingNumber.ShouldBe("0000000192");
            invoice.Project.ChargedTotal.ShouldBe(5035m);
        }

        [Fact]
        public async Task MoveMoneyUpdatesWhenThereAre3PassthroughAccountsWithTwoPiAccounts()
        {
            SetupGenericData();
            var invoice = Invoices.Single(a => a.Id == 1);
            invoice.Status = Invoice.Statuses.Created;
            invoice.Expenses = new List<Expense>();
            var expense = CreateValidEntities.Expense(1, 1);
            expense.Total = 10.00m;
            expense.IsPassthrough = true;
            expense.Account = "3110-13U20-ADNO003-775001-64-000-0000000000-000000-0000-000000-000000";
            invoice.Expenses.Add(expense);

            expense = CreateValidEntities.Expense(2, 1);
            expense.Total = 20.00m;
            expense.IsPassthrough = true;
            expense.Account = "3110-13U20-ADNO004-775001-64-000-0000000000-000000-0000-000000-000000";
            invoice.Expenses.Add(expense);

            expense = CreateValidEntities.Expense(4, 1); //Test above didn't have this one. Shows grouping
            expense.Total = 100.00m;
            expense.IsPassthrough = true;
            expense.Account = "3110-13U20-ADNO004-775001-64-000-0000000000-000000-0000-000000-000000";
            invoice.Expenses.Add(expense);

            expense = CreateValidEntities.Expense(3, 1);
            expense.Total = 5.00m;
            expense.IsPassthrough = true;
            expense.Account = "3110-13U20-ADNO005-775001-64-000-0000000000-000000-0000-000000-000000";
            invoice.Expenses.Add(expense);

            invoice.Project.Accounts.Add(CreateValidEntities.Account(77, acctNumber: "KP0953010U-301077-ADNO001-770002"));
            invoice.Project.Accounts[0].Percentage = 75m;
            invoice.Project.Accounts[1].Percentage = 25m;
            MockData();

            invoice.Transfers.ShouldBeNull();
            invoice.Status.ShouldBe(Invoice.Statuses.Created);
            invoice.Project.ChargedTotal.ShouldBe(5000m);

            var rtValue = await SlothService.MoveMoney(invoice.Id);
            rtValue.ShouldNotBeNull();
            rtValue.IsError.ShouldBeFalse();
            MockProjectHistoryService.Verify(a => a.MoveMoneyRequested(It.IsAny<int>(), It.IsAny<Invoice>()), times: Times.Once);
            MockDbContext.Verify(a => a.SaveChangesAsync(It.IsAny<CancellationToken>()), times: Times.Once);
            MockDbContext.Verify(a => a.SaveChanges(), times: Times.Never);

            invoice.Transfers.Count.ShouldBe(5);

            var piAccounts = invoice.Transfers.Where(a => a.IsProjectAccount).ToArray();
            piAccounts.Length.ShouldBe(2);
            piAccounts.ShouldAllBe(a => a.Type == "Debit");
            piAccounts[0].Total.ShouldBe(101.25m);
            piAccounts[0].Account.ShouldBe("KP0953010U-301001-ADNO001-770002");

            piAccounts[1].Total.ShouldBe(33.75m);
            piAccounts[1].Account.ShouldBe("KP0953010U-301077-ADNO001-770002");


            var harvestAccounts = invoice.Transfers.Where(a => !a.IsProjectAccount).ToArray();
            harvestAccounts.Length.ShouldBe(3);
            harvestAccounts.ShouldAllBe(a => a.Type == "Credit");

            harvestAccounts[0].Total.ShouldBe(10m);
            harvestAccounts[0].Account.ShouldBe("3110-13U20-ADNO003-775001-64-000-0000000000-000000-0000-000000-000000");

            harvestAccounts[1].Total.ShouldBe(120m);
            harvestAccounts[1].Account.ShouldBe("3110-13U20-ADNO004-775001-64-000-0000000000-000000-0000-000000-000000");

            harvestAccounts[2].Total.ShouldBe(5m);
            harvestAccounts[2].Account.ShouldBe("3110-13U20-ADNO005-775001-64-000-0000000000-000000-0000-000000-000000");

            invoice.Status.ShouldBe(Invoice.Statuses.Pending);
            invoice.KfsTrackingNumber.ShouldBe("0000000192");
            invoice.Project.ChargedTotal.ShouldBe(5135m);
        }

        #endregion MoveMoney tests

        #region ProcessTransferUpdates tests

        [Theory]
        [InlineData(Invoice.Statuses.Completed)]
        [InlineData(Invoice.Statuses.Created)]
        public async Task ProcessTransferUpdatesReturnsEarlyIfThereAreNoPendingInvoices(string status)
        {
            SetupGenericData();
            foreach (var invoice in Invoices)
            {
                invoice.Status = status;
            }
            MockData();

            await SlothService.ProcessTransferUpdates();
            MockDbContext.Verify(a => a.SaveChangesAsync(It.IsAny<CancellationToken>()), times: Times.Never);
            MockDbContext.Verify(a => a.SaveChanges(), times: Times.Never);
            MockEmailService.Verify(a => a.InvoiceDone(It.IsAny<Invoice>(), It.IsAny<string>()), Times.Never);
            MockProjectHistoryService.Verify(a => a.InvoiceCancelled(It.IsAny<int>(), It.IsAny<Invoice>()), Times.Never);
            MockProjectHistoryService.Verify(a => a.InvoiceCompleted(It.IsAny<int>(), It.IsAny<Invoice>()), Times.Never);
        }

        [Theory]
        [InlineData(null)]
        [InlineData(" ")]
        [InlineData("")]
        public async Task ProcessTransferUpdatesWhenInvoiceHasNoSlothTransactionId(string slothId)
        {
            SetupGenericData();
            foreach (var invoice in Invoices)
            {
                invoice.Status = Invoice.Statuses.Completed;
            }

            Invoices[1].Status = Invoice.Statuses.Pending;
            Invoices[1].SlothTransactionId = slothId;
            MockData();

            await SlothService.ProcessTransferUpdates();
            MockDbContext.Verify(a => a.SaveChangesAsync(It.IsAny<CancellationToken>()), times: Times.Once); //It didn't save anything though
            MockDbContext.Verify(a => a.SaveChanges(), times: Times.Never);
            MockEmailService.Verify(a => a.InvoiceDone(It.IsAny<Invoice>(), It.IsAny<string>()), Times.Never);
            Invoices[1].Status.ShouldBe(Invoice.Statuses.Pending);
            MockProjectHistoryService.Verify(a => a.InvoiceCancelled(It.IsAny<int>(), It.IsAny<Invoice>()), Times.Never);
            MockProjectHistoryService.Verify(a => a.InvoiceCompleted(It.IsAny<int>(), It.IsAny<Invoice>()), Times.Never);
        }

        [Theory]
        [InlineData(HttpStatusCode.NoContent, true)]
        [InlineData(HttpStatusCode.NotFound, false)]
        public async Task ProcessTransferUpdatesWhenHttpStatus(HttpStatusCode statusCode, bool noContent)
        {
            var httpClientFactory = BasicSetup(out var httpClient, statusCode, noContent);

            var slothService = new SlothService(MockDbContext.Object, MockSlothSettings.Object, MockFinancialService.Object,
                JsonSerializerOptions, MockProjectHistoryService.Object, MockEmailService.Object, httpClientFactory.Object, MockAeSettings.Object, MockAeService.Object);

            SetupGenericData();
            foreach (var invoice in Invoices)
            {
                invoice.Status = Invoice.Statuses.Completed;
            }

            Invoices[1].Status = Invoice.Statuses.Pending;
            Invoices[1].SlothTransactionId = "FakeId";
            MockData();

            await slothService.ProcessTransferUpdates();
            MockDbContext.Verify(a => a.SaveChangesAsync(It.IsAny<CancellationToken>()), times: Times.Once); //It didn't save anything though
            MockDbContext.Verify(a => a.SaveChanges(), times: Times.Never);
            MockEmailService.Verify(a => a.InvoiceDone(It.IsAny<Invoice>(), It.IsAny<string>()), Times.Never);
            Invoices[1].Status.ShouldBe(Invoice.Statuses.Pending);
            MockProjectHistoryService.Verify(a => a.InvoiceCancelled(It.IsAny<int>(), It.IsAny<Invoice>()), Times.Never);
            MockProjectHistoryService.Verify(a => a.InvoiceCompleted(It.IsAny<int>(), It.IsAny<Invoice>()), Times.Never);
        }

        /// <summary>
        /// Still pending, don't do anything.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task ProcessTransferUpdatesSlothReturnsPending()
        {
            var httpClientFactory = BasicSetup(out var httpClient, HttpStatusCode.OK, false, SlothStatuses.PendingApproval);

            var slothService = new SlothService(MockDbContext.Object, MockSlothSettings.Object, MockFinancialService.Object,
                JsonSerializerOptions, MockProjectHistoryService.Object, MockEmailService.Object, httpClientFactory.Object, MockAeSettings.Object, MockAeService.Object);

            SetupGenericData();
            foreach (var invoice in Invoices)
            {
                invoice.Status = Invoice.Statuses.Completed;
            }

            Invoices[1].Status = Invoice.Statuses.Pending;
            Invoices[1].SlothTransactionId = "FakeId";
            MockData();

            await slothService.ProcessTransferUpdates();
            MockDbContext.Verify(a => a.SaveChangesAsync(It.IsAny<CancellationToken>()), times: Times.Once); //It didn't save anything though
            MockDbContext.Verify(a => a.SaveChanges(), times: Times.Never);
            MockEmailService.Verify(a => a.InvoiceDone(It.IsAny<Invoice>(), It.IsAny<string>()), Times.Never);
            Invoices[1].Status.ShouldBe(Invoice.Statuses.Pending);

            MockProjectHistoryService.Verify(a => a.InvoiceCancelled(It.IsAny<int>(), It.IsAny<Invoice>()), Times.Never);
            MockProjectHistoryService.Verify(a => a.InvoiceCompleted(It.IsAny<int>(), It.IsAny<Invoice>()), Times.Never);
        }

        [Fact]
        public async Task ProcessTransferUpdatesSlothReturnsCancelled()
        {
            var httpClientFactory = BasicSetup(out var httpClient, HttpStatusCode.OK, false, SlothStatuses.Cancelled);

            var slothService = new SlothService(MockDbContext.Object, MockSlothSettings.Object, MockFinancialService.Object,
                JsonSerializerOptions, MockProjectHistoryService.Object, MockEmailService.Object, httpClientFactory.Object, MockAeSettings.Object, MockAeService.Object);

            SetupGenericData();
            foreach (var invoice in Invoices)
            {
                invoice.Status = Invoice.Statuses.Completed;
            }

            Invoices[1].Status = Invoice.Statuses.Pending;
            Invoices[1].SlothTransactionId = "FakeId";
            MockData();

            await slothService.ProcessTransferUpdates();
            MockDbContext.Verify(a => a.SaveChangesAsync(It.IsAny<CancellationToken>()), times: Times.Once); //It didn't save anything though
            MockDbContext.Verify(a => a.SaveChanges(), times: Times.Never);
            MockEmailService.Verify(a => a.InvoiceDone(It.IsAny<Invoice>(), It.IsAny<string>()), Times.Never);
            Invoices[1].Status.ShouldBe(Invoice.Statuses.Pending);

            MockProjectHistoryService.Verify(a => a.InvoiceCancelled(Invoices[1].Project.Id, Invoices[1]), Times.Once);
            MockProjectHistoryService.Verify(a => a.InvoiceCompleted(It.IsAny<int>(), It.IsAny<Invoice>()), Times.Never);
        }

        [Fact]
        public async Task ProcessTransferUpdatesSlothReturnsCompletedWithActiveProject()
        {
            var httpClientFactory = BasicSetup(out var httpClient, HttpStatusCode.OK, false, SlothStatuses.Completed);

            var slothService = new SlothService(MockDbContext.Object, MockSlothSettings.Object, MockFinancialService.Object,
                JsonSerializerOptions, MockProjectHistoryService.Object, MockEmailService.Object, httpClientFactory.Object, MockAeSettings.Object, MockAeService.Object);

            SetupGenericData();
            foreach (var invoice in Invoices)
            {
                invoice.Status = Invoice.Statuses.Completed;
            }

            Invoices[1].Status = Invoice.Statuses.Pending;
            Invoices[1].SlothTransactionId = "FakeId";
            Invoices[1].Project.UpdateStatus(Project.Statuses.Active); 
            MockData();

            await slothService.ProcessTransferUpdates();
            MockDbContext.Verify(a => a.SaveChangesAsync(It.IsAny<CancellationToken>()), times: Times.AtLeastOnce); 
            MockDbContext.Verify(a => a.SaveChanges(), times: Times.Never);
            MockEmailService.Verify(a => a.InvoiceDone(Invoices[1], Invoice.Statuses.Completed), Times.Once);
            Invoices[1].Status.ShouldBe(Invoice.Statuses.Completed);

            MockProjectHistoryService.Verify(a => a.InvoiceCancelled(It.IsAny<int>(), It.IsAny<Invoice>()), Times.Never);
            MockProjectHistoryService.Verify(a => a.InvoiceCompleted(Invoices[1].Project.Id, Invoices[1]), Times.Once);
        }

        [Fact]
        public async Task ProcessTransferUpdatesSlothReturnsCompletedWithActiveProjects()
        {
            var httpClientFactory = BasicSetup(out var httpClient, HttpStatusCode.OK, false, SlothStatuses.Completed);

            var slothService = new SlothService(MockDbContext.Object, MockSlothSettings.Object, MockFinancialService.Object,
                JsonSerializerOptions, MockProjectHistoryService.Object, MockEmailService.Object, httpClientFactory.Object, MockAeSettings.Object, MockAeService.Object);

            SetupGenericData();
            foreach (var invoice in Invoices)
            {
                invoice.Status = Invoice.Statuses.Pending;
                invoice.SlothTransactionId = "FakeId";
                invoice.Project.UpdateStatus(Project.Statuses.Active);
            }

            MockData();

            await slothService.ProcessTransferUpdates();
            MockDbContext.Verify(a => a.SaveChangesAsync(It.IsAny<CancellationToken>()), times: Times.AtLeastOnce);
            MockDbContext.Verify(a => a.SaveChanges(), times: Times.Never);
            MockEmailService.Verify(a => a.InvoiceDone(It.IsAny<Invoice>(), Invoice.Statuses.Completed), Times.Exactly(3));
            
            Invoices[0].Status.ShouldBe(Invoice.Statuses.Completed);
            Invoices[1].Status.ShouldBe(Invoice.Statuses.Completed);
            Invoices[2].Status.ShouldBe(Invoice.Statuses.Completed);

            MockProjectHistoryService.Verify(a => a.InvoiceCancelled(It.IsAny<int>(), It.IsAny<Invoice>()), Times.Never);
            MockProjectHistoryService.Verify(a => a.InvoiceCompleted(It.IsAny<int>(), It.IsAny<Invoice>()), Times.Exactly(3));

            Invoices[1].Project.Status.ShouldBe(Project.Statuses.Active);
            MockProjectHistoryService.Verify(a => a.ProjectCompleted(It.IsAny<int>(), It.IsAny<Project>()), Times.Never);
        }

        [Fact]
        public async Task ProcessTransferUpdatesSlothReturnsCompletedWithProjectFinalInvoicePending()
        {
            var httpClientFactory = BasicSetup(out var httpClient, HttpStatusCode.OK, false, SlothStatuses.Completed);

            var slothService = new SlothService(MockDbContext.Object, MockSlothSettings.Object, MockFinancialService.Object,
                JsonSerializerOptions, MockProjectHistoryService.Object, MockEmailService.Object, httpClientFactory.Object, MockAeSettings.Object, MockAeService.Object);

            SetupGenericData();
            foreach (var invoice in Invoices)
            {
                invoice.Status = Invoice.Statuses.Completed;
            }

            Invoices[1].Status = Invoice.Statuses.Pending;
            Invoices[1].SlothTransactionId = "FakeId";
            Invoices[1].Project.UpdateStatus(Project.Statuses.FinalInvoicePending);
            MockData();

            await slothService.ProcessTransferUpdates();
            MockDbContext.Verify(a => a.SaveChangesAsync(It.IsAny<CancellationToken>()), times: Times.AtLeastOnce);
            MockDbContext.Verify(a => a.SaveChanges(), times: Times.Never);
            MockEmailService.Verify(a => a.InvoiceDone(Invoices[1], Invoice.Statuses.Completed), Times.Once);
            Invoices[1].Status.ShouldBe(Invoice.Statuses.Completed);
            Invoices[1].Project.Status.ShouldBe(Project.Statuses.Completed);
            MockProjectHistoryService.Verify(a => a.ProjectCompleted(Invoices[1].Project.Id, Invoices[1].Project), Times.Once);

            MockProjectHistoryService.Verify(a => a.InvoiceCancelled(It.IsAny<int>(), It.IsAny<Invoice>()), Times.Never);
            MockProjectHistoryService.Verify(a => a.InvoiceCompleted(Invoices[1].Project.Id, Invoices[1]), Times.Once);
        }

        #endregion ProcessTransferUpdates tests
    }
}
