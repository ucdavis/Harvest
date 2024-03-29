﻿using Harvest.Core.Models;
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
    public class SystemControllerTests
    {
    }

    [Trait("Category", "ControllerReflectionTests")]
    public class SystemControllerReflectionTests
    {
        private readonly ITestOutputHelper output;
        public ControllerReflection ControllerReflection;

        public SystemControllerReflectionTests(ITestOutputHelper output)
        {
            this.output = output;
            ControllerReflection = new ControllerReflection(this.output, typeof(SystemController));
        }

        [Fact]
        public void TestControllerClassAttributes()
        {
            ControllerReflection.ControllerInherits("SuperController");
            ControllerReflection.ClassExpectedAttribute<ControllerAttribute>(3);
            var attributes = ControllerReflection.ClassExpectedAttribute<AuthorizeAttribute>(3, showListOfAttributes: true);
            attributes.ElementAt(0).Policy.ShouldBe(AccessCodes.SystemAccess);

            ControllerReflection.ClassExpectedAttribute<AutoValidateAntiforgeryTokenAttribute>(3);
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
#if DEBUG
            var countAdjustment = 1;
#else
            var countAdjustment = 0;
#endif

            //1 -1 
            ControllerReflection.MethodExpectedAttribute<HttpGetAttribute>("Emulate", countAdjustment + 0);

            //1 -2 
            ControllerReflection.MethodExpectedAttribute<HttpPostAttribute>("Emulate", countAdjustment + 2, isSecondMethod: true);
            ControllerReflection.MethodExpectedAttribute<AsyncStateMachineAttribute>("Emulate", countAdjustment + 2, isSecondMethod: true);

            ControllerReflection.MethodExpectedAttribute<HttpGetAttribute>("UpdatePendingExpenses", countAdjustment + 2);
            ControllerReflection.MethodExpectedAttribute<AsyncStateMachineAttribute>("UpdatePendingExpenses", countAdjustment + 2);

            ControllerReflection.MethodExpectedAttribute<HttpPostAttribute>("UpdatePendingExpenses", countAdjustment + 2, isSecondMethod: true);
            ControllerReflection.MethodExpectedAttribute<AsyncStateMachineAttribute>("UpdatePendingExpenses", countAdjustment + 2, isSecondMethod: true);

        }
    }
    }
