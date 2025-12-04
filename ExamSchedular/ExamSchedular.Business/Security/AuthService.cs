using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ExamSchedular.Data;
using ExamSchedular.Data.Entities;

public class AuthService : IAuthService
{
    private readonly IDbContextFactory<AppDbContext> _factory;

    public AuthService(IDbContextFactory<AppDbContext> factory)
    {
        _factory = factory;
    }

    public async Task<AppUser?> LoginAsync(string email, string password)
    {
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            return null;

        await using var db = _factory.CreateDbContext();

        var e = email.Trim().ToLowerInvariant();

        var user = await db.AppUsers
                           .Include(u => u.Department)
                           .AsNoTracking()
                           .FirstOrDefaultAsync(u => u.Email.ToLower() == e);

        if (user == null) return null;

        // HASH KONTROLÜ (Seed admin hashli)
        if (string.IsNullOrWhiteSpace(user.PasswordHash))
            return null;

        if (!BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
            return null;

        return user;
    }
}
