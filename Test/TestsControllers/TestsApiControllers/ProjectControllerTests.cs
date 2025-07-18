using Harvest.Core.Models;
using Harvest.Web.Controllers.Api;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shouldly;
using System;
using System.Linq;
using System.Runtime.CompilerServices;
using TestHelpers.Helpers;
using Xunit;
using Xunit.Abstractions;

namespace Test.TestsControllers.TestsApiControllers
{
    [Trait("Category", "ControllerTests")]
    public class ProjectControllerTests
    {
    }
    [Trait("Category", "ControllerReflectionTests")]
    public class ProjectControllerReflectionTests
    {
        private readonly ITestOutputHelper output;
        public ControllerReflection ControllerReflection;

        public ProjectControllerReflectionTests(ITestOutputHelper output)
        {
            this.output = output;
            ControllerReflection = new ControllerReflection(this.output, typeof(ProjectController));
        }

        [Fact]
        public void TestControllerClassAttributes()
        {
            ControllerReflection.ControllerInherits("SuperController");
            ControllerReflection.ClassExpectedAttribute<ControllerAttribute>(3);
            var attributes = ControllerReflection.ClassExpectedAttribute<AuthorizeAttribute>(3, showListOfAttributes: false);
            attributes.ElementAt(0).Policy.ShouldBeNull();

            ControllerReflection.ClassExpectedAttribute<AutoValidateAntiforgeryTokenAttribute>(3);
            ControllerReflection.ClassExpectedAttribute<ControllerAttribute>(3);
        }

