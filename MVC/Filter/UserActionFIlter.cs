using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Filters;

namespace MVC
{
    public class UserActionFilter : IActionFilter
    {
        public void OnActionExecuting(ActionExecutingContext context)
        {
            // Check if the user has the required role
            var role = context.HttpContext.Session.GetString("Role");
            var controller = context.RouteData.Values["controller"]?.ToString();
            var action = context.RouteData.Values["action"]?.ToString();

            // skip filter for login page
            if (controller == "User" && (action == "Login" || action == "Register"))
            {
                return;
            }

            if (role != "User")
            {
                context.Result = new Microsoft.AspNetCore.Mvc.RedirectToActionResult("Login", "User", null);
            }
        }

        public void OnActionExecuted(ActionExecutedContext context)
        {
            // Implementation for after action execution
        }
    }
}