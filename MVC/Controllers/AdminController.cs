using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Repositories.Interfaces;
using Repository.Models;

namespace MVC.Controllers
{
    public class AdminController : Controller
    {
        private readonly ILogger<AdminController> _logger;
        private readonly IAdminInterface _adminRepo;

        public AdminController(ILogger<AdminController> logger, IAdminInterface adminRepo)
        {
            _logger = logger;
            _adminRepo = adminRepo;
        }

        public IActionResult Index() => View();

        [HttpGet]
        public async Task<IActionResult> AdminDashboard()
        {
            ViewBag.AllQueries = await _adminRepo.GetAllQueries();
            ViewBag.SolvedQueries = await _adminRepo.GetAllQueriesSolved();
            ViewBag.OpenQueries = await _adminRepo.GetAllQueriesOpen();
            ViewBag.ProgressQueries = await _adminRepo.GetAllQueriesInProgress();
            ViewBag.Cards = await _adminRepo.GetDashboardCards();
            ViewBag.EmpPerformance = await _adminRepo.GetEmployeePerformance();
            ViewBag.Employees = await _adminRepo.GetAllEmployees();

            return View();
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

        public IActionResult Error() => View();
    }
}
