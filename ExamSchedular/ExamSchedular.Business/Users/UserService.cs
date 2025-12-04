using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ExamSchedular.Data;
using ExamSchedular.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace ExamSchedular.Business
{
    public class UserService : IUserService
    {
        private readonly IDbContextFactory<AppDbContext> _factory;
        private readonly ICurrentUser _current;

        public UserService(IDbContextFactory<AppDbContext> factory, ICurrentUser current)
        {
            _factory = factory;
            _current = current;
        }

        public async Task<List<AppUser>> GetAsync()
        {
            await using var db = await _factory.CreateDbContextAsync();
            var q = db.AppUsers.AsNoTracking().Include(u => u.Department);

            if (_current.IsAdmin)
                return await q.OrderBy(u => u.Email).ToListAsync();

            if (_current.UserId.HasValue)
                return await q.Where(u => u.AppUserId == _current.UserId.Value).ToListAsync();

            var email = (_current.Email ?? string.Empty).ToLower();
            return await q.Where(u => u.Email.ToLower() == email).ToListAsync();
        }

        public async Task<AppUser> CreateCoordinatorAsync(string email, string password, int departmentId)
        {
            if (!_current.IsAdmin)
                throw new InvalidOperationException("Sadece admin kullanıcı ekleyebilir.");

            await using var db = await _factory.CreateDbContextAsync();

            email = email.Trim().ToLower();
            if (await db.AppUsers.AnyAsync(u => u.Email.ToLower() == email))
                throw new InvalidOperationException("Bu e-posta ile kayıt zaten var.");

            if (!await db.Departments.AnyAsync(d => d.DepartmentId == departmentId))
                throw new InvalidOperationException("Geçersiz bölüm.");

            var user = new AppUser
            {
                Email = email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
                Role = UserRole.Coordinator,
                DepartmentId = departmentId
            };

            db.AppUsers.Add(user);
            await db.SaveChangesAsync();
            return user;
        }

        public async Task<bool> DeleteAsync(int userId)
        {
            if (!_current.IsAdmin) throw new InvalidOperationException("Yetki yok.");

            await using var db = await _factory.CreateDbContextAsync();
            var u = await db.AppUsers.FindAsync(userId);
            if (u == null) return false;

            db.AppUsers.Remove(u);
            await db.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ResetPasswordAsync(int userId, string newPassword)
        {
            if (!_current.IsAdmin && (!_current.UserId.HasValue || _current.UserId.Value != userId))
                throw new InvalidOperationException("Yetki yok.");

            await using var db = await _factory.CreateDbContextAsync();
            var u = await db.AppUsers.FindAsync(userId);
            if (u == null) return false;

            u.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
            await db.SaveChangesAsync();
            return true;
        }
    }
}
