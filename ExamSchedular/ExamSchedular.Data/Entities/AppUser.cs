using System.ComponentModel.DataAnnotations;

namespace ExamSchedular.Data.Entities
{
    
    public class AppUser
    {
        public int AppUserId { get; set; }

        [Required, MaxLength(128)]
        public string Email { get; set; } = null!;

        [Required]
        public string PasswordHash { get; set; } = null!;

        public UserRole Role { get; set; }

        // Admin için null olabilir; Koordinatör için zorunlu
        public int? DepartmentId { get; set; }
        public Department? Department { get; set; }
    }
}
