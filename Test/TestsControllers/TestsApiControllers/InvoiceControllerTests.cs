using Harvest.Core.Models;
using Harvest.Web.Controllers.Api;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shouldly;
using System.Linq;
using System.Runtime.CompilerServices;
using TestHelpers.Helpers;
using Xunit;
using Xunit.Abstractions;

namespace Test.TestsControllers.TestsApiControllers
{
    [Trait("Category", "ControllerTests")]
    public class InvoiceControllerTests
    {
    }

    [Trait("Category", "ControllerReflectionTests")]
    public class InvoiceControllerReflectionTests
    {
        private readonly ITestOutputHelper output;
        public ControllerReflection ControllerReflection;

        public InvoiceControllerReflectionTests(ITestOutputHelper output)
        {
            this.output = output;
            ControllerReflection = new ControllerReflection(this.output, typeof(InvoiceController));
        }

        [Fact]
        public void TestControllerClassAttributes()
        {
            ControllerReflection.ControllerInherits("SuperController");
            ControllerReflection.ClassExpectedAttribute<ControllerAttribute>(3);
            var attributes = ControllerReflection.ClassExpectedAttribute<AuthorizeAttribute>(3, showListOfAttributes: true);
            attributes.ElementAt(0).Policy.ShouldBe(AccessCodes.InvoiceAccess);
            ControllerReflection.ClassExpectedAttribute<AutoValidateAntiforgeryTokenAttribute>(3); //TODO: Enable when this gets added.
            ControllerReflection.ClassExpectedAttribute<ControllerAttribute>(3);
        }

        [Fact]
        public void TestControllerContainsExpectedNumberOfPublicMethods()
        {
            ControllerReflection.ControllerPublicMethods(4);
        }

        [Fact]
        public void TestControllerMethodAttributes()
        {
            var methodName = "";
#if DEBUG
            var countAdjustment = 1;
#else
            var countAdjustment = 0;
#endif

            //1
            methodName = "Get";
            ControllerReflection.MethodExpectedAttribute<HttpGetAttribute>(methodName, countAdjustment + 2);
            ControllerReflection.MethodExpectedAttribute<AsyncStateMachineAttribute>(methodName, countAdjustment + 2);

            //2
            methodName = "List";
            ControllerReflection.MethodExpectedAttribute<HttpGetAttribute>(methodName, countAdjustment + 2);
            ControllerReflection.MethodExpectedAttribute<AsyncStateMachineAttribute>(methodName, countAdjustment + 2);

            //3
            methodName = "DoCloseout";
            ControllerReflection.MethodExpectedAttribute<HttpPostAttribute>(methodName, countAdjustment + 3);
            ControllerReflection.MethodExpectedAttribute<AsyncStateMachineAttribute>(methodName, countAdjustment + 3);
            var attribute = ControllerReflection.MethodExpectedAttribute<AuthorizeAttribute>(methodName, countAdjustment + 3);
            attribute.ShouldNotBeNull();
            attribute.ElementAt(0).Policy.ShouldBe(AccessCodes.PrincipalInvestigator);

            //4
            methodName = "InitiateCloseout";
            ControllerReflection.MethodExpectedAttribute<HttpPostAttribute>(methodName, countAdjustment + 3);
            ControllerReflection.MethodExpectedAttribute<AsyncStateMachineAttribute>(methodName, countAdjustment + 3);
            attribute = ControllerReflection.MethodExpectedAttribute<AuthorizeAttribute>(methodName, countAdjustment + 3);
            attribute.ShouldNotBeNull();
            attribute.ElementAt(0).Policy.ShouldBe(AccessCodes.FieldManagerAccess);
        }

    }
}
