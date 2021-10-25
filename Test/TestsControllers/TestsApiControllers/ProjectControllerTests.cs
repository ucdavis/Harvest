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
            ControllerReflection.ControllerPublicMethods(9);
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
            var attribute = ControllerReflection.MethodExpectedAttribute<AuthorizeAttribute>("All", countAdjustment + 2);
            attribute.ShouldNotBeNull();
            attribute.ElementAt(0).Policy.ShouldBe(AccessCodes.WorkerAccess);
            ControllerReflection.MethodExpectedAttribute<AsyncStateMachineAttribute>("All", countAdjustment + 2);

            //2
            attribute = null;
            attribute = ControllerReflection.MethodExpectedAttribute<AuthorizeAttribute>("Active", countAdjustment + 2);
            attribute.ShouldNotBeNull();
            attribute.ElementAt(0).Policy.ShouldBe(AccessCodes.WorkerAccess);
            ControllerReflection.MethodExpectedAttribute<AsyncStateMachineAttribute>("Active", countAdjustment + 2);

            //3
            attribute = null;
            attribute = ControllerReflection.MethodExpectedAttribute<AuthorizeAttribute>("RequiringManagerAttention", countAdjustment + 2);
            attribute.ShouldNotBeNull();
            attribute.ElementAt(0).Policy.ShouldBe(AccessCodes.SupervisorAccess);
            ControllerReflection.MethodExpectedAttribute<AsyncStateMachineAttribute>("RequiringManagerAttention", countAdjustment + 2);

            //4
            ControllerReflection.MethodExpectedAttribute<AsyncStateMachineAttribute>("RequiringPIAttention", countAdjustment + 1);

            //5
            ControllerReflection.MethodExpectedAttribute<AsyncStateMachineAttribute>("GetMine", countAdjustment + 1);

            //6
            attribute = null;
            attribute = ControllerReflection.MethodExpectedAttribute<AuthorizeAttribute>("Get", countAdjustment + 2);
            attribute.ShouldNotBeNull();
            attribute.ElementAt(0).Policy.ShouldBe(AccessCodes.PrincipalInvestigator);
            ControllerReflection.MethodExpectedAttribute<AsyncStateMachineAttribute>("Get", countAdjustment + 2);

            //7
            attribute = null;
            attribute = ControllerReflection.MethodExpectedAttribute<AuthorizeAttribute>("GetFields", countAdjustment + 3);
            attribute.ShouldNotBeNull();
            attribute.ElementAt(0).Policy.ShouldBe(AccessCodes.SupervisorAccess);
            ControllerReflection.MethodExpectedAttribute<AsyncStateMachineAttribute>("GetFields", countAdjustment + 3);
            ControllerReflection.MethodExpectedAttribute<HttpGetAttribute>("GetFields", countAdjustment + 3);

            //8
            attribute = null;
            attribute = ControllerReflection.MethodExpectedAttribute<AuthorizeAttribute>("AccountApproval", countAdjustment + 3);
            attribute.ShouldNotBeNull();
            attribute.ElementAt(0).Policy.ShouldBe(AccessCodes.FieldManagerAccess);
            ControllerReflection.MethodExpectedAttribute<AsyncStateMachineAttribute>("AccountApproval", countAdjustment + 3);
            ControllerReflection.MethodExpectedAttribute<HttpGetAttribute>("AccountApproval", countAdjustment + 3);

            //9
            attribute = null;
            attribute = ControllerReflection.MethodExpectedAttribute<AuthorizeAttribute>("RefreshTotal", countAdjustment + 2);
            attribute.ShouldNotBeNull();
            attribute.ElementAt(0).Policy.ShouldBe(AccessCodes.FieldManagerAccess);
            ControllerReflection.MethodExpectedAttribute<AsyncStateMachineAttribute>("RefreshTotal", countAdjustment + 2);

        }
    }

}
