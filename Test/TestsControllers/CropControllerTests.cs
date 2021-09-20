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
    public class CropControllerTests
    {
    }

    [Trait("Category", "ControllerReflectionTests")]
    public class CropControllerReflectionTests
    {
        private readonly ITestOutputHelper output;
        public ControllerReflection ControllerReflection;

        public CropControllerReflectionTests(ITestOutputHelper output)
        {
            this.output = output;
            ControllerReflection = new ControllerReflection(this.output, typeof(CropController));
        }

        [Fact]
        public void TestControllerClassAttributes()
        {
            ControllerReflection.ControllerInherits("SuperController");
            ControllerReflection.ClassExpectedAttribute<ControllerAttribute>(3);
            var attributes = ControllerReflection.ClassExpectedAttribute<AuthorizeAttribute>(3, showListOfAttributes: true);
            attributes.ElementAt(0).Policy.ShouldBeNull();

            ControllerReflection.ClassExpectedAttribute<AutoValidateAntiforgeryTokenAttribute>(3);
            ControllerReflection.ClassExpectedAttribute<ControllerAttribute>(3);
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
            var attributes = ControllerReflection.MethodExpectedAttribute<AuthorizeAttribute>("Index", countAdjustment + 2);
            attributes.ShouldNotBeNull();
            attributes.ElementAt(0).Policy.ShouldBe(AccessCodes.FieldManagerAccess);
            ControllerReflection.MethodExpectedAttribute<AsyncStateMachineAttribute>("Index", countAdjustment + 2);

            //2 - 1
            attributes = null;
            attributes = ControllerReflection.MethodExpectedAttribute<AuthorizeAttribute>("Create", countAdjustment + 0, isSecondMethod: false );
            attributes.ShouldNotBeNull();
            attributes.ElementAt(0).Policy.ShouldBe(AccessCodes.FieldManagerAccess);

            //2-2
            attributes = null;
            attributes = ControllerReflection.MethodExpectedAttribute<AuthorizeAttribute>("Create", countAdjustment + 3, isSecondMethod: true);
            attributes.ShouldNotBeNull();
            attributes.ElementAt(0).Policy.ShouldBe(AccessCodes.FieldManagerAccess);
            ControllerReflection.MethodExpectedAttribute<AsyncStateMachineAttribute>("Create", countAdjustment + 3, isSecondMethod:true);
            ControllerReflection.MethodExpectedAttribute<HttpPostAttribute>("Create", countAdjustment + 3, isSecondMethod: true);

            //3 - 1
            attributes = null;
            attributes = ControllerReflection.MethodExpectedAttribute<AuthorizeAttribute>("Edit", countAdjustment + 2, isSecondMethod: false);
            attributes.ShouldNotBeNull();
            attributes.ElementAt(0).Policy.ShouldBe(AccessCodes.FieldManagerAccess);
            ControllerReflection.MethodExpectedAttribute<AsyncStateMachineAttribute>("Edit", countAdjustment + 2, isSecondMethod: false);

            //3-2
            attributes = null;
            attributes = ControllerReflection.MethodExpectedAttribute<AuthorizeAttribute>("Edit", countAdjustment + 3, isSecondMethod: true);
            attributes.ShouldNotBeNull();
            attributes.ElementAt(0).Policy.ShouldBe(AccessCodes.FieldManagerAccess);
            ControllerReflection.MethodExpectedAttribute<AsyncStateMachineAttribute>("Edit", countAdjustment + 3, isSecondMethod: true);
            ControllerReflection.MethodExpectedAttribute<HttpPostAttribute>("Edit", countAdjustment + 3, isSecondMethod: true);

            //4 - 1
            attributes = null;
            attributes = ControllerReflection.MethodExpectedAttribute<AuthorizeAttribute>("Delete", countAdjustment + 2, isSecondMethod: false);
            attributes.ShouldNotBeNull();
            attributes.ElementAt(0).Policy.ShouldBe(AccessCodes.FieldManagerAccess);
            ControllerReflection.MethodExpectedAttribute<AsyncStateMachineAttribute>("Delete", countAdjustment + 2, isSecondMethod: false);

            //4-2
            attributes = null;
            attributes = ControllerReflection.MethodExpectedAttribute<AuthorizeAttribute>("Delete", countAdjustment + 3, isSecondMethod: true);
            attributes.ShouldNotBeNull();
            attributes.ElementAt(0).Policy.ShouldBe(AccessCodes.FieldManagerAccess);
            ControllerReflection.MethodExpectedAttribute<AsyncStateMachineAttribute>("Delete", countAdjustment + 3, isSecondMethod: true);
            ControllerReflection.MethodExpectedAttribute<HttpPostAttribute>("Delete", countAdjustment + 3, isSecondMethod: true);

            //5
            ControllerReflection.MethodExpectedAttribute<AsyncStateMachineAttribute>("Search", countAdjustment + 1);
        }
    }
}
