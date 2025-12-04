using System;
using System.Windows;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ExamSchedular.UI.ViewModels;
using ExamSchedular.UI.Views;
using ExamSchedular.Data;
using ExamSchedular.Business;

namespace ExamSchedular.UI
{
    public partial class App : Application
    {
        public static IServiceProvider Services { get; private set; }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            try
            {
                ShutdownMode = ShutdownMode.OnExplicitShutdown;

                // appsettings.json
                var config = new ConfigurationBuilder()
                    .SetBasePath(AppContext.BaseDirectory)
                    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                    .Build();

                var cs = config.GetConnectionString("DefaultConnection");
                if (string.IsNullOrWhiteSpace(cs))
                    throw new InvalidOperationException("ConnectionStrings:DefaultConnection tanımlı değil.");

                var services = new ServiceCollection();

                // 1) DbContext Factory (HAVUZLU)
                services.AddPooledDbContextFactory<AppDbContext>(
                    builder => builder.UseNpgsql(cs), poolSize: 64);

                // 2) Core/current user
                services.AddSingleton<ICurrentUser, CurrentUser>();

                // 3) Servisler (Transient)
                services.AddTransient<IAuthService, AuthService>();
                services.AddTransient<IUserService, UserService>();
                services.AddTransient<IClassroomService, ClassroomService>();
                services.AddTransient<ICourseService, CourseService>();
                services.AddTransient<IStudentService, StudentService>();
                services.AddTransient<ISchedulingService, SchedulingService>();
                services.AddTransient<IExcelImportService, ExcelImportService>();
                services.AddTransient<IImportWriterService, ImportWriterService>();
                services.AddTransient<IAdminDataService, AdminDataService>();

                // Authorization
                services.AddSingleton<IAuthorizationService, AuthorizationService>();

                // 4) ViewModels (Transient)
                services.AddTransient<LoginViewModel>(sp =>
                    new LoginViewModel(
                        sp.GetRequiredService<IAuthService>(),
                        () => sp.GetRequiredService<MainWindow>(),
                        sp.GetRequiredService<ICurrentUser>()));

                services.AddTransient<UsersPageViewModel>(sp =>
                new UsersPageViewModel(
                sp.GetRequiredService<IUserService>(),
                sp.GetRequiredService<ICurrentUser>(),
                () => sp.GetRequiredService<AddUserDialog>()
                ));
                services.AddTransient<ClassroomsPageViewModel>(sp =>
                    new ClassroomsPageViewModel(
                        sp.GetRequiredService<IClassroomService>(),
                        () => sp.GetRequiredService<EditClassroomDialog>()));

                services.AddTransient<CoursesListPageViewModel>();
                services.AddTransient<StudentsListPageViewModel>();
                services.AddTransient<ProgramCreatePageViewModel>();

                
                services.AddTransient<CoursesImportPageViewModel>();
                services.AddTransient<StudentsImportPageViewModel>();

                // 5) Views (Transient)
                services.AddTransient<LoginWindow>();
                services.AddTransient<MainWindow>();
                services.AddTransient<UsersPage>();
                services.AddTransient<ClassroomsPage>();
                services.AddTransient<CoursesImportPage>();
                services.AddTransient<StudentsImportPage>();
                services.AddTransient<CoursesListPage>();
                services.AddTransient<CourseStudentsPage>();
                services.AddTransient<StudentsListPage>();

                // ProgramCreatePage: DataContext bağlayarak kaydet
                services.AddTransient<ProgramCreatePage>(sp => new ProgramCreatePage
                {
                    DataContext = sp.GetRequiredService<ProgramCreatePageViewModel>()
                });

                // Dialoglar
                services.AddTransient<EditClassroomDialog>();
                services.AddTransient<AddUserDialog>();

                Services = services.BuildServiceProvider();

                // 6) DB migrate + seed (FACTORY ile)
                using (var scope = Services.CreateScope())
                {
                    var factory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<AppDbContext>>();
                    using var db = factory.CreateDbContext();
                    db.Database.Migrate();
                    DbInitializer.Seed(db);
                }

                var login = Services.GetRequiredService<LoginWindow>();
                login.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Başlatma hatası", MessageBoxButton.OK, MessageBoxImage.Error);
                Shutdown(-1);
            }
        }
    }
}
