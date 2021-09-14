using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Harvest.Core.Data;
using Moq;
using Xunit;

namespace Test.TestsServices
{
    [Trait("Category", "ServiceTest")]
    public class InvoiceServiceTests
    {
        public Mock<AppDbContext> MockDbContext { get; set; }
    }
}
