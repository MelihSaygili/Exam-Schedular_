using ExamSchedular.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace ExamSchedular.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        // ---- Core tablolar ----
        public DbSet<Department> Departments { get; set; }

        public DbSet<AppUser> AppUsers { get; set; }
        public DbSet<AppUser> Users => Set<AppUser>(); // alias (isteğe bağlı)

        // ---- Diğer tablolar ----
        public DbSet<Classroom> Classrooms { get; set; }
        public DbSet<Course> Courses { get; set; }
        public DbSet<Instructor> Instructors { get; set; }
        public DbSet<Student> Students { get; set; }
        public DbSet<Enrollment> Enrollments { get; set; }
        public DbSet<Exam> Exams { get; set; }
        public DbSet<Timeslot> Timeslots { get; set; }
        public DbSet<ExamAssignment> ExamAssignments { get; set; }
        public DbSet<SeatAssignment> SeatAssignments { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // -------- AppUser --------
            modelBuilder.Entity<AppUser>()
                .HasIndex(u => u.Email)
                .IsUnique();

            modelBuilder.Entity<AppUser>()
                .HasOne(u => u.Department)
                .WithMany(d => d.Users)
                .HasForeignKey(u => u.DepartmentId)
                .OnDelete(DeleteBehavior.Restrict);

            // -------- Department --------
            modelBuilder.Entity<Department>()
                .HasIndex(d => d.Name)
                .IsUnique();

            // -------- Student --------
            modelBuilder.Entity<Student>()
                .HasOne(s => s.Department)
                .WithMany(d => d.Students)  // Department.Students NAVIGATION LAZIM
                .HasForeignKey(s => s.DepartmentId)
                .OnDelete(DeleteBehavior.Restrict);

            // -------- Course --------
            modelBuilder.Entity<Course>()
                .HasIndex(c => new { c.DepartmentId, c.Code })
                .IsUnique();

            modelBuilder.Entity<Course>()
                .HasOne(c => c.Instructor)
                .WithMany(i => i.Courses)
                .HasForeignKey(c => c.InstructorId)
                .OnDelete(DeleteBehavior.SetNull);

            // -------- Enrollment --------
            modelBuilder.Entity<Enrollment>()
                .HasOne(e => e.Student)
                .WithMany(s => s.Enrollments)
                .HasForeignKey(e => e.StudentId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Enrollment>()
                .HasOne(e => e.Course)
                .WithMany(c => c.Enrollments)
                .HasForeignKey(e => e.CourseId)
                .OnDelete(DeleteBehavior.Cascade);

            // Exam ↔ Department
            modelBuilder.Entity<Exam>()
                .HasOne(e => e.Department)
                .WithMany(d => d.Exams)               // Department tarafında ICollection<Exam> varsa
                .HasForeignKey(e => e.DepartmentId)
                .OnDelete(DeleteBehavior.Restrict);

            // Timeslot ↔ Department  (Timeslot’ta DepartmentId alanı varsa)
            modelBuilder.Entity<Timeslot>()
                .HasOne(t => t.Department)                 // ya da .HasOne(t => t.Department)
                .WithMany()
                .HasForeignKey(t => t.DepartmentId)
                .OnDelete(DeleteBehavior.Restrict);

            // ExamAssignment
            modelBuilder.Entity<ExamAssignment>(e =>
            {
                e.Property(x => x.CapacityUsed)
                 .HasDefaultValue(0); // NOT NULL default 0
            });

            // -------- Timeslot --------
            modelBuilder.Entity<Timeslot>(e =>
            {
                e.Property(p => p.Date).HasColumnType("date");
                e.Property(p => p.StartTime).HasColumnType("time");
            });
        }
    }
}
