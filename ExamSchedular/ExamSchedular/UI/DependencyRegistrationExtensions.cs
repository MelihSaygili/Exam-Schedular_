using Microsoft.Extensions.DependencyInjection;
using ExamSchedular.Business; // IAuthorizationService, AuthorizationService

namespace ExamSchedular.UI
{
    public static class DependencyRegistrationExtensions
    {
        public static IServiceCollection AddExamSchedularUi(this IServiceCollection services)
        {
            // Pages
            services.AddTransient<Views.ClassroomsPage>();
            services.AddTransient<Views.UsersPage>();
            services.AddTransient<Views.CoursesImportPage>();
            services.AddTransient<Views.StudentsImportPage>();

            // ViewModels
            services.AddTransient<ViewModels.ClassroomsPageViewModel>();
            services.AddTransient<ViewModels.UsersPageViewModel>();
            services.AddTransient<ViewModels.CoursesImportPageViewModel>();
            services.AddTransient<ViewModels.StudentsImportPageViewModel>();

            // Authorization
            services.AddSingleton<IAuthorizationService, AuthorizationService>();

            // MainWindow da DI’dan çözülsün (ctor baðýmlýlýklarý için þart)
            services.AddSingleton<Views.MainWindow>();

            return services;
        }
    }
}