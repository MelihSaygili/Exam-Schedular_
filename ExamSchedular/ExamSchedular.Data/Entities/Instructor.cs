namespace ExamSchedular.Data.Entities
{
    public class Instructor
    {
        public int InstructorId { get; set; }
        public string Name { get; set; }

        // Navigation
        public ICollection<Course> Courses { get; set; }
    }
}
