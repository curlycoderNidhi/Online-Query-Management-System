using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using repositories.Interfaces;

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
            _adminRepo = _adminRepo;
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



        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View("Error!");
        }
    }
}