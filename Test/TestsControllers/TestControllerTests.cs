using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Harvest.Core.Models;
using Harvest.Web.Controllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shouldly;
using TestHelpers.Helpers;
using Xunit;
using Xunit.Abstractions;

namespace Test.TestsControllers
{
    /// <summary>
    /// The only thing we really care about for this controller is that it is System Access
    /// </summary>
    [Trait("Category", "ControllerReflectionTests")]
    public class TestControllerReflectionTests
    {
        private readonly ITestOutputHelper output;
        public ControllerReflection ControllerReflection;

        public TestControllerReflectionTests(ITestOutputHelper output)
        {
            this.output = output;
            ControllerReflection = new ControllerReflection(this.output, typeof(TestController));
        }

        [Fact]
        public void TestControllerClassAttributes()
        {
            ControllerReflection.ControllerInherits("SuperController");
            ControllerReflection.ClassExpectedAttribute<ControllerAttribute>(3);
            var attributes = ControllerReflection.ClassExpectedAttribute<AuthorizeAttribute>(3, showListOfAttributes: true);
            attributes.ElementAt(0).Policy.ShouldBe(AccessCodes.SystemAccess);

            ControllerReflection.ClassExpectedAttribute<AutoValidateAntiforgeryTokenAttribute>(3);
            ControllerReflection.ClassExpectedAttribute<ControllerAttribute>(3);
        }
    }
}
