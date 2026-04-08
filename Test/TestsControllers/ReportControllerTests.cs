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
            ControllerReflection.ClassExpectedAttribute<ControllerAttribute>(2);
            ControllerReflection.ClassExpectedAttribute<AutoValidateAntiforgeryTokenAttribute>(2);
            ControllerReflection.ClassExpectedAttribute<ControllerAttribute>(2);
        }

        [Fact]
        public void TestControllerContainsExpectedNumberOfPublicMethods()
        {
            ControllerReflection.ControllerPublicMethods(7);
        }

        [Fact]
        public void TestControllerMethodAttributes()
        {
            var attributes = ControllerReflection.MethodExpectedAttribute<AuthorizeAttribute>("Index", 1);
            attributes.ElementAt(0).Policy.ShouldBe(AccessCodes.ReportAccess);

            attributes = ControllerReflection.MethodExpectedAttribute<AuthorizeAttribute>("AllProjects", 3);
            attributes.ElementAt(0).Policy.ShouldBe(AccessCodes.ReportAccess);
            ControllerReflection.MethodExpectedAttribute<AsyncStateMachineAttribute>("AllProjects", 3);

            attributes = ControllerReflection.MethodExpectedAttribute<AuthorizeAttribute>("HistoricalRateActivity", 3);
            attributes.ElementAt(0).Policy.ShouldBe(AccessCodes.ReportAccess);
            ControllerReflection.MethodExpectedAttribute<AsyncStateMachineAttribute>("HistoricalRateActivity", 3);

            attributes = ControllerReflection.MethodExpectedAttribute<AuthorizeAttribute>("StaleProjects", 3);
            attributes.ElementAt(0).Policy.ShouldBe(AccessCodes.ReportAccess);
            ControllerReflection.MethodExpectedAttribute<AsyncStateMachineAttribute>("StaleProjects", 3);

            attributes = ControllerReflection.MethodExpectedAttribute<AuthorizeAttribute>("ProjectsUnbilledExpenses", 3);
            attributes.ElementAt(0).Policy.ShouldBe(AccessCodes.ReportAccess);
            ControllerReflection.MethodExpectedAttribute<AsyncStateMachineAttribute>("ProjectsUnbilledExpenses", 3);

            attributes = ControllerReflection.MethodExpectedAttribute<AuthorizeAttribute>("UnbilledExpenses", 3);
            attributes.ElementAt(0).Policy.ShouldBe(AccessCodes.ReportAccess);
            ControllerReflection.MethodExpectedAttribute<AsyncStateMachineAttribute>("UnbilledExpenses", 3);

            attributes = ControllerReflection.MethodExpectedAttribute<AuthorizeAttribute>("WeeklyHoursByWorker", 3);
            attributes.ElementAt(0).Policy.ShouldBe(AccessCodes.SupervisorAccess);
            ControllerReflection.MethodExpectedAttribute<AsyncStateMachineAttribute>("WeeklyHoursByWorker", 3);
        }
    }
}
