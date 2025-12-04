namespace ExamSchedular.Data.Entities
{
    public class SeatAssignment
    {
        public int SeatAssignmentId { get; set; }

        public int ExamAssignmentId { get; set; }
        public ExamAssignment ExamAssignment { get; set; }

        public int StudentId { get; set; }
        public Student Student { get; set; }

        public int RowNum { get; set; }
        public int ColNum { get; set; }
    }
}
