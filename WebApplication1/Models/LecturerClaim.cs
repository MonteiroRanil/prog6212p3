using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApplication1.Models
{
    public class LecturerClaim
    {
        public enum ClaimStatus
        {
            Pending,            // Lecturer submitted
            CoordinatorApproved,
            CoordinatorRejected,
            ManagerApproved,
            ManagerRejected,
            Processed
        }

        [Key]
        public int ClaimId { get; set; }

        // Link to ApplicationUser
        [Required]
        public string UserId { get; set; }
        public ApplicationUser User { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal HoursWorked { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal HourlyRate { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalAmount { get; set; }

        public string? Notes { get; set; }

        public ClaimStatus Status { get; set; } = ClaimStatus.Pending;

        public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;
        public DateTime? CoordinatorReviewedAt { get; set; }
        public DateTime? ManagerReviewedAt { get; set; }

        public string? CoordinatorComment { get; set; }
        public string? ManagerComment { get; set; }

        // Relationship with documents
        public ICollection<ClaimDocument>? Documents { get; set; }
    }
}
