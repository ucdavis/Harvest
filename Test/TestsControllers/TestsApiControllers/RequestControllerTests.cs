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
            ControllerReflection.ControllerPublicMethods(9);
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
            methodName = "Create";
            ControllerReflection.MethodExpectedAttribute<HttpGetAttribute>(methodName, 1);

            //2 - 1
            methodName = "Approve";
            ControllerReflection.MethodExpectedAttribute<HttpGetAttribute>(methodName, 2);
            var authAttribute = ControllerReflection.MethodExpectedAttribute<AuthorizeAttribute>(methodName, 2);
            authAttribute.ShouldNotBeNull();
            authAttribute.ElementAt(0).Policy.ShouldBe(AccessCodes.PrincipalInvestigator);

            //3
            methodName = "ChangeAccount";
            ControllerReflection.MethodExpectedAttribute<HttpGetAttribute>(methodName, 2);
            authAttribute = ControllerReflection.MethodExpectedAttribute<AuthorizeAttribute>(methodName, 2);
            authAttribute.ShouldNotBeNull();
            authAttribute.ElementAt(0).Policy.ShouldBe(AccessCodes.PrincipalInvestigator);

            //4
            methodName = "Cancel";
            ControllerReflection.MethodExpectedAttribute<HttpPostAttribute>(methodName, countAdjustment + 3);
            authAttribute = ControllerReflection.MethodExpectedAttribute<AuthorizeAttribute>(methodName, countAdjustment + 3);
            authAttribute.ShouldNotBeNull();
            authAttribute.ElementAt(0).Policy.ShouldBe(AccessCodes.FieldManagerAccess);
            ControllerReflection.MethodExpectedAttribute<AsyncStateMachineAttribute>(methodName, countAdjustment + 3);

            //5 (Post of Approve)
            methodName = "Approve";
            ControllerReflection.MethodExpectedAttribute<HttpPostAttribute>(methodName, countAdjustment + 3, isSecondMethod: true);
            authAttribute = ControllerReflection.MethodExpectedAttribute<AuthorizeAttribute>(methodName, countAdjustment + 3, isSecondMethod: true);
            authAttribute.ShouldNotBeNull();
            authAttribute.ElementAt(0).Policy.ShouldBe(AccessCodes.PrincipalInvestigator);
            ControllerReflection.MethodExpectedAttribute<AsyncStateMachineAttribute>(methodName, countAdjustment + 3, isSecondMethod: true);

            //6
            methodName = "RejectQuote";
            ControllerReflection.MethodExpectedAttribute<HttpPostAttribute>(methodName, countAdjustment + 3);
            authAttribute = ControllerReflection.MethodExpectedAttribute<AuthorizeAttribute>(methodName, countAdjustment + 3);
            authAttribute.ShouldNotBeNull();
            authAttribute.ElementAt(0).Policy.ShouldBe(AccessCodes.PrincipalInvestigator);
            ControllerReflection.MethodExpectedAttribute<AsyncStateMachineAttribute>(methodName, countAdjustment + 3);

            //7 (Post of ChangeAccount)
            methodName = "ChangeAccount";
            ControllerReflection.MethodExpectedAttribute<HttpPostAttribute>(methodName, countAdjustment + 3, isSecondMethod: true);
            authAttribute = ControllerReflection.MethodExpectedAttribute<AuthorizeAttribute>(methodName, countAdjustment + 3, isSecondMethod: true);
            authAttribute.ShouldNotBeNull();
            authAttribute.ElementAt(0).Policy.ShouldBe(AccessCodes.PrincipalInvestigator);
            ControllerReflection.MethodExpectedAttribute<AsyncStateMachineAttribute>(methodName, countAdjustment + 3, isSecondMethod: true);

            //8 
            methodName = "Files";
            ControllerReflection.MethodExpectedAttribute<HttpPostAttribute>(methodName, countAdjustment + 3);
            authAttribute = ControllerReflection.MethodExpectedAttribute<AuthorizeAttribute>(methodName, countAdjustment + 3);
            authAttribute.ShouldNotBeNull();
            authAttribute.ElementAt(0).Policy.ShouldBe(AccessCodes.PrincipalInvestigator);
            ControllerReflection.MethodExpectedAttribute<AsyncStateMachineAttribute>(methodName, countAdjustment + 3);

            //9 (Post of Create) 
            methodName = "Create";
            ControllerReflection.MethodExpectedAttribute<HttpPostAttribute>(methodName, countAdjustment + 2, isSecondMethod: true);
            ControllerReflection.MethodExpectedAttribute<AsyncStateMachineAttribute>(methodName, countAdjustment + 2, isSecondMethod: true);
        }
    }
}
