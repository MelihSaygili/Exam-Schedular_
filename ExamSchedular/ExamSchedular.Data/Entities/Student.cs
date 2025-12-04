namespace ExamSchedular.Data.Entities
{
    public class Student : IHasDepartment // eğer IHasDepartment kullanıyorsan
    {
        public int StudentId { get; set; }
        public string StudentNo { get; set; } = "";
        public string NameSurname { get; set; } = "";
        public int ClassYear { get; set; }

        // FK + Navigation (FK zaten var: FK_Students_Departments_DepartmentId)
        public int DepartmentId { get; set; }
        public Department Department { get; set; } = null!;

        // Enrollments
        public ICollection<Enrollment>? Enrollments { get; set; }
    }
}
