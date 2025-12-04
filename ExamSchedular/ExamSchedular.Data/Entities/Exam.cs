using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ExamSchedular.Data.Entities
{
    public class Exam : IHasDepartment
    {
        public int ExamId { get; set; }

        [Required, MaxLength(32)]
        public string ExamType { get; set; } = null!; // Vize/Final/Bütünleme

        public int DefaultDurationMinutes { get; set; } = 90;

        // Ders
        public int CourseId { get; set; }
        public Course Course { get; set; } = null!;

        // 🔽 ZAMAN DİLİMİ (NAVIGATION + FK)  <<<< BUNLAR EKSİK OLDUĞU İÇİN HATA ALIYORSUN
        public int TimeslotId { get; set; }
        public Timeslot Timeslot { get; set; } = null!;

        // Role-based filtre için
        public int DepartmentId { get; set; }
        public Department Department { get; set; } = null!;

        public ICollection<ExamAssignment> ExamAssignments { get; set; } = new List<ExamAssignment>();
    }
}
