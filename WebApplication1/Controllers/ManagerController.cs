using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;
using WebApplication1.Models;

namespace WebApplication1.Controllers
{
    [Authorize(Roles = "Manager")]
    public class ManagerController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ManagerController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Display all claims that have been approved by coordinator
        public async Task<IActionResult> ManagerPending()
        {
            var claims = await _context.LecturerClaims
                .Include(c => c.User)
                .Where(c => c.Status == LecturerClaim.ClaimStatus.CoordinatorApproved)
                .OrderBy(c => c.SubmittedAt)
                .ToListAsync();

            return View(claims);
        }

        // Display details for a single claim for manager review
        public async Task<IActionResult> ManagerReview(int id)
        {
            var claim = await _context.LecturerClaims
                .Include(c => c.User)
                .Include(c => c.Documents)
                .FirstOrDefaultAsync(c => c.ClaimId == id);

            if (claim == null)
                return NotFound();

            return View(claim);
        }

        // Process approval or rejection
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ManagerReview(int id, string actionType, string comment)
        {
            var claim = await _context.LecturerClaims.FindAsync(id);
            if (claim == null)
                return NotFound();

            if (actionType == "approve")
            {
                claim.Status = LecturerClaim.ClaimStatus.ManagerApproved;
                claim.ManagerReviewedAt = DateTime.UtcNow;
            }
            else if (actionType == "reject")
            {
                claim.Status = LecturerClaim.ClaimStatus.ManagerRejected;
                claim.ManagerReviewedAt = DateTime.UtcNow;
            }

            claim.ManagerComment = comment;

            try
            {
                _context.Update(claim);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ClaimExists(claim.ClaimId))
                    return NotFound();
                else
                    throw;
            }

            return RedirectToAction(nameof(ManagerPending));
        }

        private bool ClaimExists(int id)
        {
            return _context.LecturerClaims.Any(e => e.ClaimId == id);
        }
    }
}
