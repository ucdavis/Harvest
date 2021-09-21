using Harvest.Core.Models;
using Harvest.Web.Controllers.Api;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shouldly;
using System.Linq;
using System.Net.Mime;
using System.Runtime.CompilerServices;
using TestHelpers.Helpers;
using Xunit;
using Xunit.Abstractions;

namespace Test.TestsControllers.TestsApiControllers
{
    [Trait("Category", "ControllerTests")]
    public class ExpenseControllerTests
    {
    }

    [Trait("Category", "ControllerReflectionTests")]
    public class ExpenseControllerReflectionTests
    {
        private readonly ITestOutputHelper output;
        public ControllerReflection ControllerReflection;

        public ExpenseControllerReflectionTests(ITestOutputHelper output)
        {
            this.output = output;
            ControllerReflection = new ControllerReflection(this.output, typeof(ExpenseController));
        }

        [Fact]
        public void TestControllerClassAttributes()
        {
            ControllerReflection.ControllerInherits("Controller");
            ControllerReflection.ClassExpectedAttribute<ControllerAttribute>(2);
            var attributes = ControllerReflection.ClassExpectedAttribute<AuthorizeAttribute>(2, showListOfAttributes: true);
            attributes.ElementAt(0).Policy.ShouldBeNull();

            //ControllerReflection.ClassExpectedAttribute<AutoValidateAntiforgeryTokenAttribute>(3); //TODO: Enable when this gets added.
            ControllerReflection.ClassExpectedAttribute<ControllerAttribute>(2);
        }

        [Fact]
        public void TestControllerContainsExpectedNumberOfPublicMethods()
        {
            ControllerReflection.ControllerPublicMethods(8);
        }

        [Fact]
        public void TestControllerMethodAttributes()
        {
#if DEBUG
            var countAdjustment = 1;
#else
            var countAdjustment = 0;
#endif

            //1
            ControllerReflection.MethodExpectedNoAttribute("Entry");

            //2
            ControllerReflection.MethodExpectedNoAttribute("Unbilled");

            //3
            ControllerReflection.MethodExpectedAttribute<HttpPostAttribute>("Create", countAdjustment + 4, showListOfAttributes:true);
            var authAttribute = ControllerReflection.MethodExpectedAttribute<AuthorizeAttribute>("Create", countAdjustment + 4);
            authAttribute.ShouldNotBeNull();
            authAttribute.ElementAt(0).Policy.ShouldBe(AccessCodes.WorkerAccess);
            var consumesAtt = ControllerReflection.MethodExpectedAttribute<ConsumesAttribute>("Create", countAdjustment + 4);
            consumesAtt.ShouldNotBeNull();
            consumesAtt.ElementAt(0).ContentTypes.Count.ShouldBe(1);
            consumesAtt.ElementAt(0).ContentTypes[0].ShouldBe(MediaTypeNames.Application.Json);
            ControllerReflection.MethodExpectedAttribute<AsyncStateMachineAttribute>("Create", countAdjustment + 4);

            //4
            ControllerReflection.MethodExpectedAttribute<HttpPostAttribute>("CreateAcreage", countAdjustment + 4);
            authAttribute = ControllerReflection.MethodExpectedAttribute<AuthorizeAttribute>("CreateAcreage", countAdjustment + 4);
            authAttribute.ShouldNotBeNull();
            authAttribute.ElementAt(0).Policy.ShouldBe(AccessCodes.SupervisorAccess);
            consumesAtt = ControllerReflection.MethodExpectedAttribute<ConsumesAttribute>("CreateAcreage", countAdjustment + 4);
            consumesAtt.ShouldNotBeNull();
            consumesAtt.ElementAt(0).ContentTypes.Count.ShouldBe(1);
            consumesAtt.ElementAt(0).ContentTypes[0].ShouldBe(MediaTypeNames.Application.Json);
            ControllerReflection.MethodExpectedAttribute<AsyncStateMachineAttribute>("CreateAcreage", countAdjustment + 4);

            //5
            ControllerReflection.MethodExpectedAttribute<HttpPostAttribute>("Delete", countAdjustment + 3);
            authAttribute = ControllerReflection.MethodExpectedAttribute<AuthorizeAttribute>("Delete", countAdjustment + 3);
            authAttribute.ShouldNotBeNull();
            authAttribute.ElementAt(0).Policy.ShouldBe(AccessCodes.SupervisorAccess);
            ControllerReflection.MethodExpectedAttribute<AsyncStateMachineAttribute>("Delete", countAdjustment + 3);

            //6 This should probably do PI auth... 
            ControllerReflection.MethodExpectedAttribute<HttpGetAttribute>("GetUnbilled", countAdjustment + 2);
            ControllerReflection.MethodExpectedAttribute<AsyncStateMachineAttribute>("GetUnbilled", countAdjustment + 2);

            //7 
            ControllerReflection.MethodExpectedAttribute<HttpGetAttribute>("GetUnbilledTotal", countAdjustment + 2);
            ControllerReflection.MethodExpectedAttribute<AsyncStateMachineAttribute>("GetUnbilledTotal", countAdjustment + 2);

            //8
            ControllerReflection.MethodExpectedAttribute<HttpGetAttribute>("GetRecentExpensedProjects", countAdjustment + 3);
            authAttribute = ControllerReflection.MethodExpectedAttribute<AuthorizeAttribute>("GetRecentExpensedProjects", countAdjustment + 3);
            authAttribute.ShouldNotBeNull();
            authAttribute.ElementAt(0).Policy.ShouldBe(AccessCodes.WorkerAccess);
            ControllerReflection.MethodExpectedAttribute<AsyncStateMachineAttribute>("GetRecentExpensedProjects", countAdjustment + 3);
        }

    }
}
