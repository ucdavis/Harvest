using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Harvest.Core.Data;
using Harvest.Core.Models.Settings;
using Harvest.Core.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Moq;
using Shouldly;
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
        public void TestSample()
        {
            var xxx = "1";
            xxx.ShouldBe("1");

            var invoiceServ = new InvoiceService(MockDbContext.Object, MockProjectHistoryService.Object, MockEmailService.Object,
                MockExpenseService.Object, MockDevSettings.Object);
        }
    }
}
