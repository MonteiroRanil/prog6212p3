using System.ComponentModel.DataAnnotations;

namespace WebApplication1.ViewModels
{
    public class HRUserViewModel
    {
        public string Id { get; set; }

        [Required] public string FirstName { get; set; }
        [Required] public string LastName { get; set; }
        [Required, EmailAddress] public string Email { get; set; }
        [Required] public string Role { get; set; } // HR, Lecturer, Coordinator, Manager

        [Required][DataType(DataType.Password)] public string Password { get; set; }

        // Only for lecturers
        public decimal HourlyRate { get; set; }
    }
}
