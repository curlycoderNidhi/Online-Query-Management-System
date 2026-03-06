using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MVC.Filters;
using Repository.Interfaces;
using Repository.Models;
using Repository.Models.Enums;

namespace MVC.Controllers
{
    // [Route("[controller]")]
    // [ServiceFilter(typeof(EmployeeRoleFilter))]
    public class EmployeeController : Controller
    {
        private readonly IEmployeeInterface _employee;

        public EmployeeController(IEmployeeInterface employee)
        {
            _employee = employee;
        }

        public IActionResult Index()
        {
            return View();
        }

        private int? GetCurrentEmployeeId()
        {
            if (int.TryParse(HttpContext.Session.GetString("empid"), out int empid) && empid > 0)
                return empid;
            return null;
        }

        [ServiceFilter(typeof(EmployeeRoleFilter))]

        public async Task<IActionResult> Dashboard()
        {
            var empid = GetCurrentEmployeeId();
            if (!empid.HasValue)
                return RedirectToAction("Login", "Employee");

            ViewBag.Resolved = await _employee.GetResolvedCount(empid.Value);
            ViewBag.Pending = await _employee.GetPendingCount(empid.Value);
            ViewBag.Assigned = await _employee.GetAssignedCount(empid.Value);
            ViewBag.TodayResolved = await _employee.GetTodayResolvedCount(empid.Value);
            ViewBag.PerformancePercent = ViewBag.Assigned > 0
                ? Math.Round((double)ViewBag.Resolved * 100.0 / (double)ViewBag.Assigned, 2)
                : 0;

            return View();
        }

        [ServiceFilter(typeof(EmployeeRoleFilter))]

        public IActionResult MyQueries()
        {
            if (!GetCurrentEmployeeId().HasValue)
                return RedirectToAction("Login", "Employee");

            return View();
        }

        [HttpGet]
        [ServiceFilter(typeof(EmployeeRoleFilter))]

        public async Task<IActionResult> UpdateQuery(int id)
        {
            var empid = GetCurrentEmployeeId();
            if (!empid.HasValue)
                return Unauthorized("Session expired. Please login again.");

            var queries = await _employee.GetEmployeeQueries(empid.Value);
            var query = queries.FirstOrDefault(q => q.QueryId == id);
            if (query is null)
                return NotFound("Query not found.");

            return PartialView("UpdateQuery", query);
        }

        [ServiceFilter(typeof(EmployeeRoleFilter))]

        public async Task<IActionResult> GetMyQueries()
        {
            var empid = GetCurrentEmployeeId();
            if (!empid.HasValue)
                return Unauthorized(new { success = false, message = "Session expired. Please login again." });

            var queries = await _employee.GetEmployeeQueries(empid.Value);

            return Json(queries);
        }

        [HttpPost]
        [ServiceFilter(typeof(EmployeeRoleFilter))]

        public async Task<IActionResult> UpdateQueryStatus(Query model)
        {
            if (!GetCurrentEmployeeId().HasValue)
                return Unauthorized(new { success = false, message = "Session expired. Please login again." });

            var updated = await _employee.UpdateQueryStatus(model);
            if (!updated)
                return BadRequest(new { success = false, message = "Unable to update query." });

            return Json(new { success = true });
        }

        [HttpGet("/Home/Login")]
        public IActionResult Login()
        {
            // if (GetCurrentEmployeeId().HasValue)
            // {
            //     var role = HttpContext.Session.GetString("role");
            //     if (string.Equals(role, "admin", StringComparison.OrdinalIgnoreCase))
            //         return RedirectToAction("Dashboard", "Admin");
            //     if (string.Equals(role, "employee", StringComparison.OrdinalIgnoreCase))
            //         return RedirectToAction("Dashboard", "Employee");
            // }

            return View("~/Views/Home/Login.cshtml", new LoginModel());
        }

        [HttpPost("/Home/Login")]
        public async Task<IActionResult> Login(LoginModel model)
        {
            if (!ModelState.IsValid)
                return View("~/Views/Home/Login.cshtml", model);

            var employee = await _employee.Login(model.EmpName, model.Password ?? string.Empty);
            if (employee is null)
            {
                ModelState.AddModelError(string.Empty, "Invalid employee name or password.");
                return View("~/Views/Home/Login.cshtml", model);
            }

            HttpContext.Session.SetString("empid", employee.EmpId.ToString());
            HttpContext.Session.SetString("empname", employee.EmpName);
            HttpContext.Session.SetString("role", employee.Role.ToString());

            if (employee.Role == Repository.Models.Enums.Role.admin)
                return RedirectToAction("Dashboard", "Admin");

            if (employee.Role == Repository.Models.Enums.Role.employee)
                return RedirectToAction("Dashboard", "Employee");

            ModelState.AddModelError(string.Empty, "Only admin and employee login is supported.");
            HttpContext.Session.Clear();
            return View("~/Views/Home/Login.cshtml", model);
        }

        [HttpPost("/Home/Logout")]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login", "Employee");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View("Error");
        }
    }
}
