using System.Windows.Controls;
using ExamSchedular.UI.ViewModels;

namespace ExamSchedular.UI.Views
{
    public partial class CoursesImportPage : UserControl
    {
        public CoursesImportPage(CoursesImportPageViewModel vm)
        {
            InitializeComponent();
            DataContext = vm;   // ← kritik: VM bağla
        }
    }
}
