using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Repositories.Interfaces;
using Repository.Models;
using Repository.Models.Enums;

namespace MVC.Controllers
{
    // [Route("[controller]")]
    public class AdminController : Controller
    {
        private readonly ILogger<AdminController> _logger;
        private readonly IAdminInterface _adminRepo;

        public AdminController(ILogger<AdminController> logger,IAdminInterface adminRepo)
        {
            _logger = logger;
            _adminRepo = adminRepo;
        }

        public IActionResult Index()
        {
            return View();
        }
        [HttpGet]
        public async Task<IActionResult> AdminDashboard()
        {
            ViewBag.AllQueries      = await _adminRepo.GetAllQueries();
            ViewBag.SolvedQueries   = await _adminRepo.GetAllQueriesSolved();
            ViewBag.OpenQueries     = await _adminRepo.GetAllQueriesOpen();
            ViewBag.ProgressQueries = await _adminRepo.GetAllQueriesInProgress();
            ViewBag.Cards           = await _adminRepo.GetDashboardCards();
            ViewBag.EmpPerformance  = await _adminRepo.GetEmployeePerformance();

            return View();
        }    
        [HttpPost]
        public async Task<IActionResult> AssignEmployee(int queryId, int empId)
        {
            try
            {
                var result = await _adminRepo.AssignEmployee(queryId, empId);
                if (result > 0)
                    return Json(new { success = true,  message = "Employee assigned successfully." });
                else
                    return Json(new { success = false, message = "Assignment failed." });
            }
            catch (Exception e)
            {
                return Json(new { success = false, message = e.Message });
            }
        }


        [HttpPost]
        public async Task<IActionResult> UnassignEmployee(int queryId)
        {
            try
            {
                // Sets empId = 0 / null to unassign
                var result = await _adminRepo.AssignEmployee(queryId, 0);
                if (result > 0)
                    return Json(new { success = true,  message = "Employee unassigned." });
                else
                    return Json(new { success = false, message = "Unassign failed." });
            }
            catch (Exception e)
            {
                return Json(new { success = false, message = e.Message });
            }
        }

        

        [HttpGet]
        public IActionResult GetAllQueriesSolved()
        {
            try
            {
                var queries = _adminRepo.GetAllQueriesSolved();
                return Ok(queries);
            }
            catch(Exception e)
            {
                return StatusCode(500, e.Message);
            }
        }

        [HttpGet]
        public IActionResult GetAllQueriesInProgress()
        {
            try
            {
                var queries = _adminRepo.GetAllQueriesInProgress();
                return Ok(queries);
            }
            catch(Exception e)
            {
                return StatusCode(500, e.Message);
            }
        }



        [HttpGet]
        public IActionResult GetAllQueriesOpen()
        {
            try
            {
                var queries = _adminRepo.GetAllQueriesOpen();
                return Ok(queries);
            }
            catch(Exception e)
            {
                return StatusCode(500, e.Message);
            }
        }


        [HttpGet]
        public IActionResult GetAllQueries()
        {
            try
            {
                var queries = _adminRepo.GetAllQueries();
                return Ok(queries);
            }
            catch(Exception e)
            {
                return StatusCode(500, e.Message);
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetDashboardCards()
        {
            try
            {
                var cards = await _adminRepo.GetDashboardCards();
                return Ok(cards);
            }
            catch(Exception e)
            {
                return StatusCode(500,e.Message);
            }
        }


        [HttpGet]
        public async Task<IActionResult> GetEmployeePerformance()
        {
            try
            {
                var data = await _adminRepo.GetEmployeePerformance();
                return Ok(data);
            }
            catch(Exception e)
            {
                return StatusCode(500,e.Message);
            }
        }

        [HttpGet]
public async Task<IActionResult> GetAllUsers()
{
    try
    {
        var users = await _adminRepo.GetAllUsers();
        return Ok(users);
    }
    catch(Exception e)
    {
        return StatusCode(500,e.Message);
    }
}


[HttpGet]
public async Task<IActionResult> GetUserDetails(int id)
{
    try
    {
        var user = await _adminRepo.GetUserDetails(id);
        return Ok(user);
    }
    catch(Exception e)
    {
        return StatusCode(500,e.Message);
    }
}


[HttpGet]
public async Task<IActionResult> GetSubmittedQueries(int id)
{
    try
    {
        var queries = await _adminRepo.GetSubmittedQueries(id);
        return Ok(queries);
    }
    catch(Exception e)
    {
        return StatusCode(500,e.Message);
    }
}


// [HttpPost]
// public async Task<IActionResult> AssignEmployee(int queryId,int empId)
// {
//     try
//     {
//         var result = await _adminRepo.AssignEmployee(queryId,empId);

//         if(result > 0)
//             return Ok("Employee Assigned Successfully");
//         else
//             return BadRequest("Assignment Failed");
//     }
//     catch(Exception e)
//     {
//         return StatusCode(500,e.Message);
//     }
// }


        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View("Error!");
        }
    }
}