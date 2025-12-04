using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Controls;
using ExamSchedular.Business;
using ExamSchedular.Data.Entities;
using Microsoft.Extensions.DependencyInjection;

namespace ExamSchedular.UI.Views
{
    public partial class CourseStudentsPage : UserControl
    {
        private readonly ICourseService _courses;

        public CourseStudentsPage()
        {
            InitializeComponent();
            _courses = App.Services.GetRequiredService<ICourseService>();
            DataContext = this;
        }

        // Header için
        public string CourseCode { get; private set; }
        public string CourseName { get; private set; }
        public int CourseId { get; private set; }

        public ObservableCollection<Student> Items { get; } = new();

        public async void LoadCourse(int courseId, string code, string name)
        {
            CourseId = courseId;
            CourseCode = code ?? "";
            CourseName = name ?? "";

            // UI refresh
            // Basit bağlamda property changed yok; DataContext'i yeniden set edelim:
            var dc = DataContext;
            DataContext = null;
            DataContext = dc;

            Items.Clear();
            var list = await _courses.GetStudentsAsync(courseId);
            foreach (var s in list) Items.Add(s);
        }
    }
}
