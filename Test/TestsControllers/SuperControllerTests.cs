using Harvest.Web.Controllers;
using Microsoft.AspNetCore.Mvc;
using TestHelpers.Helpers;
using Xunit;
using Xunit.Abstractions;

namespace Test.TestsControllers
{
    [Trait("Category", "ControllerTests")]
    public class SuperControllerTests
    {
    }

    [Trait("Category", "ControllerReflectionTests")]
    public class SuperControllerReflectionTests
    {
        private readonly ITestOutputHelper output;
        public ControllerReflection ControllerReflection;

        public SuperControllerReflectionTests(ITestOutputHelper output)
        {
            this.output = output;
            ControllerReflection = new ControllerReflection(this.output, typeof(SuperController));
        }

        [Fact]
        public void TestControllerClassAttributes()
        {
            ControllerReflection.ClassExpectedAttribute<AutoValidateAntiforgeryTokenAttribute>(2);
            ControllerReflection.ClassExpectedAttribute<ControllerAttribute>(2);
        }

        [Fact]
        public void TestControllerContainsExpectedNumberOfPublicMethods()
        {
            ControllerReflection.ControllerPublicMethods(4);
        }
    }
}
