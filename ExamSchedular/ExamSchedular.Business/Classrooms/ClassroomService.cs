using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ExamSchedular.Data;
using ExamSchedular.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace ExamSchedular.Business
{
    public class ClassroomService : IClassroomService
    {
        private readonly IDbContextFactory<AppDbContext> _factory;
        private readonly ICurrentUser _current;

        public ClassroomService(IDbContextFactory<AppDbContext> factory, ICurrentUser current)
        {
            _factory = factory;
            _current = current;
        }

        public async Task<List<Classroom>> GetAsync()
        {
            await using var db = _factory.CreateDbContext();

            var q = db.Classrooms.AsNoTracking().AsQueryable();

            if (!_current.IsAdmin && _current.DepartmentId.HasValue)
            {
                var dep = _current.DepartmentId.Value;
                q = q.Where(x => x.DepartmentId == dep);
            }

            return await q.OrderBy(x => x.Code).ToListAsync();
        }

        public async Task<Classroom?> GetByIdAsync(int id)
        {
            await using var db = _factory.CreateDbContext();
            var c = await db.Classrooms.AsNoTracking()
                                       .FirstOrDefaultAsync(x => x.ClassroomId == id);
            return c;
        }

        public async Task<Classroom> AddOrUpdateAsync(Classroom model)
        {
            await using var db = _factory.CreateDbContext();

            // Koordinatör ise DepartmentId’yi zorla kendi departmanı yap
            if (!_current.IsAdmin)
            {
                var depId = _current.DepartmentId
                             ?? throw new InvalidOperationException("Koordinatör departmanı bulunamadı.");

                if (model.DepartmentId == 0 || model.DepartmentId != depId)
                    model.DepartmentId = depId;
            }

            if (model.ClassroomId == 0)
            {
                // Yeni kayıt
                db.Classrooms.Add(model);
                await db.SaveChangesAsync();
                return model;
            }
            else
            {
                // Güncelleme
                var dbEntity = await db.Classrooms
                                       .FirstOrDefaultAsync(x => x.ClassroomId == model.ClassroomId)
                               ?? throw new InvalidOperationException("Sınıf bulunamadı.");

                // Yetki: koordinatör sadece kendi departmanını düzenleyebilir
                if (!_current.IsAdmin && _current.DepartmentId.HasValue &&
                    dbEntity.DepartmentId != _current.DepartmentId.Value)
                {
                    throw new InvalidOperationException("Bu işleme yetkiniz yok.");
                }

                dbEntity.Code = model.Code?.Trim();
                dbEntity.Name = string.IsNullOrWhiteSpace(model.Name) ? model.Code : model.Name;
                dbEntity.Rows = model.Rows;
                dbEntity.Columns = model.Columns;
                dbEntity.SeatGroupSize = model.SeatGroupSize;
                dbEntity.Capacity = model.Capacity;

                // DepartmentId
                if (!_current.IsAdmin && _current.DepartmentId.HasValue)
                    dbEntity.DepartmentId = _current.DepartmentId.Value;
                else if (_current.IsAdmin && model.DepartmentId > 0)
                    dbEntity.DepartmentId = model.DepartmentId;

                await db.SaveChangesAsync();
                return dbEntity;
            }
        }

        public async Task<bool> DeleteAsync(int id)
        {
            await using var db = _factory.CreateDbContext();

            var c = await db.Classrooms.FirstOrDefaultAsync(x => x.ClassroomId == id);
            if (c == null) return false;

            if (!_current.IsAdmin && _current.DepartmentId.HasValue &&
                c.DepartmentId != _current.DepartmentId.Value)
            {
                throw new InvalidOperationException("Bu işleme yetkiniz yok.");
            }

            db.Classrooms.Remove(c);
            await db.SaveChangesAsync();
            return true;
        }
    }
}
