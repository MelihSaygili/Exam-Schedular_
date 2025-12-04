using System.Windows.Controls;
using ExamSchedular.UI.ViewModels;

namespace ExamSchedular.UI.Views
{
    public partial class UsersPage : UserControl
    {
        public UsersPage(UsersPageViewModel vm)
        {
            InitializeComponent();
            DataContext = vm;
        }
    }
}
