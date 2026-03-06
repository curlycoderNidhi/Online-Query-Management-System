using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Repositories.Interfaces;

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

        public IActionResult AdminDashBoard()
        {
            return View();
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


[HttpPost]
public async Task<IActionResult> AssignEmployee(int queryId,int empId)
{
    try
    {
        var result = await _adminRepo.AssignEmployee(queryId,empId);

        if(result > 0)
            return Ok("Employee Assigned Successfully");
        else
            return BadRequest("Assignment Failed");
    }
    catch(Exception e)
    {
        return StatusCode(500,e.Message);
    }
}


        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View("Error!");
        }
    }
}