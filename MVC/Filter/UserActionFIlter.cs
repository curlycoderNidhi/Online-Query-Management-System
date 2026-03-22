// using Microsoft.AspNetCore.Mvc;
// using Microsoft.AspNetCore.Mvc.Filters;

// namespace MVC
// {
//     public class UserActionFilter : IActionFilter
//     {
//         public void OnActionExecuting(ActionExecutingContext context)
//         {
//             var role = context.HttpContext.Session.GetString("Role");
//             var controller = context.RouteData.Values["controller"]?.ToString();
//             var action = context.RouteData.Values["action"]?.ToString();

//             // ✅ Allow public routes (VERY IMPORTANT FIX)
//             if (controller == "User" && (
//                 action == "Login" ||
//                 action == "Register" ||
//                 action == "VerifyOtp" ||
//                 action == "VerifyOtpForReset" ||   // 🔥 ADDED
//                 action == "ForgotPassword" ||
//                 action == "ResetPassword"
//             ))
//             {
//                 return;
//             }

//             // 🔐 Protect other routes
//             if (!string.Equals(role, "user", StringComparison.OrdinalIgnoreCase))
//             {
//                 context.Result = new RedirectToActionResult("Login", "User", null);
//             }
//         }

//         public void OnActionExecuted(ActionExecutedContext context)
//         {
//         }
//     }
// }


using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace MVC.Filters
{
    public class UserActionFilter : IActionFilter
    {
        // ── Routes that do NOT require login ──
        private static readonly HashSet<string> _publicActions = new(StringComparer.OrdinalIgnoreCase)
        {
            "Login",
            "Register",
            "VerifyOtp",
            "VerifyOtpForReset",
            "ForgotPassword",
            "ResetPassword",
            "ResendOtp"
        };

        public void OnActionExecuting(ActionExecutingContext context)
        {
            var controller = context.RouteData.Values["controller"]?.ToString();
            var action     = context.RouteData.Values["action"]?.ToString();

            // ── Only guard the User controller ──
            if (!string.Equals(controller, "User", StringComparison.OrdinalIgnoreCase))
                return;

            var response = context.HttpContext.Response;

            // ── Prevent ALL pages from being cached by the browser ──
            // This forces a fresh server request on every back/forward navigation
            // so the session check always runs and stale pages are never shown
            response.Headers["Cache-Control"] = "no-cache, no-store, must-revalidate, private";
            response.Headers["Pragma"]        = "no-cache";
            response.Headers["Expires"]       = "0";

            // ── Allow public actions through ──
            if (_publicActions.Contains(action ?? ""))
                return;

            // ── Check session for protected actions ──
            var role   = context.HttpContext.Session.GetString("Role");
            var userId = context.HttpContext.Session.GetInt32("UserId");

            if (!string.Equals(role, "user", StringComparison.OrdinalIgnoreCase) || userId == null)
            {
                context.Result = new RedirectToActionResult("Login", "User", null);
            }
        }

        public void OnActionExecuted(ActionExecutedContext context)
        {
            // nothing needed after execution
        }
    }
}