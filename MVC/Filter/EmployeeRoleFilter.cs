using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Filters;

namespace MVC
{
    public class EmployeeRoleFilter : IActionFilter
    {
        public void OnActionExecuting(ActionExecutingContext context)
        {
                    var role = context.HttpContext.Session.GetString("Role");
            var controller = context.RouteData.Values["controller"]?.ToString();
            var action = context.RouteData.Values["action"]?.ToString();
             if (controller == "Employee" && action == "Login")
            {
                return;
            }


            if (role != "employee")
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