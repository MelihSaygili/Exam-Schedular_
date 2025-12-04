using System.Windows.Controls;
using Microsoft.Extensions.DependencyInjection;
using ExamSchedular.UI.ViewModels;

namespace ExamSchedular.UI.Views
{
    public partial class StudentsListPage : UserControl
    {
        public StudentsListPage()
        {
            InitializeComponent();

            
            if (App.Services != null)
            {
                DataContext = App.Services.GetRequiredService<StudentsListPageViewModel>();
            }
        }
    }
}
