namespace ExamSchedular.Business.DTOs
{
    public class CourseListItemDto
    {
        public int CourseId { get; set; }
        public string Code { get; set; } = "";
        public string Name { get; set; } = "";
        public string InstructorName { get; set; } = "";
        public bool IsMandatory { get; set; }
        public string DepartmentName { get; set; } = "";
    }
}
