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
    public class PermissionsControllerTests
    {
        //To Test if user has system access for Create, those tests would need to be added here
    }

    [Trait("Category", "ControllerReflectionTests")]
    public class PermissionsControllerReflectionTests
    {
        private readonly ITestOutputHelper output;
        public ControllerReflection ControllerReflection;

        public PermissionsControllerReflectionTests(ITestOutputHelper output)
        {
            this.output = output;
            ControllerReflection = new ControllerReflection(this.output, typeof(PermissionsController));
        }

        [Fact]
        public void TestControllerClassAttributes()
        {
            ControllerReflection.ControllerInherits("SuperController");
            ControllerReflection.ClassExpectedAttribute<ControllerAttribute>(3);
            var attributes = ControllerReflection.ClassExpectedAttribute<AuthorizeAttribute>(3, showListOfAttributes: false);
            attributes.ElementAt(0).Policy.ShouldBe(AccessCodes.FieldManagerAccess);

            ControllerReflection.ClassExpectedAttribute<AutoValidateAntiforgeryTokenAttribute>(3);
            ControllerReflection.ClassExpectedAttribute<ControllerAttribute>(3);
        }

        [Fact]
        public void TestControllerContainsExpectedNumberOfPublicMethods()
        {
            ControllerReflection.ControllerPublicMethods(10);
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
            ControllerReflection.MethodExpectedAttribute<AsyncStateMachineAttribute>("Index", countAdjustment + 1);

            //2 - 1
            ControllerReflection.MethodExpectedAttribute<AsyncStateMachineAttribute>("Create", countAdjustment + 1, isSecondMethod: false);

            //2-2
            ControllerReflection.MethodExpectedAttribute<AsyncStateMachineAttribute>("Create", countAdjustment + 2, isSecondMethod: true);
            ControllerReflection.MethodExpectedAttribute<HttpPostAttribute>("Create", countAdjustment + 2, isSecondMethod: true);

            //3 - 1
            ControllerReflection.MethodExpectedAttribute<AsyncStateMachineAttribute>("Delete", countAdjustment + 1, isSecondMethod: false);

            //3-2
            ControllerReflection.MethodExpectedAttribute<AsyncStateMachineAttribute>("Delete", countAdjustment + 2, isSecondMethod: true);
            ControllerReflection.MethodExpectedAttribute<HttpPostAttribute>("Delete", countAdjustment + 2, isSecondMethod: true);

            //4
            ControllerReflection.MethodExpectedAttribute<AsyncStateMachineAttribute>("Details", countAdjustment + 2);
            ControllerReflection.MethodExpectedAttribute<HttpGetAttribute>("Details", countAdjustment + 2);

            //5-1
            ControllerReflection.MethodExpectedAttribute<AsyncStateMachineAttribute>("AddWorkerToSupervisor", countAdjustment + 2, isSecondMethod: false);
            ControllerReflection.MethodExpectedAttribute<HttpGetAttribute>("AddWorkerToSupervisor", countAdjustment + 2, isSecondMethod: false);

            //5-2
            ControllerReflection.MethodExpectedAttribute<AsyncStateMachineAttribute>("AddWorkerToSupervisor", countAdjustment + 2, isSecondMethod: true);
            ControllerReflection.MethodExpectedAttribute<HttpPostAttribute>("AddWorkerToSupervisor", countAdjustment + 2, isSecondMethod: true);

            //6-1
            ControllerReflection.MethodExpectedAttribute<AsyncStateMachineAttribute>("RemoveWorkerFromSupervisor", countAdjustment +2, isSecondMethod: false);
            ControllerReflection.MethodExpectedAttribute<HttpGetAttribute>("RemoveWorkerFromSupervisor", countAdjustment + 2, isSecondMethod: false);

            //6-2
            ControllerReflection.MethodExpectedAttribute<AsyncStateMachineAttribute>("RemoveWorkerFromSupervisor", countAdjustment + 2, isSecondMethod: true);
            ControllerReflection.MethodExpectedAttribute<HttpPostAttribute>("RemoveWorkerFromSupervisor", countAdjustment + 2, isSecondMethod: true);
        }
    }
}
