using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;
using WebApplication1.Models;
using WebApplication1.ViewModels;

namespace WebApplication1.Controllers
{
    [Authorize(Roles = "HR")]
    public class HRController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ApplicationDbContext _context;

        public HRController(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            ApplicationDbContext context)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _context = context;
        }

        // -------------------------------
        // LIST ALL USERS
        // -------------------------------
        public async Task<IActionResult> Index()
        {
            var users = await _userManager.Users.ToListAsync();
            var model = new List<HRUserViewModel>();

            foreach (var u in users)
            {
                var role = (await _userManager.GetRolesAsync(u)).FirstOrDefault();
                model.Add(new HRUserViewModel
                {
                    Id = u.Id,
                    FirstName = u.FirstName,
                    LastName = u.LastName,
                    Email = u.Email,
                    HourlyRate = u.HourlyRate,
                    Role = role
                });
            }

            return View(model); // pass HRUserViewModel list
        }

        // -------------------------------
        // CREATE NEW USER
        // -------------------------------
        public IActionResult Create()
        {
            ViewBag.Roles = _roleManager.Roles.Select(r => r.Name).ToList();
            return View(new HRUserViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(HRUserViewModel model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Roles = _roleManager.Roles.Select(r => r.Name).ToList();
                return View(model);
            }

            var user = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email,
                FirstName = model.FirstName,
                LastName = model.LastName,
                HourlyRate = model.HourlyRate
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, model.Role);
                return RedirectToAction("Index");
            }

            foreach (var error in result.Errors)
                ModelState.AddModelError("", error.Description);

            ViewBag.Roles = _roleManager.Roles.Select(r => r.Name).ToList();
            return View(model);
        }

        // -------------------------------
        // EDIT USER
        // -------------------------------
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null) return NotFound();

            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            var role = (await _userManager.GetRolesAsync(user)).FirstOrDefault();

            var model = new HRUserViewModel
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                HourlyRate = user.HourlyRate,
                Role = role
            };

            ViewBag.Roles = _roleManager.Roles.Select(r => r.Name).ToList();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(HRUserViewModel model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Roles = _roleManager.Roles.Select(r => r.Name).ToList();
                return View(model);
            }

            var user = await _userManager.FindByIdAsync(model.Id);
            if (user == null) return NotFound();

            user.FirstName = model.FirstName;
            user.LastName = model.LastName;
            user.Email = model.Email;
            user.UserName = model.Email;
            user.HourlyRate = model.HourlyRate;

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                    ModelState.AddModelError("", error.Description);

                ViewBag.Roles = _roleManager.Roles.Select(r => r.Name).ToList();
                return View(model);
            }

            // Update role
            var currentRoles = await _userManager.GetRolesAsync(user);
            await _userManager.RemoveFromRolesAsync(user, currentRoles);
            await _userManager.AddToRoleAsync(user, model.Role);

            return RedirectToAction("Index");
        }

        // -------------------------------
        // DELETE USER
        // -------------------------------
        public async Task<IActionResult> Delete(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            await _userManager.DeleteAsync(user);
            return RedirectToAction("Index");
        }

        // -------------------------------
        // RESET PASSWORD
        // -------------------------------
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(string id, string newPassword)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            await _userManager.ResetPasswordAsync(user, token, newPassword);

            return RedirectToAction("Edit", new { id });
        }

        // -------------------------------
        // REPORTS (LINQ)
        // -------------------------------
        public IActionResult Reports()
        {
            var report = _context.LecturerClaims
                .Include(c => c.User)
                .GroupBy(c => c.User.Email)
                .Select(g => new
                {
                    Lecturer = g.Key,
                    TotalHours = g.Sum(x => x.HoursWorked),
                    TotalAmount = g.Sum(x => x.TotalAmount),
                    Pending = g.Count(x => x.Status == LecturerClaim.ClaimStatus.Pending),
                    CoordinatorApproved = g.Count(x => x.Status == LecturerClaim.ClaimStatus.CoordinatorApproved),
                    CoordinatorRejected = g.Count(x => x.Status == LecturerClaim.ClaimStatus.CoordinatorRejected),
                    ManagerApproved = g.Count(x => x.Status == LecturerClaim.ClaimStatus.ManagerApproved),
                    ManagerRejected = g.Count(x => x.Status == LecturerClaim.ClaimStatus.ManagerRejected),
                    Processed = g.Count(x => x.Status == LecturerClaim.ClaimStatus.Processed)
                })
                .ToList();

            return View(report);
        }
    }
}
