using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Extensions.DependencyInjection;
using ExamSchedular.UI.ViewModels;
using ExamSchedular.Data.Entities;

namespace ExamSchedular.UI.Views
{
    public partial class CoursesListPage : UserControl
    {
        public CoursesListPage()
        {
            InitializeComponent();
        }

        private void OnRowDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (DataContext is not CoursesListPageViewModel vm) return;
            if (vm.SelectedItem is not Course sel) return;

            var page = App.Services.GetRequiredService<CourseStudentsPage>();
            page.LoadCourse(sel.CourseId, sel.Code, sel.Name);

            // Ana içerik alanına bas
            var main = System.Windows.Application.Current.MainWindow as MainWindow;
            main?.SetContent(page);
        }
    }
}
