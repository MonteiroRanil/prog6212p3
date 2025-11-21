using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Models;

namespace WebApplication1.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<LecturerClaim> LecturerClaims { get; set; }
        public DbSet<ClaimDocument> ClaimDocuments { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Decimal precision for SQL Server
            builder.Entity<LecturerClaim>()
                .Property(c => c.HourlyRate)
                .HasPrecision(18, 2);

            builder.Entity<LecturerClaim>()
                .Property(c => c.HoursWorked)
                .HasPrecision(18, 2);

            builder.Entity<LecturerClaim>()
                .Property(c => c.TotalAmount)
                .HasPrecision(18, 2);
        }
    }
}
