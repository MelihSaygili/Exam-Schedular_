using System.Windows.Controls;
using ExamSchedular.UI.ViewModels;

namespace ExamSchedular.UI.Views
{
    public partial class ClassroomsPage : UserControl
    {
        public ClassroomsPage(ClassroomsPageViewModel vm)
        {
            InitializeComponent();
            DataContext = vm;
        }
    }
}
