using System.Windows;
using ExamSchedular.UI.ViewModels;

namespace ExamSchedular.UI.Views
{
    public partial class EditClassroomDialog : Window
    {
        public EditClassroomDialog()
        {
            InitializeComponent();

            Loaded += (_, __) =>
            {
                if (DataContext is EditClassroomDialogViewModel vm)
                {
                    vm.RequestClose = result =>
                    {
                        DialogResult = result;   // OK/Cancel bilgisi
                        Close();
                    };
                }
            };
        }
    }
}
