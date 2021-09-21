using Harvest.Web.Controllers;
using Microsoft.AspNetCore.Mvc;
using TestHelpers.Helpers;
using Xunit;
using Xunit.Abstractions;

namespace Test.TestsControllers
{
    [Trait("Category", "ControllerTests")]
    public class ErrorControllerTests
    {
    }

    [Trait("Category", "ControllerReflectionTests")]
    public class ErrorControllerReflectionTests
    {
        private readonly ITestOutputHelper output;
        public ControllerReflection ControllerReflection;
        public ErrorControllerReflectionTests(ITestOutputHelper output)
        {
            this.output = output;
            ControllerReflection = new ControllerReflection(this.output, typeof(ErrorController));
        }

        [Fact]
        public void TestControllerClassAttributes()
        {
            ControllerReflection.ControllerInherits("Controller");
            ControllerReflection.ClassExpectedAttribute<ControllerAttribute>(1);
        }

        [Fact]
        public void TestControllerContainsExpectedNumberOfPublicMethods()
        {
            ControllerReflection.ControllerPublicMethods(1);
        }

        [Fact]
        public void TestControllerMethodAttributes()
        {
            ControllerReflection.MethodExpectedNoAttribute("Index");
        }
    }
}
