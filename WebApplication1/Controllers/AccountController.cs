using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Models;  
using Microsoft.AspNetCore.Http;

namespace WebApplication1.Controllers
{
    public class AccountController : Controller
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public AccountController(
            SignInManager<ApplicationUser> signInManager,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        // ================================
        // LOGIN GET
        // ================================
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        // ================================
        // LOGIN POST
        // ================================
        [HttpPost]
        public async Task<IActionResult> Login(string email, string password)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                ViewBag.Error = "Please enter both email and password.";
                return View();
            }

            var user = await _userManager.Users.FirstOrDefaultAsync(x => x.Email == email);
            if (user == null)
            {
                ViewBag.Error = "Invalid login attempt.";
                return View();
            }

            var result = await _signInManager.PasswordSignInAsync(user, password, false, false);

            if (!result.Succeeded)
            {
                ViewBag.Error = "Invalid login credentials.";
                return View();
            }

            // ================================
            // STORE SESSION VALUES
            // ================================
            var roles = await _userManager.GetRolesAsync(user);
            string role = roles.FirstOrDefault() ?? "Lecturer";

            HttpContext.Session.SetString("UserId", user.Id);
            HttpContext.Session.SetString("Email", user.Email ?? "");
            HttpContext.Session.SetString("FirstName", user.FirstName ?? "");
            HttpContext.Session.SetString("LastName", user.LastName ?? "");
            HttpContext.Session.SetString("Role", role);
            HttpContext.Session.SetString("HourlyRate", user.HourlyRate.ToString());

            // ================================
            // REDIRECT BASED ON ROLE
            // ================================
            return role switch
            {
                "HR" => RedirectToAction("Index", "HR"),
                "Lecturer" => RedirectToAction("Dashboard", "Lecturer"),
                "Coordinator" => RedirectToAction("Index", "Coordinator"),
                "Manager" => RedirectToAction("Index", "Manager"),

                _ => RedirectToAction("Index", "Home"),
            };
        }

        // ================================
        // LOGOUT
        // ================================
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }
    }
}
