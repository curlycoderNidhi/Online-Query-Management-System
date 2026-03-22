using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Repositories.Interfaces;
using MVC.Filters;
using Repository.Models;
using Repository.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Repository;

namespace MVC.Controllers
{
    [ServiceFilter(typeof(AdminFilter))]
    public class AdminController : Controller
    {
        private readonly ILogger<AdminController> _logger;
        private readonly IAdminInterface _adminRepo;
        private readonly IEmployeeInterface _employee;

                    private readonly INotificationRedisService _redisService;

        private readonly IElasticSearchService _elastic;
       public AdminController(ILogger<AdminController> logger, IAdminInterface adminRepo, IEmployeeInterface employee , INotificationRedisService redisService , IElasticSearchService elastic)
        {
            _logger = logger;
            _adminRepo = adminRepo;
            _employee = employee;
            _redisService = redisService;
            _elastic = elastic;

        }

        public IActionResult Index() => View();

        [HttpGet("Login")]
        [AllowAnonymous]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost("Login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login(Repository.Models.EmployeeLoginModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var emp = await _employee.Login(model.EmpName, model.Password ?? string.Empty);
            if (emp == null || emp.Role != Repository.Models.Enums.Role.admin)
            {
                ModelState.AddModelError(string.Empty, "Invalid admin credentials.");
                return View(model);
            }

            HttpContext.Session.SetString("empid", emp.EmpId.ToString());
            HttpContext.Session.SetString("empname", emp.EmpName);
            HttpContext.Session.SetString("Role", emp.Role.ToString());

            return RedirectToAction("AdminDashboard");
        }

        [HttpGet]
        public async Task<IActionResult> AdminDashboard()
        {
               ViewBag.Count = await _redisService.GetNotificationCountAsync();
            ViewBag.Notifications = await _redisService.GetNotificationsAsync();

            System.Console.WriteLine(ViewBag.Count);
            System.Console.WriteLine(ViewBag.Notifications);

            foreach (var item in ViewBag.Notifications)
            {
                System.Console.WriteLine(item.ToString());
            }

            var all = await _adminRepo.GetAllQueries();
            ViewBag.AllQueries = all;
            var solved = await _adminRepo.GetAllQueriesSolved();
            var open = await _adminRepo.GetAllQueriesOpen();
            var progress = await _adminRepo.GetAllQueriesInProgress();
            ViewBag.SolvedQueries = solved;
            ViewBag.OpenQueries = open;
            ViewBag.ProgressQueries = progress;

            int totalCount = all.Count;
            int solvedCount = solved.Count;
            int openCount = open.Count;
            int pendingCount = progress.Count;

            var todayNow = DateTime.Today;
            int todayTotal = all.Count(q => q.QueryDate.Date == todayNow);
            int todaySolved = solved.Count(q => q.QueryDate.Date == todayNow);
            int todayOpen = open.Count(q => q.QueryDate.Date == todayNow);
            int todayPending = progress.Count(q => q.QueryDate.Date == todayNow);

            ViewBag.TotalCount = totalCount;
            ViewBag.SolvedCount = solvedCount;
            ViewBag.OpenCount = openCount;
            ViewBag.PendingCount = pendingCount;
            ViewBag.TodayTotal = todayTotal;
            ViewBag.TodaySolved = todaySolved;
            ViewBag.TodayOpen = todayOpen;
            ViewBag.TodayPending = todayPending;

            ViewBag.Cards = await _adminRepo.GetDashboardCards();

            var employees = await _adminRepo.GetAllEmployees();
            var perf = await _adminRepo.GetEmployeePerformance();
            var mergedPerf = employees
                .Where(e => !string.Equals(e.EmpName, "Admin", StringComparison.OrdinalIgnoreCase))
                .Select(e => new
                {
                    EmpName = e.EmpName,
                    Solved = perf.FirstOrDefault(p => p.EmployeeName == e.EmpName)?.ResolvedQueries ?? 0,
                    Total = all.Count(q => string.Equals(q.EmployeeName, e.EmpName, StringComparison.OrdinalIgnoreCase))
                })
                .ToList();

            ViewBag.EmpPerformance = mergedPerf;
            ViewBag.Employees = employees;
            ViewBag.Users = await _adminRepo.GetAllUsers();

            var today = DateTime.Today;
            ViewBag.TodayTotal = all.Count(q => q.QueryDate.Date == today);
            ViewBag.TodaySolved = solved.Count(q => q.QueryDate.Date == today);
            ViewBag.TodayOpen = open.Count(q => q.QueryDate.Date == today);
            ViewBag.TodayPending = progress.Count(q => q.QueryDate.Date == today);

            return View();
        }

