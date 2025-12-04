using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace ExamSchedular.Data
{
    public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
    {
        public AppDbContext CreateDbContext(string[] args)
        {
            var cs = "Host=localhost;Port=5432;Database=Yazlab2.1;Username=postgres;Password=Melih9546";
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseNpgsql(cs)
                .Options;

            return new AppDbContext(options);
        }
    }
}
 