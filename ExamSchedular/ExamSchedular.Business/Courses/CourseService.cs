// Business/Courses/CourseService.cs
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ExamSchedular.Data;
using ExamSchedular.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace ExamSchedular.Business
{
    public class CourseService : ICourseService
    {
        private readonly IDbContextFactory<AppDbContext> _factory;
        private readonly ICurrentUser _current;

        public CourseService(IDbContextFactory<AppDbContext> factory, ICurrentUser current)
        {
            _factory = factory;
            _current = current;
        }

        public async Task<List<Course>> GetAsync()
        {
            await using var db = _factory.CreateDbContext();

            var q = db.Courses.AsNoTracking()
                              .Include(c => c.Instructor)
                              .Include(c => c.Department)
                              .AsQueryable();

            // Koordinatör ise sadece kendi departmanı
            if (!_current.IsAdmin && _current.DepartmentId.HasValue)
            {
                var depId = _current.DepartmentId.Value;
                q = q.Where(c => c.DepartmentId == depId);
            }

            return await q.OrderBy(x => x.Code).ToListAsync();
        }

        public async Task<Course?> GetByIdAsync(int id)
        {
            await using var db = _factory.CreateDbContext();

            var c = await db.Courses.AsNoTracking()
                                    .Include(x => x.Instructor)
                                    .Include(x => x.Department)
                                    .FirstOrDefaultAsync(x => x.CourseId == id);
            if (c == null) return null;

            // (İstersen) erişim denetimi:
            ScopeExtensions.EnsureCanAccess(_current, c);
            return c;
        }

        public async Task<Course> AddOrUpdateAsync(Course c)
        {
            await using var db = _factory.CreateDbContext();

            // Erişim denetimi
            ScopeExtensions.EnsureCanAccess(_current, c);

            if (c.CourseId == 0)
            {
                db.Courses.Add(c);
            }
            else
            {
                db.Courses.Update(c);
            }
            await db.SaveChangesAsync();
            return c;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            await using var db = _factory.CreateDbContext();

            var c = await db.Courses.FindAsync(id);
            if (c == null) return false;

            // Erişim denetimi
            ScopeExtensions.EnsureCanAccess(_current, c);

            db.Courses.Remove(c);
            await db.SaveChangesAsync();
            return true;
        }

        public async Task<List<Course>> SearchAsync(string search)
        {
            await using var db = _factory.CreateDbContext();

            var q = db.Courses.AsNoTracking()
                              .Include(c => c.Instructor)
                              .Include(c => c.Department)
                              .AsQueryable();

            // Koordinatör: departman filtresi
            if (!_current.IsAdmin && _current.DepartmentId.HasValue)
            {
                var depId = _current.DepartmentId.Value;
                q = q.Where(c => c.DepartmentId == depId);
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = search.Trim().ToLower();
                q = q.Where(c =>
                    c.Code.ToLower().Contains(s) ||
                    c.Name.ToLower().Contains(s) ||
                    (c.Instructor != null && c.Instructor.Name.ToLower().Contains(s)));
            }

            return await q.OrderBy(c => c.Code).ToListAsync();
        }

        public async Task<List<Student>> GetStudentsAsync(int courseId)
        {
            await using var db = _factory.CreateDbContext();

            var q = db.Enrollments.AsNoTracking()
                                   .Where(e => e.CourseId == courseId);

            if (!_current.IsAdmin && _current.DepartmentId.HasValue)
            {
                var dep = _current.DepartmentId.Value;
                q = q.Where(e => e.Course.DepartmentId == dep);
            }

            return await q.OrderBy(e => e.Student.StudentNo)
                          .Select(e => e.Student)
                          .ToListAsync();
        }
    }
}
