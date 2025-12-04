// ExamSchedular.Data/Entities/Timeslot.cs
using System;

namespace ExamSchedular.Data.Entities
{
    public class Timeslot : IHasDepartment
    {
        public int TimeslotId { get; set; }
        public DateOnly Date { get; set; }
        public TimeOnly StartTime { get; set; }
        public int DurationMin { get; set; }

        // 🔽 Bölüm bağı
        public int DepartmentId { get; set; }
        public Department Department { get; set; } = null!;
    }
}
