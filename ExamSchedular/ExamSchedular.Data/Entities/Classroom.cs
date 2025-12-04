namespace ExamSchedular.Data.Entities
{
    public class Classroom : IHasDepartment
    {
        public int ClassroomId { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public int Capacity { get; set; }
        public int Rows { get; set; }
        public int Columns { get; set; }
        public int SeatGroupSize { get; set; } // 2 veya 3 kişilik sıra

        public int DepartmentId { get; set; }
        public Department Department { get; set; }

        // Navigation
        public ICollection<ExamAssignment> ExamAssignments { get; set; }
    }
}
