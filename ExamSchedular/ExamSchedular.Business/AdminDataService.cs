using System;
using System.Linq;
using System.Threading.Tasks;
using ExamSchedular.Data;
using Microsoft.EntityFrameworkCore;

namespace ExamSchedular.Business
{
    public class AdminDataService : IAdminDataService
    {
        private readonly IDbContextFactory<AppDbContext> _factory;
        private readonly ICurrentUser _current;

        public AdminDataService(IDbContextFactory<AppDbContext> factory, ICurrentUser current)
        {
            _factory = factory;
            _current = current;
        }

        private bool IsAdmin => _current.IsAdmin;
        private bool IsCoordinator => !IsAdmin && _current.DepartmentId.HasValue;
        private int CoordinatorDeptId => _current.DepartmentId ?? 0;

        private void EnsureHasPrivilege()
        {
            if (IsAdmin) return;
            if (IsCoordinator) return;
            throw new InvalidOperationException("Bu işleme yalnızca admin veya bölüm koordinatörü yetkilidir.");
        }

        // -------------------- Course --------------------
        public async Task<int> DeleteCourseByCodeAsync(string code)
        {
            EnsureHasPrivilege();
            if (string.IsNullOrWhiteSpace(code)) return 0;
            var norm = code.Trim().ToLowerInvariant();

            await using var db = await _factory.CreateDbContextAsync();
            await using var tx = await db.Database.BeginTransactionAsync();

            try
            {
                // Admin -> tüm departmanlarda kod arar
                // Koordinatör -> sadece kendi departmanı
                var cq = db.Courses
                           .Include(c => c.Enrollments)
                           .AsQueryable();

                if (IsCoordinator)
                    cq = cq.Where(c => c.DepartmentId == CoordinatorDeptId);

                var course = await cq.FirstOrDefaultAsync(c => c.Code.ToLower() == norm);
                if (course == null) return 0;

                // Enrollments
                if (course.Enrollments?.Count > 0)
                    db.Enrollments.RemoveRange(course.Enrollments);

                // Exams -> ExamAssignments
                var exams = await db.Exams.Where(e => e.CourseId == course.CourseId).ToListAsync();
                if (exams.Count > 0)
                {
                    var examIds = exams.Select(x => x.ExamId).ToList();
                    var exAssigns = await db.ExamAssignments.Where(a => examIds.Contains(a.ExamId)).ToListAsync();
                    if (exAssigns.Count > 0) db.ExamAssignments.RemoveRange(exAssigns);

                    db.Exams.RemoveRange(exams);
                }

                db.Courses.Remove(course);
                var affected = await db.SaveChangesAsync();
                await tx.CommitAsync();
                return affected;
            }
            catch
            {
                await tx.RollbackAsync();
                throw;
            }
        }

        public async Task<int> DeleteAllCoursesAsync()
        {
            EnsureHasPrivilege();

            await using var db = await _factory.CreateDbContextAsync();
            await using var tx = await db.Database.BeginTransactionAsync();

            try
            {
                var coursesQ = db.Courses.AsQueryable();
                if (IsCoordinator)
                    coursesQ = coursesQ.Where(c => c.DepartmentId == CoordinatorDeptId);

                var courseIds = await coursesQ.Select(c => c.CourseId).ToListAsync();
                if (courseIds.Count == 0) return 0;

                // Enrollments
                var enr = await db.Enrollments.Where(e => courseIds.Contains(e.CourseId)).ToListAsync();
                if (enr.Count > 0) db.Enrollments.RemoveRange(enr);

                // Exams -> ExamAssignments
                var exams = await db.Exams.Where(e => courseIds.Contains(e.CourseId)).ToListAsync();
                if (exams.Count > 0)
                {
                    var examIds = exams.Select(x => x.ExamId).ToList();
                    var exAssigns = await db.ExamAssignments.Where(a => examIds.Contains(a.ExamId)).ToListAsync();
                    if (exAssigns.Count > 0) db.ExamAssignments.RemoveRange(exAssigns);

                    db.Exams.RemoveRange(exams);
                }

                var courses = await coursesQ.ToListAsync();
                db.Courses.RemoveRange(courses);

                var affected = await db.SaveChangesAsync();
                await tx.CommitAsync();
                return affected;
            }
            catch
            {
                await tx.RollbackAsync();
                throw;
            }
        }

        // -------------------- Student --------------------
        public async Task<int> DeleteStudentByNoAsync(string studentNo)
        {
            EnsureHasPrivilege();
            if (string.IsNullOrWhiteSpace(studentNo)) return 0;
            var no = studentNo.Trim();

            await using var db = await _factory.CreateDbContextAsync();
            await using var tx = await db.Database.BeginTransactionAsync();

            try
            {
                var sq = db.Students
                           .Include(s => s.Enrollments)
                           .AsQueryable();

                if (IsCoordinator)
                    sq = sq.Where(s => s.DepartmentId == CoordinatorDeptId);

                var student = await sq.FirstOrDefaultAsync(s => s.StudentNo == no);
                if (student == null) return 0;

                if (student.Enrollments?.Count > 0)
                    db.Enrollments.RemoveRange(student.Enrollments);

                db.Students.Remove(student);

                var affected = await db.SaveChangesAsync();
                await tx.CommitAsync();
                return affected;
            }
            catch
            {
                await tx.RollbackAsync();
                throw;
            }
        }

        public async Task<int> DeleteAllStudentsAsync()
        {
            EnsureHasPrivilege();

            await using var db = await _factory.CreateDbContextAsync();
            await using var tx = await db.Database.BeginTransactionAsync();

            try
            {
                var studentsQ = db.Students.AsQueryable();
                if (IsCoordinator)
                    studentsQ = studentsQ.Where(s => s.DepartmentId == CoordinatorDeptId);

                var studentIds = await studentsQ.Select(s => s.StudentId).ToListAsync();
                if (studentIds.Count == 0) return 0;

                var enrollments = await db.Enrollments.Where(e => studentIds.Contains(e.StudentId)).ToListAsync();
                if (enrollments.Count > 0) db.Enrollments.RemoveRange(enrollments);

                var students = await studentsQ.ToListAsync();
                db.Students.RemoveRange(students);

                var affected = await db.SaveChangesAsync();
                await tx.CommitAsync();
                return affected;
            }
            catch
            {
                await tx.RollbackAsync();
                throw;
            }
        }
    }
}
