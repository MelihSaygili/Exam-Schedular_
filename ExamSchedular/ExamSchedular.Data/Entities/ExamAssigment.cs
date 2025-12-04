namespace ExamSchedular.Data.Entities
{
    public class ExamAssignment
    {
        public int ExamAssignmentId { get; set; }

        public int ExamId { get; set; }
        public Exam Exam { get; set; }

        public int TimeslotId { get; set; }
        public Timeslot Timeslot { get; set; }

        public int ClassroomId { get; set; }
        public Classroom Classroom { get; set; }
        public int CapacityUsed { get; set; }

        // Navigation
        public ICollection<SeatAssignment> SeatAssignments { get; set; }
    }
}
