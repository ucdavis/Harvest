﻿using Harvest.Web.Controllers.Api;
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
    public class FinancialAccountControllerTests
    {
    }

    [Trait("Category", "ControllerReflectionTests")]
    public class FinancialAccountControllerReflectionTests
    {
        private readonly ITestOutputHelper output;
        public ControllerReflection ControllerReflection;

        public FinancialAccountControllerReflectionTests(ITestOutputHelper output)
        {
            this.output = output;
            ControllerReflection = new ControllerReflection(this.output, typeof(FinancialAccountController));
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
            ControllerReflection.ControllerPublicMethods(1);
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
            ControllerReflection.MethodExpectedAttribute<HttpGetAttribute>("Get", countAdjustment + 3);
            ControllerReflection.MethodExpectedAttribute<AsyncStateMachineAttribute>("Get", countAdjustment + 3);
            var routeAttribute = ControllerReflection.MethodExpectedAttribute<RouteAttribute>("Get", countAdjustment + 3);
            routeAttribute.ElementAt(0).Template.ShouldBe("/api/{controller}/{action}");
        }
    }
}
