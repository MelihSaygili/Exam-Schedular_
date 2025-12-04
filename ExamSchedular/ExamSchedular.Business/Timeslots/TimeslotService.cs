using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ExamSchedular.Data;
using ExamSchedular.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace ExamSchedular.Business
{
    public class TimeslotService : ITimeslotService
    {
        private readonly AppDbContext _db;
        private readonly ICurrentUser _current;
        public TimeslotService(AppDbContext db, ICurrentUser current)
        {
            _db = db; _current = current;
        }

        public async Task<List<Timeslot>> GetAsync()
        {
            return await _db.Timeslots.AsNoTracking()
                .WhereByScope(_current)
                .OrderBy(t => t.Date).ThenBy(t => t.StartTime)
                .ToListAsync();
        }

        public async Task<Timeslot?> GetByIdAsync(int id)
        {
            var t = await _db.Timeslots.AsNoTracking().FirstOrDefaultAsync(x => x.TimeslotId == id);
            if (t == null) return null;
            ScopeExtensions.EnsureCanAccess(_current, t);
            return t;
        }

        public async Task<Timeslot> AddOrUpdateAsync(Timeslot t)
        {
            ScopeExtensions.EnsureCanAccess(_current, t);

            if (t.TimeslotId == 0) _db.Timeslots.Add(t);
            else _db.Timeslots.Update(t);

            await _db.SaveChangesAsync();
            return t;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var t = await _db.Timeslots.FindAsync(id);
            if (t == null) return false;

            ScopeExtensions.EnsureCanAccess(_current, t);

            _db.Timeslots.Remove(t);
            await _db.SaveChangesAsync();
            return true;
        }
    }
}
