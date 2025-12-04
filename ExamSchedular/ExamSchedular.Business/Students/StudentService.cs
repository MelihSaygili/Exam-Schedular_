// Business/Students/StudentService.cs
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ExamSchedular.Data;
using ExamSchedular.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace ExamSchedular.Business
{
    public class StudentService : IStudentService
    {
        private readonly IDbContextFactory<AppDbContext> _factory;
        private readonly ICurrentUser _current;

        public StudentService(IDbContextFactory<AppDbContext> factory, ICurrentUser current)
        {
            _factory = factory;
            _current = current;
        }

        public async Task<List<Student>> SearchAsync(string search)
        {
            await using var db = _factory.CreateDbContext();

            var q = db.Students.AsNoTracking().AsQueryable();

            // Koordinatör kendi bölümündeki öğrencileri görsün
            if (!_current.IsAdmin && _current.DepartmentId.HasValue)
            {
                var depId = _current.DepartmentId.Value;
                q = q.Where(s => s.DepartmentId == depId);
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = search.Trim().ToLower();
                q = q.Where(st =>
                    st.StudentNo.ToLower().Contains(s) ||
                    st.NameSurname.ToLower().Contains(s));
            }

            return await q.OrderBy(st => st.StudentNo).ToListAsync();
        }

        public async Task<List<Course>> GetCoursesAsync(int studentId)
        {
            await using var db = _factory.CreateDbContext();

            var q = db.Enrollments.AsNoTracking()
                                   .Where(e => e.StudentId == studentId)
                                   .Include(e => e.Course)
                                   .ThenInclude(c => c.Instructor)
                                   .AsQueryable();

            if (!_current.IsAdmin && _current.DepartmentId.HasValue)
            {
                var depId = _current.DepartmentId.Value;
                q = q.Where(e => e.Course.DepartmentId == depId);
            }

            return await q.Select(e => e.Course)
                          .OrderBy(c => c.Code)
                          .ToListAsync();
        }

        // Opsiyonel CRUD'lar
        public async Task<List<Student>> GetAsync()
        {
            await using var db = _factory.CreateDbContext();

            var q = db.Students.AsNoTracking().AsQueryable();

            if (!_current.IsAdmin && _current.DepartmentId.HasValue)
            {
                var depId = _current.DepartmentId.Value;
                q = q.Where(s => s.DepartmentId == depId);
            }
            return await q.OrderBy(x => x.StudentNo).ToListAsync();
        }

        public async Task<Student?> GetByIdAsync(int id)
        {
            await using var db = _factory.CreateDbContext();
            var s = await db.Students.AsNoTracking().FirstOrDefaultAsync(x => x.StudentId == id);
            return s;
        }

        public async Task<Student> AddOrUpdateAsync(Student s)
        {
            await using var db = _factory.CreateDbContext();

            if (s.StudentId == 0) db.Students.Add(s);
            else db.Students.Update(s);

            await db.SaveChangesAsync();
            return s;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            await using var db = _factory.CreateDbContext();

            var s = await db.Students.FindAsync(id);
            if (s == null) return false;

            db.Students.Remove(s);
            await db.SaveChangesAsync();
            return true;
        }
    }
}
