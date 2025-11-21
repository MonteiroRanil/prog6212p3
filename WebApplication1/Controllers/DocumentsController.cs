using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using WebApplication1.Data;
using WebApplication1.Models;

public class DocumentsController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly IWebHostEnvironment _env;

    public DocumentsController(ApplicationDbContext context, IWebHostEnvironment env)
    {
        _context = context;
        _env = env;
    }

    // GET: Upload form
    [HttpGet]
    public IActionResult Upload(int claimId)
    {
        ViewBag.ClaimId = claimId;
        return View();
    }

    // POST: Upload file
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Upload(IFormFile file, int claimId)
    {
        if (file == null || file.Length == 0)
        {
            ModelState.AddModelError("", "Please select a file.");
            ViewBag.ClaimId = claimId;
            return View();
        }

        // Check that claim exists
        var claim = await _context.LecturerClaims.FindAsync(claimId);
        if (claim == null)
        {
            ModelState.AddModelError("", "The specified claim does not exist.");
            return View();
        }

        // Create uploads folder
        var uploadPath = Path.Combine(_env.WebRootPath, "uploads");
        if (!Directory.Exists(uploadPath))
            Directory.CreateDirectory(uploadPath);

        // Save file
        var uniqueFileName = $"{Guid.NewGuid()}_{Path.GetFileName(file.FileName)}";
        var filePath = Path.Combine(uploadPath, uniqueFileName);

        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        // Save document entry in DB
        var document = new ClaimDocument
        {
            ClaimId = claimId,
            LecturerClaim = claim,
            FileName = file.FileName,
            FilePath = "/uploads/" + uniqueFileName,
            ContentType = file.ContentType,
            FileSize = file.Length,
            UploadedAt = DateTime.UtcNow
        };

        _context.ClaimDocuments.Add(document);
        await _context.SaveChangesAsync();

        // Fix redirect (no ClaimsController)
        return RedirectToAction("ClaimsHistory", "Lecturer");
    }
}
