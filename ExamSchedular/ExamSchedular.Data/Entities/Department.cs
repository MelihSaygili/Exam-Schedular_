using ExamSchedular.Data.Entities;

public class Department
{
    public int DepartmentId { get; set; }
    public string Name { get; set; }

    public ICollection<AppUser> Users { get; set; } = new List<AppUser>();
    public ICollection<Student> Students { get; set; } = new List<Student>();
    public ICollection<Course> Courses { get; set; } = new List<Course>();
    public ICollection<Classroom> Classrooms { get; set; } = new List<Classroom>();
    public ICollection<Timeslot> Timeslots { get; set; } = new List<Timeslot>();
    public ICollection<Exam> Exams { get; set; } = new List<Exam>();
}
