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

                // appsettings.json'dan ConnectionString oku
                var config = new ConfigurationBuilder()
                    .SetBasePath(AppContext.BaseDirectory)
                    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                    .Build();

                var cs = config.GetConnectionString("DefaultConnection");
                if (string.IsNullOrWhiteSpace(cs))
                    throw new InvalidOperationException("ConnectionStrings:DefaultConnection tanýmlý deðil.");

                var services = new ServiceCollection();

                // DbContext, Core servisler
                services.AddDbContext<AppDbContext>(opt => opt.UseNpgsql(cs));
                services.AddSingleton<ICurrentUser, CurrentUser>();
                services.AddScoped<IAuthService, AuthService>();
                services.AddScoped<IUserService, UserService>();
                services.AddScoped<IClassroomService, ClassroomService>();

                // Authorization service (MainWindow için GEREKLÝ)
                services.AddSingleton<IAuthorizationService, AuthorizationService>();

                // ViewModel kayýtlarý
                services.AddTransient<LoginViewModel>(sp =>
                    new LoginViewModel(
                        sp.GetRequiredService<IAuthService>(),
                        () => sp.GetRequiredService<MainWindow>()));

                services.AddTransient<UsersPageViewModel>();
                services.AddTransient<ClassroomsPageViewModel>(sp =>
                    new ClassroomsPageViewModel(
                        sp.GetRequiredService<IClassroomService>(),
                        () => sp.GetRequiredService<EditClassroomDialog>()));

                // View kayýtlarý
                services.AddTransient<LoginWindow>();
                services.AddTransient<MainWindow>();
                services.AddTransient<UsersPage>();
                services.AddTransient<ClassroomsPage>();
                services.AddTransient<CoursesImportPage>();
                services.AddTransient<StudentsImportPage>();

                // Dialoglar
                services.AddTransient<EditClassroomDialog>();

                Services = services.BuildServiceProvider();

                // DB migrate + seed
                using (var scope = Services.CreateScope())
                {
                    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                    db.Database.Migrate();
                    DbInitializer.Seed(db);
                }

                var login = Services.GetRequiredService<LoginWindow>();
                login.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Baþlatma hatasý", MessageBoxButton.OK, MessageBoxImage.Error);
                Shutdown(-1);
            }
        }
    }
}