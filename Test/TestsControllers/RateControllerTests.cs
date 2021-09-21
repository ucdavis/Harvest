using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Harvest.Core.Models;
using Harvest.Web.Controllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shouldly;
using TestHelpers.Helpers;
using Xunit;
using Xunit.Abstractions;

namespace Test.TestsControllers
{
    [Trait("Category", "ControllerTests")]
    public class RateControllerTests
    {
    }

    [Trait("Category", "ControllerReflectionTests")]
    public class RateControllerReflectionTests
    {
        private readonly ITestOutputHelper output;
        public ControllerReflection ControllerReflection;

        public RateControllerReflectionTests(ITestOutputHelper output)
        {
            this.output = output;
            ControllerReflection = new ControllerReflection(this.output, typeof(RateController));
        }

        [Fact]
        public void TestControllerClassAttributes()
        {
            ControllerReflection.ControllerInherits("SuperController");
            ControllerReflection.ClassExpectedAttribute<ControllerAttribute>(3);
            var attributes = ControllerReflection.ClassExpectedAttribute<AuthorizeAttribute>(3, showListOfAttributes: false);
            attributes.ElementAt(0).Policy.ShouldBe(AccessCodes.WorkerAccess);

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

            //1 Possibly this should have a FieldManager role...
            ControllerReflection.MethodExpectedAttribute<AsyncStateMachineAttribute>("Index", countAdjustment + 1);

            //2
            ControllerReflection.MethodExpectedAttribute<AsyncStateMachineAttribute>("Active", countAdjustment + 1);

            //3
            ControllerReflection.MethodExpectedAttribute<AsyncStateMachineAttribute>("Details", countAdjustment + 1);

            //4 - 1
            var attributes = ControllerReflection.MethodExpectedAttribute<AuthorizeAttribute>("Create", countAdjustment + 0, isSecondMethod: false);
            attributes.ShouldNotBeNull();
            attributes.ElementAt(0).Policy.ShouldBe(AccessCodes.FieldManagerAccess);

            //4-2
            attributes = null;
            attributes = ControllerReflection.MethodExpectedAttribute<AuthorizeAttribute>("Create", countAdjustment + 3, isSecondMethod: true);
            attributes.ShouldNotBeNull();
            attributes.ElementAt(0).Policy.ShouldBe(AccessCodes.FieldManagerAccess);
            ControllerReflection.MethodExpectedAttribute<AsyncStateMachineAttribute>("Create", countAdjustment + 3, isSecondMethod: true);
            ControllerReflection.MethodExpectedAttribute<HttpPostAttribute>("Create", countAdjustment + 3, isSecondMethod: true);

            //5 - 1
            attributes = null;
            attributes = ControllerReflection.MethodExpectedAttribute<AuthorizeAttribute>("Edit", countAdjustment + 2, isSecondMethod: false);
            attributes.ShouldNotBeNull();
            attributes.ElementAt(0).Policy.ShouldBe(AccessCodes.FieldManagerAccess);
            ControllerReflection.MethodExpectedAttribute<AsyncStateMachineAttribute>("Edit", countAdjustment + 2, isSecondMethod: false);

            //5-2
            attributes = null;
            attributes = ControllerReflection.MethodExpectedAttribute<AuthorizeAttribute>("Edit", countAdjustment + 3, isSecondMethod: true);
            attributes.ShouldNotBeNull();
            attributes.ElementAt(0).Policy.ShouldBe(AccessCodes.FieldManagerAccess);
            ControllerReflection.MethodExpectedAttribute<AsyncStateMachineAttribute>("Edit", countAdjustment + 3, isSecondMethod: true);
            ControllerReflection.MethodExpectedAttribute<HttpPostAttribute>("Edit", countAdjustment + 3, isSecondMethod: true);

            //6 - 1
            attributes = null;
            attributes = ControllerReflection.MethodExpectedAttribute<AuthorizeAttribute>("Delete", countAdjustment + 2, isSecondMethod: false);
            attributes.ShouldNotBeNull();
            attributes.ElementAt(0).Policy.ShouldBe(AccessCodes.FieldManagerAccess);
            ControllerReflection.MethodExpectedAttribute<AsyncStateMachineAttribute>("Delete", countAdjustment + 2, isSecondMethod: false);

            //6-2
            attributes = null;
            attributes = ControllerReflection.MethodExpectedAttribute<AuthorizeAttribute>("Delete", countAdjustment + 3, isSecondMethod: true);
            attributes.ShouldNotBeNull();
            attributes.ElementAt(0).Policy.ShouldBe(AccessCodes.FieldManagerAccess);
            ControllerReflection.MethodExpectedAttribute<AsyncStateMachineAttribute>("Delete", countAdjustment + 3, isSecondMethod: true);
            ControllerReflection.MethodExpectedAttribute<HttpPostAttribute>("Delete", countAdjustment + 3, isSecondMethod: true);
        }

    }
}
