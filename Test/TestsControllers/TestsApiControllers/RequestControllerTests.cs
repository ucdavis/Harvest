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
    public class RequestControllerTests
    {
    }

    [Trait("Category", "ControllerReflectionTests")]
    public class RequestControllerReflectionTests
    {
        private readonly ITestOutputHelper output;
        public ControllerReflection ControllerReflection;

        public RequestControllerReflectionTests(ITestOutputHelper output)
        {
            this.output = output;
            ControllerReflection = new ControllerReflection(this.output, typeof(RequestController));
        }

        [Fact]
        public void TestControllerClassAttributes()
        {
            ControllerReflection.ControllerInherits("SuperController");
            ControllerReflection.ClassExpectedAttribute<ControllerAttribute>(3);
            var attributes = ControllerReflection.ClassExpectedAttribute<AuthorizeAttribute>(3, showListOfAttributes: true);
            attributes.ElementAt(0).Policy.ShouldBeNull();
            ControllerReflection.ClassExpectedAttribute<AutoValidateAntiforgeryTokenAttribute>(3); //TODO: Enable when this gets added.
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
            var methodName = "";
#if DEBUG
            var countAdjustment = 1;
#else
            var countAdjustment = 0;
#endif

            //1
            methodName = "Cancel";
            ControllerReflection.MethodExpectedAttribute<HttpPostAttribute>(methodName, countAdjustment + 3, testMessage: "Cancel");
            var authAttribute = ControllerReflection.MethodExpectedAttribute<AuthorizeAttribute>(methodName, countAdjustment + 3);
            authAttribute.ShouldNotBeNull();
            authAttribute.ElementAt(0).Policy.ShouldBe(AccessCodes.FieldManagerAccess);
            ControllerReflection.MethodExpectedAttribute<AsyncStateMachineAttribute>(methodName, countAdjustment + 3);

            //2 (Post of Approve)
            methodName = "Approve";
            ControllerReflection.MethodExpectedAttribute<HttpPostAttribute>(methodName, countAdjustment + 3, testMessage: "Approve");
            authAttribute = ControllerReflection.MethodExpectedAttribute<AuthorizeAttribute>(methodName, countAdjustment + 3);
            authAttribute.ShouldNotBeNull();
            authAttribute.ElementAt(0).Policy.ShouldBe(AccessCodes.PIandFinance);
            ControllerReflection.MethodExpectedAttribute<AsyncStateMachineAttribute>(methodName, countAdjustment + 3);

            //3
            methodName = "RejectQuote";
            ControllerReflection.MethodExpectedAttribute<HttpPostAttribute>(methodName, countAdjustment + 3, testMessage: "RejectQuote");
            authAttribute = ControllerReflection.MethodExpectedAttribute<AuthorizeAttribute>(methodName, countAdjustment + 3);
            authAttribute.ShouldNotBeNull();
            authAttribute.ElementAt(0).Policy.ShouldBe(AccessCodes.PrincipalInvestigatorOnly);
            ControllerReflection.MethodExpectedAttribute<AsyncStateMachineAttribute>(methodName, countAdjustment + 3);

            //4 (Post of ChangeAccount)
            methodName = "ChangeAccount";
            ControllerReflection.MethodExpectedAttribute<HttpPostAttribute>(methodName, countAdjustment + 3, testMessage: "ChangeAccount");
            authAttribute = ControllerReflection.MethodExpectedAttribute<AuthorizeAttribute>(methodName, countAdjustment + 3);
            authAttribute.ShouldNotBeNull();
            authAttribute.ElementAt(0).Policy.ShouldBe(AccessCodes.PrincipalInvestigatorOnly);
            ControllerReflection.MethodExpectedAttribute<AsyncStateMachineAttribute>(methodName, countAdjustment + 3);

            //5 
            methodName = "Files";
            ControllerReflection.MethodExpectedAttribute<HttpPostAttribute>(methodName, countAdjustment + 3, testMessage: "Files");
            authAttribute = ControllerReflection.MethodExpectedAttribute<AuthorizeAttribute>(methodName, countAdjustment + 3);
            authAttribute.ShouldNotBeNull();
            authAttribute.ElementAt(0).Policy.ShouldBe(AccessCodes.PrincipalInvestigator);
            ControllerReflection.MethodExpectedAttribute<AsyncStateMachineAttribute>(methodName, countAdjustment + 3);

            //6 (Post of Create) 
            methodName = "Create";
            ControllerReflection.MethodExpectedAttribute<HttpPostAttribute>(methodName, countAdjustment + 3, testMessage: "Create");
            ControllerReflection.MethodExpectedAttribute<AsyncStateMachineAttribute>(methodName, countAdjustment + 3);
            var routeAttribute = ControllerReflection.MethodExpectedAttribute<RouteAttribute>(methodName, countAdjustment + 3);
            routeAttribute.ElementAt(0).Template.ShouldBe("/api/{controller}/{action}");
        }
    }
}
