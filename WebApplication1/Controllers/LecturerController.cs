using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;
using WebApplication1.Models;
using WebApplication1.ViewModels;

namespace WebApplication1.Controllers
{
    public class LecturerController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;

        public LecturerController(UserManager<ApplicationUser> userManager, ApplicationDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var claims = await _context.LecturerClaims
                .Where(c => c.UserId == user.Id)
                .ToListAsync();

            return View(claims);
        }
        public async Task<IActionResult> SubmitClaim()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var model = new CreateClaimViewModel
            {
                FirstName = user.FirstName,
                LastName = user.LastName,
                HourlyRate = user.HourlyRate
            };

            return View(model);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitClaim(CreateClaimViewModel model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            if (model.HoursWorked > 180)
            {
                ModelState.AddModelError("HoursWorked", "Cannot work more than 180 hours per month.");
            }

            if (ModelState.IsValid)
            {
                var claim = new LecturerClaim
                {
                    UserId = user.Id,
                    HoursWorked = model.HoursWorked,
                    HourlyRate = user.HourlyRate,
                    TotalAmount = model.HoursWorked * user.HourlyRate,
                    Notes = model.Notes,
                    Status = LecturerClaim.ClaimStatus.Pending
                };

                _context.LecturerClaims.Add(claim);
                await _context.SaveChangesAsync();

                // handle document uploads if any
                if (model.Documents != null)
                {
                    foreach (var file in model.Documents)
                    {
                        var doc = new ClaimDocument
                        {
                            ClaimId = claim.ClaimId,
                            FileName = file.FileName,
                            FilePath = "/uploads/" + file.FileName, // adjust path
                            ContentType = file.ContentType,
                            FileSize = file.Length
                        };
                        _context.ClaimDocuments.Add(doc);
                    }
                    await _context.SaveChangesAsync();
                }

                return RedirectToAction(nameof(ClaimsHistory));
            }

            model.FirstName = user.FirstName;
            model.LastName = user.LastName;
            model.HourlyRate = user.HourlyRate;

            return View(model);
        }
        public async Task<IActionResult> ClaimsHistory()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var claims = await _context.LecturerClaims
                .Include(c => c.Documents)
                .Where(c => c.UserId == user.Id)
                .ToListAsync();

            return View(claims);
        }

    }
}
