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
    public class TicketControllerTests
    {
    }

    [Trait("Category", "ControllerReflectionTests")]
    public class TicketControllerReflectionTests
    {
        private readonly ITestOutputHelper output;
        public ControllerReflection ControllerReflection;

        public TicketControllerReflectionTests(ITestOutputHelper output)
        {
            this.output = output;
            ControllerReflection = new ControllerReflection(this.output, typeof(TicketController));
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
            methodName = "GetList";
            ControllerReflection.MethodExpectedAttribute<HttpGetAttribute>(methodName, countAdjustment + 3);
            var authAttribute = ControllerReflection.MethodExpectedAttribute<AuthorizeAttribute>(methodName, countAdjustment + 3);
            authAttribute.ShouldNotBeNull();
            authAttribute.ElementAt(0).Policy.ShouldBe(AccessCodes.PrincipalInvestigator);
            ControllerReflection.MethodExpectedAttribute<AsyncStateMachineAttribute>(methodName, countAdjustment + 3);

            //2
            methodName = "UpdateWorkNotes";
            ControllerReflection.MethodExpectedAttribute<HttpPostAttribute>(methodName, countAdjustment + 3);
            authAttribute = ControllerReflection.MethodExpectedAttribute<AuthorizeAttribute>(methodName, countAdjustment + 3);
            authAttribute.ShouldNotBeNull();
            authAttribute.ElementAt(0).Policy.ShouldBe(AccessCodes.FieldManagerAccess);
            ControllerReflection.MethodExpectedAttribute<AsyncStateMachineAttribute>(methodName, countAdjustment + 3);

            //3 (Create Post)
            methodName = "Create";
            ControllerReflection.MethodExpectedAttribute<HttpPostAttribute>(methodName, countAdjustment + 3);
            authAttribute = ControllerReflection.MethodExpectedAttribute<AuthorizeAttribute>(methodName, countAdjustment + 3);
            authAttribute.ShouldNotBeNull();
            authAttribute.ElementAt(0).Policy.ShouldBe(AccessCodes.PrincipalInvestigator);
            ControllerReflection.MethodExpectedAttribute<AsyncStateMachineAttribute>(methodName, countAdjustment + 3);

            //4
            methodName = "Get";
            var getAttributes = ControllerReflection.MethodExpectedAttribute<HttpGetAttribute>(methodName, countAdjustment + 3);
            getAttributes.ShouldNotBeNull();
            getAttributes.ElementAt(0).Template.ShouldBe("/api/[controller]/[action]/{projectId}/{ticketId}");
            authAttribute = ControllerReflection.MethodExpectedAttribute<AuthorizeAttribute>(methodName, countAdjustment + 3);
            authAttribute.ShouldNotBeNull();
            authAttribute.ElementAt(0).Policy.ShouldBe(AccessCodes.PrincipalInvestigator);
            ControllerReflection.MethodExpectedAttribute<AsyncStateMachineAttribute>(methodName, countAdjustment + 3);

            //5
            methodName = "Reply";
            ControllerReflection.MethodExpectedAttribute<HttpPostAttribute>(methodName, countAdjustment + 3);
            authAttribute = ControllerReflection.MethodExpectedAttribute<AuthorizeAttribute>(methodName, countAdjustment + 3);
            authAttribute.ShouldNotBeNull();
            authAttribute.ElementAt(0).Policy.ShouldBe(AccessCodes.PrincipalInvestigator);
            ControllerReflection.MethodExpectedAttribute<AsyncStateMachineAttribute>(methodName, countAdjustment + 3);

            //6
            methodName = "UploadFiles";
            var postAttributes = ControllerReflection.MethodExpectedAttribute<HttpPostAttribute>(methodName, countAdjustment + 3);
            postAttributes.ShouldNotBeNull();
            postAttributes.ElementAt(0).Template.ShouldBe("/api/[controller]/[action]/{projectId}/{ticketId}");
            authAttribute = ControllerReflection.MethodExpectedAttribute<AuthorizeAttribute>(methodName, countAdjustment + 3);
            authAttribute.ShouldNotBeNull();
            authAttribute.ElementAt(0).Policy.ShouldBe(AccessCodes.PrincipalInvestigator);
            ControllerReflection.MethodExpectedAttribute<AsyncStateMachineAttribute>(methodName, countAdjustment + 3);

            //7
            methodName = "Close";
            ControllerReflection.MethodExpectedAttribute<HttpPostAttribute>(methodName, countAdjustment + 3);
            authAttribute = ControllerReflection.MethodExpectedAttribute<AuthorizeAttribute>(methodName, countAdjustment + 3);
            authAttribute.ShouldNotBeNull();
            authAttribute.ElementAt(0).Policy.ShouldBe(AccessCodes.PrincipalInvestigator);
            ControllerReflection.MethodExpectedAttribute<AsyncStateMachineAttribute>(methodName, countAdjustment + 3);

            //8
            methodName = "RequiringPIAttention";
            ControllerReflection.MethodExpectedAttribute<HttpGetAttribute>(methodName, countAdjustment + 2);
            ControllerReflection.MethodExpectedAttribute<AsyncStateMachineAttribute>(methodName, countAdjustment + 2);

            //9
            methodName = "RequiringManagerAttention";
            ControllerReflection.MethodExpectedAttribute<HttpGetAttribute>(methodName, countAdjustment + 3);
            authAttribute = ControllerReflection.MethodExpectedAttribute<AuthorizeAttribute>(methodName, countAdjustment + 3);
            authAttribute.ShouldNotBeNull();
            authAttribute.ElementAt(0).Policy.ShouldBe(AccessCodes.SupervisorAccess);
            ControllerReflection.MethodExpectedAttribute<AsyncStateMachineAttribute>(methodName, countAdjustment + 3);
        }
    }
}
