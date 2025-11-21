using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;
using WebApplication1.Models;

namespace WebApplication1.Controllers
{
    [Authorize(Roles = "Coordinator")]
    public class CoordinatorController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CoordinatorController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Display all pending claims for coordinator approval
        public async Task<IActionResult> PendingClaims()
        {
            var claims = await _context.LecturerClaims
                .Include(c => c.User)
                .Where(c => c.Status == LecturerClaim.ClaimStatus.Pending)
                .ToListAsync();

            return View(claims);
        }

        // GET: Review a specific claim
        public async Task<IActionResult> ReviewClaim(int id)
        {
            var claim = await _context.LecturerClaims
                .Include(c => c.User)
                .Include(c => c.Documents)
                .FirstOrDefaultAsync(c => c.ClaimId == id);

            if (claim == null)
            {
                return NotFound();
            }

            return View(claim);
        }

        // POST: Approve or reject a claim
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ReviewClaim(int id, string action, string coordinatorComment)
        {
            var claim = await _context.LecturerClaims.FindAsync(id);
            if (claim == null)
            {
                return NotFound();
            }

            if (action == "approve")
            {
                claim.Status = LecturerClaim.ClaimStatus.CoordinatorApproved;
            }
            else if (action == "reject")
            {
                claim.Status = LecturerClaim.ClaimStatus.CoordinatorRejected;
            }

            claim.CoordinatorComment = coordinatorComment;
            claim.CoordinatorReviewedAt = DateTime.UtcNow;

            _context.Update(claim);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(PendingClaims));
        }
    }
}
