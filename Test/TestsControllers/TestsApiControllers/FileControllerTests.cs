using Harvest.Web.Controllers.Api;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shouldly;
using System.Linq;
using TestHelpers.Helpers;
using Xunit;
using Xunit.Abstractions;

namespace Test.TestsControllers.TestsApiControllers
{
    [Trait("Category", "ControllerTests")]
    public class FileControllerTests
    {
    }

    [Trait("Category", "ControllerReflectionTests")]
    public class FileControllerReflectionTests
    {
        private readonly ITestOutputHelper output;
        public ControllerReflection ControllerReflection;

        public FileControllerReflectionTests(ITestOutputHelper output)
        {
            this.output = output;
            ControllerReflection = new ControllerReflection(this.output, typeof(FileController));
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
            ControllerReflection.ControllerPublicMethods(2);
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
            ControllerReflection.MethodExpectedAttribute<HttpGetAttribute>("GetUploadDetails", countAdjustment + 1);
            var routeAttributes = ControllerReflection.MethodExpectedAttribute<RouteAttribute>("GetUploadDetails", countAdjustment + 1);
            routeAttributes.ElementAt(0).Template.ShouldBe("/api/{controller}/{action}");

            //2
            ControllerReflection.MethodExpectedAttribute<HttpGetAttribute>("GetReadDetails", countAdjustment + 1);
            routeAttributes = ControllerReflection.MethodExpectedAttribute<RouteAttribute>("GetReadDetails", countAdjustment + 1);
            routeAttributes.ElementAt(0).Template.ShouldBe("/api/{controller}/{action}");
        }
    }
}
