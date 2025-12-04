namespace ExamSchedular.Data.Entities
{
    public class Course : IHasDepartment
    {
        public int CourseId { get; set; }
        public int DepartmentId { get; set; }
        public Department Department { get; set; } = null!;

        public string Code { get; set; } = null!;
        public string Name { get; set; } = null!;

        public int? InstructorId { get; set; }
        public Instructor? Instructor { get; set; }

        public bool IsMandatory { get; set; }
        public int ClassLevel { get; set; } 

        public ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
    }
}
