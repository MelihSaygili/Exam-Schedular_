using System.Windows.Controls;
using ExamSchedular.UI.ViewModels;

namespace ExamSchedular.UI.Views
{
    public partial class StudentsImportPage : UserControl
    {
        public StudentsImportPage(StudentsImportPageViewModel vm)
        {
            InitializeComponent();
            DataContext = vm;
        }
    }
}
