using Harvest.Core.Models;
using Harvest.Web.Controllers.Api;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shouldly;
using System.Linq;
using System.Net.Mime;
using System.Runtime.CompilerServices;
using TestHelpers.Helpers;
using Xunit;
using Xunit.Abstractions;

namespace Test.TestsControllers.TestsApiControllers
{
    [Trait("Category", "ControllerTests")]
    public class ExpenseControllerTests
    {
    }

    [Trait("Category", "ControllerReflectionTests")]
    public class ExpenseControllerReflectionTests
    {
        private readonly ITestOutputHelper output;
        public ControllerReflection ControllerReflection;

        public ExpenseControllerReflectionTests(ITestOutputHelper output)
        {
            this.output = output;
            ControllerReflection = new ControllerReflection(this.output, typeof(ExpenseController));
        }

        [Fact]
        public void TestControllerClassAttributes()
        {
            ControllerReflection.ControllerInherits("SuperController");
            ControllerReflection.ClassExpectedAttribute<ControllerAttribute>(3, showListOfAttributes: true);
            var attributes = ControllerReflection.ClassExpectedAttribute<AuthorizeAttribute>(3, showListOfAttributes: true);
            attributes.ElementAt(0).Policy.ShouldBeNull();

            ControllerReflection.ClassExpectedAttribute<AutoValidateAntiforgeryTokenAttribute>(3);
            ControllerReflection.ClassExpectedAttribute<ControllerAttribute>(3);
        }

