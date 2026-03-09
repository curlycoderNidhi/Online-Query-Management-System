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
            // Check if the user has the required role
            var role = context.HttpContext.Session.GetString("role");
            if (role != "admin")
            {
                context.Result = new Microsoft.AspNetCore.Mvc.RedirectToActionResult("Login", "Home", null);
            }
        }

        public void OnActionExecuted(ActionExecutedContext context)
        {
            // Implementation for after action execution
        }
    }
}