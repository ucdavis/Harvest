using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Filters;
using Serilog;

namespace Harvest.Web.Middleware
{
    public class SerilogControllerActionFilter: IActionFilter
    {
        private readonly IDiagnosticContext _diagnosticContext;
        public SerilogControllerActionFilter(IDiagnosticContext diagnosticContext)
        {
            _diagnosticContext = diagnosticContext;
        }

        public void OnActionExecuting(ActionExecutingContext context)
        {
            _diagnosticContext.Set("RouteData", context.ActionDescriptor.RouteValues);
            _diagnosticContext.Set("ActionName", context.ActionDescriptor.DisplayName);
            _diagnosticContext.Set("ActionId", context.ActionDescriptor.Id);
            _diagnosticContext.Set("ValidationState", context.ModelState.IsValid);
        }

        public void OnActionExecuted(ActionExecutedContext context)
        {
            
        }
    }
}
