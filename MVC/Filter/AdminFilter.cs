using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Filters;

namespace MVC.Filters
{
    public class AdminFilter : IActionFilter
    {
         public void OnActionExecuting(ActionExecutingContext context)
        {
            var controller = context.RouteData.Values["controller"]?.ToString();
            var action = context.RouteData.Values["action"]?.ToString();
            if (controller == "Admin" && string.Equals(action, "Login", StringComparison.OrdinalIgnoreCase))
                return;

            var role = context.HttpContext.Session.GetString("Role");
            if (!string.Equals(role, "admin", StringComparison.OrdinalIgnoreCase))
            {
                context.Result = new Microsoft.AspNetCore.Mvc.RedirectToActionResult("Login", "Employee", null);
            }
        }

        public void OnActionExecuted(ActionExecutedContext context)
        {
            // Implementation for after action execution
        }
    }
}
