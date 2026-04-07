using Harvest.Core.Models;
using Harvest.Web.Controllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shouldly;
using System.Linq;
using System.Runtime.CompilerServices;
using TestHelpers.Helpers;
using Xunit;
using Xunit.Abstractions;

namespace Test.TestsControllers
{
    [Trait("Category", "ControllerTests")]
    public class ReportControllerTests
    {
    }

    [Trait("Category", "ControllerReflectionTests")]
    public class ReportControllerReflectionTests
    {
        private readonly ITestOutputHelper output;
        public ControllerReflection ControllerReflection;

        public ReportControllerReflectionTests(ITestOutputHelper output)
        {
            this.output = output;
            ControllerReflection = new ControllerReflection(this.output, typeof(ReportController));
        }

        [Fact]
        public void TestControllerClassAttributes()
        {
            ControllerReflection.ControllerInherits("SuperController");
            ControllerReflection.ClassExpectedAttribute<ControllerAttribute>(3);
            var attributes = ControllerReflection.ClassExpectedAttribute<AuthorizeAttribute>(3, showListOfAttributes: false);
            attributes.ElementAt(0).Policy.ShouldBe(AccessCodes.ReportAccess);

            ControllerReflection.ClassExpectedAttribute<AutoValidateAntiforgeryTokenAttribute>(3);
            ControllerReflection.ClassExpectedAttribute<ControllerAttribute>(3);
        }

        [Fact]
        public void TestControllerContainsExpectedNumberOfPublicMethods()
        {
            ControllerReflection.ControllerPublicMethods(6);
        }

        [Fact]
        public void TestControllerMethodAttributes()
        {
#if DEBUG
            var countAdjustment = 1;
#else
            var countAdjustment = 0;
#endif

            ControllerReflection.MethodExpectedNoAttribute("Index");
            ControllerReflection.MethodExpectedAttribute<AsyncStateMachineAttribute>("AllProjects", countAdjustment + 1);
            ControllerReflection.MethodExpectedAttribute<AsyncStateMachineAttribute>("HistoricalRateActivity", countAdjustment + 1);
            ControllerReflection.MethodExpectedAttribute<AsyncStateMachineAttribute>("StaleProjects", countAdjustment + 1);
            ControllerReflection.MethodExpectedAttribute<AsyncStateMachineAttribute>("ProjectsUnbilledExpenses", countAdjustment + 1);
            ControllerReflection.MethodExpectedAttribute<AsyncStateMachineAttribute>("UnbilledExpenses", countAdjustment + 1);
        }
    }
}
