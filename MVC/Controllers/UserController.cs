using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Repository.Interfaces;
using Repository.Models;


namespace MVC.Controllers
{
    [Route("user")]
    [ServiceFilter(typeof(UserActionFilter))]
    public class UserController : Controller
    {
        private readonly IUserRepository _repo;

        public UserController(IUserRepository repo)
        {
            _repo = repo;
        }

        // ---------------- REGISTER ----------------

        [HttpGet("register")]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(User user)
        {
            if (!ModelState.IsValid)
                return View(user);

            int userId = await _repo.Register(user);

            if (userId > 0)
            {
                TempData["Success"] = "Registration Successful. Please Login.";
                return RedirectToAction("Login", "User");
            }

            ModelState.AddModelError("", "Registration failed");
            return View(user);
        }


        // ---------------- LOGIN ----------------

        [HttpGet("login")]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = await _repo.Login(model);

            if (user != null)
            {
                HttpContext.Session.SetInt32("UserId", user.UserId);
                HttpContext.Session.SetString("CompanyName", user.CompanyName);
                HttpContext.Session.SetString("Role", "User");

                return RedirectToAction("Dashboard", "User");
            }

            ModelState.AddModelError("", "Invalid email or password");
            return View(model);
        }


        // ---------------- DASHBOARD ----------------

        [HttpGet("dashboard")]
        public async Task<IActionResult> Dashboard()
        {
            int? userId = HttpContext.Session.GetInt32("UserId");

            if (userId == null)
                return RedirectToAction("Login", "User");

            var queries = await _repo.GetUserQueries(userId.Value);

            return View(queries);
        }


        // ---------------- CREATE QUERY ----------------

        [HttpGet("create-query")]
        public IActionResult CreateQuery()
        {
            return View();
        }

        [HttpPost("create-query")]
        public async Task<IActionResult> CreateQuery(Query query)
        {
            int? userId = HttpContext.Session.GetInt32("UserId");

            if (userId == null)
                return RedirectToAction("Login", "User");

            query.UserId = userId.Value;

            int id = await _repo.SubmitQuery(query);

            if (id > 0)
            {
                TempData["Success"] = "Query Submitted Successfully";
                return RedirectToAction("Dashboard", "User");
            }

            ModelState.AddModelError("", "Failed to submit query");
            return View(query);
        }


        // ---------------- EDIT QUERY ----------------

        [HttpGet("edit-query/{id}")]
        public async Task<IActionResult> EditQuery(int id)
        {
            int? userId = HttpContext.Session.GetInt32("UserId");

            if (userId == null)
                return RedirectToAction("Login", "User");

            var queries = await _repo.GetUserQueries(userId.Value);

            var query = queries.FirstOrDefault(q => q.QueryId == id);

            if (query == null)
                return NotFound();

            return View(query);
        }

        [HttpPost("edit-query")]
        public async Task<IActionResult> EditQuery(Query query)
        {
            bool updated = await _repo.UpdateQuery(query);

            if (updated)
            {
                TempData["Success"] = "Query Updated";
                return RedirectToAction("Dashboard", "User");
            }

            ModelState.AddModelError("", "Cannot update solved query");
            return View(query);
        }


        // ---------------- DELETE QUERY ----------------

        [HttpPost("delete-query/{id}")]
        public async Task<IActionResult> DeleteQuery(int id)
        {
            bool deleted = await _repo.DeleteQuery(id);

            if (deleted)
                TempData["Success"] = "Query Deleted";
            else
                TempData["Error"] = "Cannot delete solved query";

            return RedirectToAction("Dashboard", "User");
        }


        // ---------------- LOGOUT ----------------

        [HttpGet("logout")]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login", "User");
        }
    }
}