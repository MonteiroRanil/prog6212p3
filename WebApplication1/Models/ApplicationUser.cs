using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApplication1.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }

        // Only lecturers will use this
        [Column(TypeName = "decimal(18,2)")]
        public decimal HourlyRate { get; set; }
    }
}
