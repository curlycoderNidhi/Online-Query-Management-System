using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Repository.Interfaces;
using Repository.Models;
using MVC.Models;
using MVC.Service;
using MVC.Filters;
using Repository;

namespace MVC.Controllers
{
    [Route("user")]
    [ServiceFilter(typeof(UserActionFilter))]
    [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
    public class UserController : Controller
    {
        private readonly IUserRepository       _userrepo;
        private readonly IQueryRepository      _queryrepo;
        private readonly EmailService          _emailService;
        private readonly OtpService            _otpService;
        private readonly EmailTemplateService  _templateService;
        private readonly IRabbitMqPublisher    _publisher;
        private readonly ElasticService _elasticService;

        public UserController(
            IUserRepository      repo,
            IQueryRepository     queryRepo,
            EmailService         emailService,
            OtpService           otpService,
            EmailTemplateService templateService,
            IRabbitMqPublisher   publisher,
             ElasticService elasticService )
        {
            _userrepo        = repo;
            _queryrepo       = queryRepo;
            _emailService    = emailService;
            _otpService      = otpService;
            _templateService = templateService;
            _publisher       = publisher;
            _elasticService = elasticService;
        }

        // ================================================================
        // REGISTER
        // ================================================================

        [HttpGet("register")]
        public IActionResult Register()
        {
            return View(new UserSignupViewModel());
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(UserSignupViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = new User
            {
                CompanyName = model.Email,
                Email       = model.Email,
                Password    = model.Password
            };

            int userId = await _userrepo.Register(user);

            if (userId > 0)
            {
                // Send welcome email
                string username = model.Email.Split('@')[0];
                string body     = _templateService.GetWelcomeEmailTemplate(username);

                await _emailService.SendEmailAsync(
                    model.Email,
                    "🎉 Welcome to Query Management System",
                    body
                );

                // Publish RabbitMQ notification
                try
                {
                    await _publisher.PublishAsync(new NotifcationMessage
                    {
                        Type     = "UserRegistered",
                        Username = user.CompanyName
                    });
                }
                catch (Exception e)
                {
                    Console.WriteLine("Notification failed: " + e.Message);
                }

                TempData["Success"] = "Registration Successful. Please Login.";
                return RedirectToAction("Login", "User");
            }

            ModelState.AddModelError("", "Registration failed");
            return View(model);
        }


        // ================================================================
        // LOGIN
        // ================================================================

        [HttpGet("login")]
        public IActionResult Login()
        {
            // Already logged in → go to dashboard
            var role   = HttpContext.Session.GetString("Role");
            var userId = HttpContext.Session.GetInt32("UserId");

            if (string.Equals(role, "user", StringComparison.OrdinalIgnoreCase) && userId != null)
                return RedirectToAction("Dashboard");

            return View();
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(UserLoginModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = await _userrepo.Login(model);

            if (user != null)
            {
                // Generate OTP and send email
                string otp      = _otpService.GenerateOtp(user.Email);
                string username = user.Email.Split('@')[0];
                string body     = _templateService.GetOtpEmailTemplate(otp, "Login", username);

                await _emailService.SendEmailAsync(user.Email, "🔐 Your OTP Code", body);

                // Store in TempData (for VerifyOtp GET/POST)
                TempData["Email"]       = user.Email;
                TempData["UserId"]      = user.UserId;
                TempData["CompanyName"] = user.CompanyName;
                TempData["OtpPurpose"]  = "Login";
                TempData.Keep();

                // Store in Session (backup for ResendOtp — survives View() returns)
                HttpContext.Session.SetString("OtpEmail",       user.Email);
                HttpContext.Session.SetString("OtpPurpose",     "Login");
                HttpContext.Session.SetInt32 ("OtpUserId",      user.UserId);
                HttpContext.Session.SetString("OtpCompanyName", user.CompanyName ?? "");

                return RedirectToAction("VerifyOtp");
            }

            ModelState.AddModelError("", "Invalid email or password");
            return View(model);
        }


        // ================================================================
        // VERIFY OTP
        // ================================================================

        [HttpGet("verify-otp")]
        public IActionResult VerifyOtp()
        {
            return View("VerifyOtp");
        }

        [HttpPost("verify-otp")]
        public IActionResult VerifyOtp(string otp)
        {
            // Read from TempData first, fall back to Session
            string email       = TempData["Email"]?.ToString()
                                 ?? HttpContext.Session.GetString("OtpEmail");
            string purpose     = TempData["OtpPurpose"]?.ToString()
                                 ?? HttpContext.Session.GetString("OtpPurpose");
            int?   userId      = TempData["UserId"] as int?
                                 ?? HttpContext.Session.GetInt32("OtpUserId");
            string companyName = TempData["CompanyName"]?.ToString()
                                 ?? HttpContext.Session.GetString("OtpCompanyName");

            var result = _otpService.VerifyOtp(email, otp);

            if (result == OtpStatus.Success)
            {
                _otpService.RemoveOtp(email);

                // Clear OTP session keys — no longer needed
                HttpContext.Session.Remove("OtpEmail");
                HttpContext.Session.Remove("OtpPurpose");
                HttpContext.Session.Remove("OtpUserId");
                HttpContext.Session.Remove("OtpCompanyName");

                if (purpose == "Login")
                {
                    HttpContext.Session.SetInt32 ("UserId",      userId ?? 0);
                    HttpContext.Session.SetString("UserEmail",   email);
                    HttpContext.Session.SetString("CompanyName", companyName ?? "");
                    HttpContext.Session.SetString("Role",        "user");

                    return RedirectToAction("Dashboard");
                }

                if (purpose == "Reset")
                {
                    TempData["Email"] = email;
                    TempData.Keep();
                    return RedirectToAction("ResetPassword");
                }
            }

            // OTP failed → restore TempData AND keep Session alive
            TempData["Email"]       = email;
            TempData["OtpPurpose"]  = purpose;
            TempData["UserId"]      = userId;
            TempData["CompanyName"] = companyName;
            TempData.Keep();

            // Refresh Session so ResendOtp can always read it
            if (email != null)
            {
                HttpContext.Session.SetString("OtpEmail",       email);
                HttpContext.Session.SetString("OtpPurpose",     purpose ?? "");
                if (userId.HasValue)
                    HttpContext.Session.SetInt32("OtpUserId",   userId.Value);
                HttpContext.Session.SetString("OtpCompanyName", companyName ?? "");
            }

            ViewBag.Error = result == OtpStatus.Expired
                ? "OTP expired. Please click Resend to get a new one."
                : "Invalid OTP. Please try again.";

            return View("VerifyOtp");
        }


        // ================================================================
        // FORGOT PASSWORD
        // ================================================================

        [HttpGet("forgot-password")]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword(string email)
        {
            var user = await _userrepo.GetByEmail(email);

            if (user == null)
            {
                ViewBag.Error = "Email not registered";
                return View();
            }

            string otp      = _otpService.GenerateOtp(email);
            string username = email.Split('@')[0];
            string body     = _templateService.GetOtpEmailTemplate(otp, "Password Reset", username);

            await _emailService.SendEmailAsync(email, "🔐 Reset Password OTP", body);

            // Store in TempData and Session
            TempData["Email"]      = email;
            TempData["OtpPurpose"] = "Reset";
            TempData.Keep();

            HttpContext.Session.SetString("OtpEmail",   email);
            HttpContext.Session.SetString("OtpPurpose", "Reset");

            return RedirectToAction("VerifyOtp");
        }


        // ================================================================
        // RESEND OTP
        // ================================================================

        [HttpPost("resend-otp")]
        public async Task<IActionResult> ResendOtp()
        {
            // Read from Session — guaranteed to survive across View() returns
            string email       = HttpContext.Session.GetString("OtpEmail");
            string purpose     = HttpContext.Session.GetString("OtpPurpose");
            int?   userId      = HttpContext.Session.GetInt32("OtpUserId");
            string companyName = HttpContext.Session.GetString("OtpCompanyName");

            if (string.IsNullOrEmpty(email))
                return BadRequest(new { success = false, message = "Session expired. Please login again." });

            string otp      = _otpService.GenerateOtp(email);
            string username = email.Split('@')[0];
            string subject  = purpose == "Reset" ? "🔐 Reset Password OTP" : "🔐 Your OTP Code";
            string label    = purpose == "Reset" ? "Password Reset" : "Login";
            string body     = _templateService.GetOtpEmailTemplate(otp, label, username);

            await _emailService.SendEmailAsync(email, subject, body);

            // Refresh Session so it stays alive
            HttpContext.Session.SetString("OtpEmail",   email);
            HttpContext.Session.SetString("OtpPurpose", purpose ?? "");
            if (userId.HasValue)
                HttpContext.Session.SetInt32("OtpUserId", userId.Value);
            HttpContext.Session.SetString("OtpCompanyName", companyName ?? "");

            return Ok(new { success = true });
        }


        // ================================================================
        // RESET PASSWORD
        // ================================================================

        [HttpGet("reset-password")]
        public IActionResult ResetPassword()
        {
            if (TempData.Peek("Email") == null)
                return RedirectToAction("ForgotPassword");

            TempData.Keep("Email");
            return View();
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword(string password, string confirmPassword)
        {
            if (password != confirmPassword)
            {
                ViewBag.Error = "Passwords do not match";
                TempData.Keep("Email");
                return View();
            }

            string email = TempData["Email"]?.ToString();

            if (email == null)
                return RedirectToAction("ForgotPassword");

            await _userrepo.UpdatePassword(email, password);

            // Clear OTP session keys after successful reset
            HttpContext.Session.Remove("OtpEmail");
            HttpContext.Session.Remove("OtpPurpose");
            HttpContext.Session.Remove("OtpUserId");
            HttpContext.Session.Remove("OtpCompanyName");

            TempData["Success"] = "Password updated successfully. Please login.";
            return RedirectToAction("Login");
        }


        // ================================================================
        // DASHBOARD
        // ================================================================

        [HttpGet("dashboard")]
        public async Task<IActionResult> Dashboard()
        {
            int? userId = HttpContext.Session.GetInt32("UserId");

            if (userId == null)
                return RedirectToAction("Login");

            var queries = await _queryrepo.GetByUserId(userId.Value);

            return View(queries);
        }


        // ================================================================
        // CREATE QUERY
        // ================================================================

        [HttpGet("create-query")]
        public IActionResult CreateQuery()
        {
            return View(new Query { Priority = Repository.Models.Enums.Priority.Low });
        }

        [HttpPost("create-query")]
        public async Task<IActionResult> CreateQuery(Query query)
        {
            if (!ModelState.IsValid)
                return View(query);

            int? userId = HttpContext.Session.GetInt32("UserId");

            if (userId == null)
                return RedirectToAction("Login", "User");

            query.UserId = userId.Value;

           int id = await _queryrepo.Create(query);

if (id > 0)
{
    query.QueryId = id;   // 🔥🔥 MOST IMPORTANT LINE

    await _elasticService.UpdateQuery(query);
}

            if (id > 0)
            {
                User? user = await _userrepo.GetById(userId.Value);

                if (user == null)
                {
                    ModelState.AddModelError("", "User not found");
                    return View(query);
                }

                TempData["Success"] = "Query Submitted Successfully";

                // Publish RabbitMQ notification
                try
                {
                    await _publisher.PublishAsync(new NotifcationMessage
                    {
                        Type       = "QueryCreated",
                        Username   = user.CompanyName,
                        QueryTitle = query.Title
                    });
                }
                catch (Exception e)
                {
                    Console.WriteLine("Error in notification: " + e.Message);
                }

                return RedirectToAction("Dashboard", "User");
            }

            ModelState.AddModelError("", "Failed to submit query");
            return View(query);
        }


        // ================================================================
        // EDIT QUERY
        // ================================================================

        [HttpGet("edit-query/{id}")]
        public async Task<IActionResult> EditQuery(int id)
        {
            int? userId = HttpContext.Session.GetInt32("UserId");

            if (userId == null)
                return RedirectToAction("Login", "User");

            var queries = await _queryrepo.GetByUserId(userId.Value);
            var query   = queries.FirstOrDefault(q => q.QueryId == id);

            if (query == null)
                return NotFound();

            return View(query);
        }

        [HttpPost("edit-query/{id}")]
        public async Task<IActionResult> EditQuery(Query query)
        {
            bool updated = await _queryrepo.Update(query);

            if (updated)
            {
                TempData["Success"] = "Query Updated";
                return RedirectToAction("Dashboard", "User");
            }

            ModelState.AddModelError("", "Cannot update solved query");
            return View(query);
        }


        // ================================================================
        // DELETE QUERY
        // ================================================================

        [HttpPost("delete-query/{id}")]
        public async Task<IActionResult> DeleteQuery(int id)
        {
            bool deleted = await _queryrepo.Delete(id);

            if (deleted)
                TempData["Success"] = "Query Deleted";
            else
                TempData["Error"] = "Cannot delete solved query";

            return RedirectToAction("Dashboard", "User");
        }


        // ================================================================
        // LOGOUT
        // ================================================================

        [HttpGet("logout")]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login", "User");
        }
    }
}