using System;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Extensions.DependencyInjection;
using ExamSchedular.Business;
using ExamSchedular.UI.ViewModels;

namespace ExamSchedular.UI.Views
{
    public partial class MainWindow : Window
    {
        private readonly IServiceProvider _sp;
        private readonly IAuthorizationService _auth;
        private readonly ICurrentUser _current;

        public void SetContent(UserControl uc) => Host.Content = uc;

        public MainWindow(IServiceProvider sp, IAuthorizationService auth, ICurrentUser current)
        {
            InitializeComponent();
            _sp = sp;
            _auth = auth;
            _current = current;

            Loaded += async (_, __) =>
            {
                var page = _sp.GetRequiredService<ClassroomsPage>();
                page.DataContext = _sp.GetRequiredService<ClassroomsPageViewModel>();
                Host.Content = page;

                if (page.DataContext is ClassroomsPageViewModel vm)
                    await vm.LoadAsync();
            };
        }

        private async void Nav_Click(object sender, RoutedEventArgs e)
        {
            var tag = (sender as Button)?.Tag as string;
            switch (tag)
            {
                case "Users":
                    {
                        if (!_auth.IsAdmin())
                        {
                            MessageBox.Show("Yetkiniz yok. Bu sayfayı yalnızca admin görebilir.",
                                "Uyarı", MessageBoxButton.OK, MessageBoxImage.Warning);
                            return;
                        }

                        var page = _sp.GetRequiredService<UsersPage>();
                        page.DataContext = _sp.GetRequiredService<UsersPageViewModel>();
                        Host.Content = page;

                        if (page.DataContext is UsersPageViewModel vm)
                            await vm.LoadAsync();
                        break;
                    }

                case "Classrooms":
                    {
                        var page = _sp.GetRequiredService<ClassroomsPage>();
                        page.DataContext = _sp.GetRequiredService<ClassroomsPageViewModel>();
                        Host.Content = page;

                        if (page.DataContext is ClassroomsPageViewModel vm)
                            await vm.LoadAsync();
                        break;
                    }

                case "CoursesImport":
                    {
                        if (!_auth.CanManageDepartmentData())
                        {
                            MessageBox.Show("Yetkiniz yok. Sadece admin/koordinatör.",
                                "Uyarı", MessageBoxButton.OK, MessageBoxImage.Warning);
                            return;
                        }

                        var page = _sp.GetRequiredService<CoursesImportPage>();
                        page.DataContext = _sp.GetRequiredService<CoursesImportPageViewModel>();
                        Host.Content = page;
                        break;
                    }

                case "CoursesList":
                    {
                        var page = _sp.GetRequiredService<CoursesListPage>();
                        page.DataContext = _sp.GetRequiredService<CoursesListPageViewModel>();
                        Host.Content = page;

                        if (page.DataContext is CoursesListPageViewModel vm)
                            await vm.LoadAsync();
                        break;
                    }

                case "StudentsImport":
                    {
                        if (!_auth.CanManageDepartmentData())
                        {
                            MessageBox.Show("Yetkiniz yok. Sadece admin/koordinatör.",
                                "Uyarı", MessageBoxButton.OK, MessageBoxImage.Warning);
                            return;
                        }

                        var page = _sp.GetRequiredService<StudentsImportPage>();
                        page.DataContext ??= _sp.GetRequiredService<StudentsImportPageViewModel>();
                        Host.Content = page;
                        break;
                    }

                case "StudentsList":
                    {
                        var page = _sp.GetRequiredService<StudentsListPage>();
                        page.DataContext = _sp.GetRequiredService<StudentsListPageViewModel>();
                        Host.Content = page;

                        if (page.DataContext is StudentsListPageViewModel vm)
                            await vm.LoadAsync();
                        break;
                    }

                case "Schedule":
                    {
                        var page = _sp.GetRequiredService<ProgramCreatePage>();
                        Host.Content = page;
                        break;
                    }

                case "Logout":
                    {
                        var confirm = MessageBox.Show(
                            "Oturumu kapatmak istiyor musunuz?",
                            "Çıkış", MessageBoxButton.YesNo, MessageBoxImage.Question);

                        if (confirm != MessageBoxResult.Yes)
                            return;

                        _current.Set(0, null, false, null);

                        Application.Current.ShutdownMode = ShutdownMode.OnExplicitShutdown;

                        var login = _sp.GetRequiredService<LoginWindow>();
                        Application.Current.MainWindow = login;
                        login.Show();

                        Close();
                        break;
                    }

                default:
                    break;
            }
        }
    }
}
