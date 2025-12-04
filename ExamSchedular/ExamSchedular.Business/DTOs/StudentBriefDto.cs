namespace ExamSchedular.Business.DTOs
{
    public class StudentBriefDto
    {
        public int StudentId { get; set; }
        public string StudentNo { get; set; } = "";
        public string Name { get; set; } = "";

        public string Class { get; set; }

        public string Display => $"{StudentNo} – {Name}";
    }
}
