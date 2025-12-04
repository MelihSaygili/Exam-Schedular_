using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ExamSchedular.Data;
using ExamSchedular.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace ExamSchedular.Business
{
    public class ExamService : IExamService
    {
        private readonly AppDbContext _db;
        private readonly ICurrentUser _current;
        public ExamService(AppDbContext db, ICurrentUser current)
        {
            _db = db; _current = current;
        }

        public async Task<List<Exam>> GetAsync()
        {
            return await _db.Exams.AsNoTracking()
                .WhereByScope(_current)
                .Include(e => e.Course)
                .Include(e => e.Timeslot)
                .OrderBy(e => e.TimeslotId)
                .ToListAsync();
        }

        public async Task<Exam?> GetByIdAsync(int id)
        {
            var e = await _db.Exams.AsNoTracking().FirstOrDefaultAsync(x => x.ExamId == id);
            if (e == null) return null;
            ScopeExtensions.EnsureCanAccess(_current, e);
            return e;
        }

        public async Task<Exam> AddOrUpdateAsync(Exam e)
        {
            ScopeExtensions.EnsureCanAccess(_current, e);

            if (e.ExamId == 0) _db.Exams.Add(e);
            else _db.Exams.Update(e);

            await _db.SaveChangesAsync();
            return e;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var e = await _db.Exams.FindAsync(id);
            if (e == null) return false;

            ScopeExtensions.EnsureCanAccess(_current, e);

            _db.Exams.Remove(e);
            await _db.SaveChangesAsync();
            return true;
        }
    }
}
