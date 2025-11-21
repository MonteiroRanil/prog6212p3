using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace WebApplication1.ViewModels
{
    public class CreateClaimViewModel
    {
        [Required]
        [Range(1, 180, ErrorMessage = "Hours worked cannot exceed 180 hours per month.")]
        public decimal HoursWorked { get; set; }

        // Auto-filled from HR record, not user input
        public decimal HourlyRate { get; set; }

        public string FirstName { get; set; }
        public string LastName { get; set; }

        public string Notes { get; set; }

        [Required(ErrorMessage = "Please upload at least one supporting document")]
        public List<IFormFile> Documents { get; set; }

    }
}