        [Fact]
        public void TestControllerContainsExpectedNumberOfPublicMethods()
        {
            ControllerReflection.ControllerPublicMethods(13);
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
            ControllerReflection.MethodExpectedAttribute<HttpPostAttribute>("Create", countAdjustment + 4, showListOfAttributes: true);
            var authAttribute = ControllerReflection.MethodExpectedAttribute<AuthorizeAttribute>("Create", countAdjustment + 4);
            authAttribute.ShouldNotBeNull();
            authAttribute.ElementAt(0).Policy.ShouldBe(AccessCodes.WorkerAccess);
            var consumesAtt = ControllerReflection.MethodExpectedAttribute<ConsumesAttribute>("Create", countAdjustment + 4);
            consumesAtt.ShouldNotBeNull();
            consumesAtt.ElementAt(0).ContentTypes.Count.ShouldBe(1);
            consumesAtt.ElementAt(0).ContentTypes[0].ShouldBe(MediaTypeNames.Application.Json);
            ControllerReflection.MethodExpectedAttribute<AsyncStateMachineAttribute>("Create", countAdjustment + 4);

            //2
            ControllerReflection.MethodExpectedAttribute<HttpPostAttribute>("Delete", countAdjustment + 3);
            authAttribute = ControllerReflection.MethodExpectedAttribute<AuthorizeAttribute>("Delete", countAdjustment + 3);
            authAttribute.ShouldNotBeNull();
            authAttribute.ElementAt(0).Policy.ShouldBe(AccessCodes.SupervisorAccess);
            ControllerReflection.MethodExpectedAttribute<AsyncStateMachineAttribute>("Delete", countAdjustment + 3);

            //3 This should probably do PI auth... 
            ControllerReflection.MethodExpectedAttribute<HttpGetAttribute>("GetUnbilled", countAdjustment + 4);
            ControllerReflection.MethodExpectedAttribute<AsyncStateMachineAttribute>("GetUnbilled", countAdjustment + 4);
            authAttribute = ControllerReflection.MethodExpectedAttribute<AuthorizeAttribute>("GetUnbilled", countAdjustment +4);
            authAttribute.ShouldNotBeNull();
            authAttribute.ElementAt(0).Policy.ShouldBe(AccessCodes.InvoiceAccess);
            var routeAttribute = ControllerReflection.MethodExpectedAttribute<RouteAttribute>("GetUnbilled", countAdjustment + 4);
            routeAttribute.ShouldNotBeNull();
            routeAttribute.ElementAt(0).Template.ShouldBe("/api/{team}/Expense/GetUnbilled/{projectId}/{shareId?}");
            //4
            ControllerReflection.MethodExpectedAttribute<HttpGetAttribute>("GetUnbilledTotal", countAdjustment + 2);
            ControllerReflection.MethodExpectedAttribute<AsyncStateMachineAttribute>("GetUnbilledTotal", countAdjustment + 2);

            //5
            ControllerReflection.MethodExpectedAttribute<HttpGetAttribute>("GetRecentExpensedProjects", countAdjustment + 3);
            authAttribute = ControllerReflection.MethodExpectedAttribute<AuthorizeAttribute>("GetRecentExpensedProjects", countAdjustment + 3);
            authAttribute.ShouldNotBeNull();
            authAttribute.ElementAt(0).Policy.ShouldBe(AccessCodes.WorkerAccess);
            ControllerReflection.MethodExpectedAttribute<AsyncStateMachineAttribute>("GetRecentExpensedProjects", countAdjustment + 3);

            //6
            ControllerReflection.MethodExpectedAttribute<HttpGetAttribute>("GetMyPendingExpenses", countAdjustment + 3);
            authAttribute = ControllerReflection.MethodExpectedAttribute<AuthorizeAttribute>("GetMyPendingExpenses", countAdjustment + 3);
            authAttribute.ShouldNotBeNull();
            authAttribute.ElementAt(0).Policy.ShouldBe(AccessCodes.SupervisorAccess);
            ControllerReflection.MethodExpectedAttribute<AsyncStateMachineAttribute>("GetMyPendingExpenses", countAdjustment + 3);

            //7
            ControllerReflection.MethodExpectedAttribute<HttpGetAttribute>("GetAllPendingExpenses", countAdjustment + 3);
            authAttribute = ControllerReflection.MethodExpectedAttribute<AuthorizeAttribute>("GetAllPendingExpenses", countAdjustment + 3);
            authAttribute.ShouldNotBeNull();
            authAttribute.ElementAt(0).Policy.ShouldBe(AccessCodes.FieldManagerAccess);
            ControllerReflection.MethodExpectedAttribute<AsyncStateMachineAttribute>("GetAllPendingExpenses", countAdjustment + 3);

           

            //8
            ControllerReflection.MethodExpectedAttribute<HttpPostAttribute>("ApproveMyWorkerExpenses", countAdjustment + 3);
            authAttribute = ControllerReflection.MethodExpectedAttribute<AuthorizeAttribute>("ApproveMyWorkerExpenses", countAdjustment + 3);
            authAttribute.ShouldNotBeNull();
            authAttribute.ElementAt(0).Policy.ShouldBe(AccessCodes.SupervisorAccess);
            ControllerReflection.MethodExpectedAttribute<AsyncStateMachineAttribute>("ApproveMyWorkerExpenses", countAdjustment + 3);

          

            //9
            ControllerReflection.MethodExpectedAttribute<HttpPostAttribute>("ApproveExpenses", countAdjustment + 3);
            authAttribute = ControllerReflection.MethodExpectedAttribute<AuthorizeAttribute>("ApproveExpenses", countAdjustment + 3);
            authAttribute.ShouldNotBeNull();
            authAttribute.ElementAt(0).Policy.ShouldBe(AccessCodes.FieldManagerAccess);
            ControllerReflection.MethodExpectedAttribute<AsyncStateMachineAttribute>("ApproveExpenses", countAdjustment + 3);

            //10
            ControllerReflection.MethodExpectedAttribute<HttpPostAttribute>("Edit", countAdjustment + 3);
            authAttribute = ControllerReflection.MethodExpectedAttribute<AuthorizeAttribute>("Edit", countAdjustment + 3);
            authAttribute.ShouldNotBeNull();
            authAttribute.ElementAt(0).Policy.ShouldBe(AccessCodes.SupervisorAccess);
            ControllerReflection.MethodExpectedAttribute<AsyncStateMachineAttribute>("Edit", countAdjustment + 3);

            //11
            ControllerReflection.MethodExpectedAttribute<HttpGetAttribute>("Get", countAdjustment + 4, showListOfAttributes: true);
            authAttribute = ControllerReflection.MethodExpectedAttribute<AuthorizeAttribute>("Get", countAdjustment + 4);
            authAttribute.ShouldNotBeNull();
            authAttribute.ElementAt(0).Policy.ShouldBe(AccessCodes.SupervisorAccess);
            ControllerReflection.MethodExpectedAttribute<AsyncStateMachineAttribute>("Get", countAdjustment + 4);
            routeAttribute = ControllerReflection.MethodExpectedAttribute<RouteAttribute>("Get", countAdjustment + 4);
            routeAttribute.ShouldNotBeNull();
            routeAttribute.ElementAt(0).Template.ShouldBe("/api/{team}/Expense/Get/{expenseId}");

            //12
            ControllerReflection.MethodExpectedAttribute<HttpGetAttribute>("GetApprovedExpenses", countAdjustment + 3);
            authAttribute = ControllerReflection.MethodExpectedAttribute<AuthorizeAttribute>("GetApprovedExpenses", countAdjustment + 3);
            authAttribute.ShouldNotBeNull();
            authAttribute.ElementAt(0).Policy.ShouldBe(AccessCodes.FieldManagerAccess);
            ControllerReflection.MethodExpectedAttribute<AsyncStateMachineAttribute>("GetApprovedExpenses", countAdjustment + 3);

            //13
            ControllerReflection.MethodExpectedAttribute<HttpGetAttribute>("GetAllBilled", countAdjustment + 4);
            authAttribute = ControllerReflection.MethodExpectedAttribute<AuthorizeAttribute>("GetAllBilled", countAdjustment + 4);
            authAttribute.ShouldNotBeNull();
            authAttribute.ElementAt(0).Policy.ShouldBe(AccessCodes.InvoiceAccess);
            ControllerReflection.MethodExpectedAttribute<AsyncStateMachineAttribute>("GetAllBilled", countAdjustment + 4);
            routeAttribute = ControllerReflection.MethodExpectedAttribute<RouteAttribute>("GetAllBilled", countAdjustment + 4);
            routeAttribute.ShouldNotBeNull();
            routeAttribute.ElementAt(0).Template.ShouldBe("/api/{team}/Expense/GetAllBilled/{projectId}/{shareId?}");
        }

    }
}
