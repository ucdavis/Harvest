using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Harvest.Web.Controllers;
using Microsoft.AspNetCore.Mvc;
using Shouldly;
using TestHelpers.Helpers;
using Xunit;
using Xunit.Abstractions;

namespace Test.TestsControllers
{
    [Trait("Category", "ControllerTests")]
    public class AccountControllerTests
    {
        //TODO: If Ever....
    }

    [Trait("Category", "ControllerReflectionTests")]
    public class AccountControllerReflectionTests
    {
        private readonly ITestOutputHelper output;
        public ControllerReflection ControllerReflection;

        public AccountControllerReflectionTests(ITestOutputHelper output)
        {
            this.output = output;
            ControllerReflection = new ControllerReflection(this.output, typeof(AccountController));
        }

        #region Controller Class Tests
        [Fact]
        public void TestControllerClassAttributes()
        {
            ControllerReflection.ControllerInherits("Controller");
            ControllerReflection.ClassExpectedAttribute<ControllerAttribute>(2);
            var attributes = ControllerReflection.ClassExpectedAttribute<ApiExplorerSettingsAttribute>(2, showListOfAttributes:true);
            attributes.ElementAt(0).IgnoreApi.ShouldBe(true);
        }

        #endregion Controller Class Tests

        #region Controller Method Tests

        [Fact]
        public void TestControllerContainsExpectedNumberOfPublicMethods()
        {
            ControllerReflection.ControllerPublicMethods(3);
        }

        [Fact]
        public void TestControllerMethodAttributes()
        {
#if DEBUG
            var countAdjustment = 1;
#else
            var countAdjustment = 0;
#endif

            ControllerReflection.MethodExpectedNoAttribute("AccessDenied");
            ControllerReflection.MethodExpectedAttribute<AsyncStateMachineAttribute>("Logout", countAdjustment + 1, showListOfAttributes:true);
            ControllerReflection.MethodExpectedAttribute<AsyncStateMachineAttribute>("EndEmulate", countAdjustment + 1);
        }

        #endregion Controller Method Tests
        }
    }
