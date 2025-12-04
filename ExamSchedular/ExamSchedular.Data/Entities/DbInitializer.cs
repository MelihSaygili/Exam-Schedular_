using ExamSchedular.Data;
using ExamSchedular.Data.Entities;
using System.Linq;

public static class DbInitializer
{
    public static void Seed(AppDbContext ctx)
    {
        // Migrate App.xaml.cs tarafında yapıldı; burada sadece seed
        // NOT: EnsureCreated KULLANMA

        // Departments
        var deps = new[] { "Bilgisayar Müh.", "Yazılım Müh.", "Elektrik Müh.",
                           "Elektronik Müh.", "İnşaat Müh." };
        foreach (var name in deps)
        {
            if (!ctx.Departments.Any(d => d.Name == name))
                ctx.Departments.Add(new Department { Name = name });
        }
        ctx.SaveChanges();

        // Admin kullanıcı
        if (!ctx.AppUsers.Any(u => u.Role == UserRole.Admin))
        {
            var email = "admin@uni.edu.tr";
            var passHash = BCrypt.Net.BCrypt.HashPassword("Admin123!");

            ctx.AppUsers.Add(new AppUser
            {
                Email = email,
                PasswordHash = passHash,
                Role = UserRole.Admin,
                DepartmentId = null
            });
            ctx.SaveChanges();
        }
    }
}