        [Fact]
        public void TestControllerContainsExpectedNumberOfPublicMethods()
        {
            ControllerReflection.ControllerPublicMethods(16);
        }
        [Fact]
        public void TestControllerMethodAttributes()
        {
            var methodName = ""; //TODO: Use methodName for other tests below
#if DEBUG
            var countAdjustment = 1;
#else
            var countAdjustment = 0;
#endif

            //1
            var attribute = ControllerReflection.MethodExpectedAttribute<AuthorizeAttribute>("All", countAdjustment + 2, testMessage: "All");
            attribute.ShouldNotBeNull();
            attribute.ElementAt(0).Policy.ShouldBe(AccessCodes.ProjectAccess);
            ControllerReflection.MethodExpectedAttribute<AsyncStateMachineAttribute>("All", countAdjustment + 2);

            //2
            attribute = null;
            attribute = ControllerReflection.MethodExpectedAttribute<AuthorizeAttribute>("Active", countAdjustment + 2, testMessage: "Active");
            attribute.ShouldNotBeNull();
            attribute.ElementAt(0).Policy.ShouldBe(AccessCodes.ProjectAccess);
            ControllerReflection.MethodExpectedAttribute<AsyncStateMachineAttribute>("Active", countAdjustment + 2);

            //3
            attribute = null;
            attribute = ControllerReflection.MethodExpectedAttribute<AuthorizeAttribute>("RequiringManagerAttention", countAdjustment + 2, testMessage: "RequiringManagerAttention");
            attribute.ShouldNotBeNull();
            attribute.ElementAt(0).Policy.ShouldBe(AccessCodes.SupervisorAccess);
            ControllerReflection.MethodExpectedAttribute<AsyncStateMachineAttribute>("RequiringManagerAttention", countAdjustment + 2);

            //4
            ControllerReflection.MethodExpectedAttribute<AsyncStateMachineAttribute>("RequiringPIAttention", countAdjustment + 2, testMessage: "RequiringPIAttention");
            var routeAttribute = ControllerReflection.MethodExpectedAttribute<RouteAttribute>("RequiringPIAttention", countAdjustment + 2, testMessage: "RequiringPIAttention");
            routeAttribute.ElementAt(0).Template.ShouldBe("/api/{controller}/{action}");

            //5
            ControllerReflection.MethodExpectedAttribute<AsyncStateMachineAttribute>("GetMine", countAdjustment + 2, testMessage: "GetMine");
            routeAttribute = ControllerReflection.MethodExpectedAttribute<RouteAttribute>("GetMine", countAdjustment + 2, testMessage: "GetMine");
            routeAttribute.ElementAt(0).Template.ShouldBe("/api/{controller}/{action}");

            //6
            attribute = null;
            attribute = ControllerReflection.MethodExpectedAttribute<AuthorizeAttribute>("Get", countAdjustment + 3, testMessage: "Get");
            attribute.ShouldNotBeNull();
            attribute.ElementAt(0).Policy.ShouldBe(AccessCodes.InvoiceAccess);
            ControllerReflection.MethodExpectedAttribute<AsyncStateMachineAttribute>("Get", countAdjustment + 3);
            routeAttribute = ControllerReflection.MethodExpectedAttribute<RouteAttribute>("Get", countAdjustment + 3, testMessage: "Get");
            routeAttribute.ElementAt(0).Template.ShouldBe("/api/{team}/Project/Get/{projectId}/{shareId?}");

            //7
            attribute = null;
            attribute = ControllerReflection.MethodExpectedAttribute<AuthorizeAttribute>("GetFields", countAdjustment + 3, testMessage: "GetFields");
            attribute.ShouldNotBeNull();
            attribute.ElementAt(0).Policy.ShouldBe(AccessCodes.SupervisorAccess);
            ControllerReflection.MethodExpectedAttribute<AsyncStateMachineAttribute>("GetFields", countAdjustment + 3);
            ControllerReflection.MethodExpectedAttribute<HttpGetAttribute>("GetFields", countAdjustment + 3);

            //8
            attribute = null;
            attribute = ControllerReflection.MethodExpectedAttribute<AuthorizeAttribute>("AccountApproval", countAdjustment + 4, testMessage: "AccountApproval");
            attribute.ShouldNotBeNull();
            attribute.ElementAt(0).Policy.ShouldBe(AccessCodes.FieldManagerAccess);
            ControllerReflection.MethodExpectedAttribute<AsyncStateMachineAttribute>("AccountApproval", countAdjustment + 4);
            ControllerReflection.MethodExpectedAttribute<HttpGetAttribute>("AccountApproval", countAdjustment + 4);
            ControllerReflection.MethodExpectedAttribute<ObsoleteAttribute>("AccountApproval", countAdjustment + 4);

            //9
            attribute = null;
            attribute = ControllerReflection.MethodExpectedAttribute<AuthorizeAttribute>("RefreshTotal", countAdjustment + 2, testMessage: "RefreshTotal");
            attribute.ShouldNotBeNull();
            attribute.ElementAt(0).Policy.ShouldBe(AccessCodes.FieldManagerAccess);
            ControllerReflection.MethodExpectedAttribute<AsyncStateMachineAttribute>("RefreshTotal", countAdjustment + 2);

            //10
            methodName = "GetCompleted";
            attribute = null;
            attribute = ControllerReflection.MethodExpectedAttribute<AuthorizeAttribute>(methodName, countAdjustment + 2, testMessage: "GetCompleted");
            attribute.ShouldNotBeNull();
            attribute.ElementAt(0).Policy.ShouldBe(AccessCodes.SupervisorAccess);
            ControllerReflection.MethodExpectedAttribute<AsyncStateMachineAttribute>(methodName, countAdjustment + 2);

            //11
            methodName = "CreateAdhoc";
            attribute = null;
            attribute = ControllerReflection.MethodExpectedAttribute<AuthorizeAttribute>(methodName, countAdjustment + 3, testMessage: "CreateAdHoc");
            attribute.ShouldNotBeNull();
            attribute.ElementAt(0).Policy.ShouldBe(AccessCodes.FieldManagerAccess);
            ControllerReflection.MethodExpectedAttribute<AsyncStateMachineAttribute>(methodName, countAdjustment + 3);
            ControllerReflection.MethodExpectedAttribute<HttpPostAttribute>(methodName, countAdjustment + 3);

            //12
            methodName = "ResetShareLink";
            attribute = null;
            attribute = ControllerReflection.MethodExpectedAttribute<AuthorizeAttribute>(methodName, countAdjustment + 3, testMessage: "ResetShareLink");
            attribute.ShouldNotBeNull();
            attribute.ElementAt(0).Policy.ShouldBe(AccessCodes.PrincipalInvestigatorOnly);
            ControllerReflection.MethodExpectedAttribute<AsyncStateMachineAttribute>(methodName, countAdjustment + 3);
            ControllerReflection.MethodExpectedAttribute<HttpPostAttribute>(methodName, countAdjustment + 3);

            //13
            methodName = "ListHistory";
            attribute = null;
            attribute = ControllerReflection.MethodExpectedAttribute<AuthorizeAttribute>(methodName, countAdjustment + 3, testMessage: "ListHistory");
            attribute.ShouldNotBeNull();
            attribute.ElementAt(0).Policy.ShouldBe(AccessCodes.InvoiceAccess);
            ControllerReflection.MethodExpectedAttribute<AsyncStateMachineAttribute>(methodName, countAdjustment + 3);
            ControllerReflection.MethodExpectedAttribute<HttpGetAttribute>(methodName, countAdjustment + 3);

            //14
            methodName = "AddProjectPermission";
            attribute = null;
            attribute = ControllerReflection.MethodExpectedAttribute<AuthorizeAttribute>(methodName, countAdjustment + 3, testMessage: "AddProjectPermission");
            attribute.ShouldNotBeNull();
            attribute.ElementAt(0).Policy.ShouldBe(AccessCodes.PrincipalInvestigatorOnly);
            ControllerReflection.MethodExpectedAttribute<AsyncStateMachineAttribute>(methodName, countAdjustment + 3);
            ControllerReflection.MethodExpectedAttribute<HttpPostAttribute>(methodName, countAdjustment + 3);

            //15
            methodName = "RemoveProjectPermission";
            attribute = null;
            attribute = ControllerReflection.MethodExpectedAttribute<AuthorizeAttribute>(methodName, countAdjustment + 3, testMessage: "RemoveProjectPermission");
            attribute.ShouldNotBeNull();
            attribute.ElementAt(0).Policy.ShouldBe(AccessCodes.PrincipalInvestigatorOnly);
            ControllerReflection.MethodExpectedAttribute<AsyncStateMachineAttribute>(methodName, countAdjustment + 3);
            ControllerReflection.MethodExpectedAttribute<HttpPostAttribute>(methodName, countAdjustment + 3);

            //16
            methodName = "GetPendingChangeRequests";
            ControllerReflection.MethodExpectedAttribute<AsyncStateMachineAttribute>(methodName, countAdjustment + 2);
            ControllerReflection.MethodExpectedAttribute<HttpGetAttribute>(methodName, countAdjustment + 2);
        }
    }

}