        [HttpGet("Employees")]
        public async Task<IActionResult> Employees()
        {
            ViewBag.Employees = await _adminRepo.GetAllEmployees();
            return View();
        }

        [HttpPost("Employees")]
        public async Task<IActionResult> Employees(string empName, string password)
        {
            if (string.IsNullOrWhiteSpace(empName) || string.IsNullOrWhiteSpace(password))
            {
                ModelState.AddModelError(string.Empty, "Employee name and password are required.");
                ViewBag.Employees = await _adminRepo.GetAllEmployees();
                return View();
            }

            var employee = new Repository.Models.Employee
            {
                EmpName = empName.Trim(),
                Password = password,
                Role = Repository.Models.Enums.Role.employee
            };

            // Return type changed here
            var result = await _adminRepo.CreateEmployee(employee);
            if (result > 0)
            {
                // Fetch the newly created employee to get the real ID back
                var all = await _adminRepo.GetAllEmployees();
                var created = all.FirstOrDefault(e =>
                    string.Equals(e.EmpName, empName.Trim(), StringComparison.OrdinalIgnoreCase));
                return Json(new
                {
                    success = true,
                    empId = created?.EmpId ?? 0,
                    empName = created?.EmpName ?? empName.Trim()
                });
            }
            return Json(new { success = false, message = "Failed to create employee." });
        }

        [HttpPost]
        public async Task<IActionResult> AssignEmployee(int queryId, int empId)
        {
            try
            {
                var result = await _adminRepo.AssignEmployee(queryId, empId);
                if (result > 0)
                    return Json(new { success = true, message = "Employee assigned successfully!" });
                return Json(new { success = false, message = "Assignment failed!" });
            }
            catch (Exception e)
            {
                return Json(new { success = false, message = e.Message });
            }
        }
        
        [HttpGet]
        public async Task<IActionResult> SearchQueries(
            string? q,
            string? keyword,
            string? status,
            DateTime? fromDate,
            DateTime? toDate,
            string? date)
        {
            try
            {
                var searchText = string.IsNullOrWhiteSpace(q) ? keyword : q;

                if (!fromDate.HasValue && !toDate.HasValue
                    && !string.IsNullOrWhiteSpace(date)
                    && DateTime.TryParse(date, out var parsedExact))
                {
                    fromDate = parsedExact;
                    toDate   = parsedExact;
                }

                var results = await _elastic.AdminSearchAsync(searchText, status, fromDate, toDate);

                var mapped = results.Select(r => new
                {
                    queryId     = r.QueryId,
                    title       = r.Title       ?? "",
                    companyName = r.Username    ?? "",
                    priority    = r.Priority.ToString(),
                    status      = r.Status.ToString(),
                    queryDate   = r.QueryDate,
                    empId       = r.EmpId ?? 0,
                    assignedTo  = string.IsNullOrEmpty(r.EmployeeName) ? "Unassigned" : r.EmployeeName,
                    comments    = r.Comments    ?? ""
                });

                return Json(mapped);
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message });
            }
        }     

        public IActionResult Error() => View();
    }
}
