using System.Windows;
using ExamSchedular.UI.ViewModels;

namespace ExamSchedular.UI.Views
{
    public partial class LoginWindow : Window
    {
        public LoginWindow(LoginViewModel vm)
        {
            InitializeComponent();
            DataContext = vm;
        }

        private void Pwd_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (sender is System.Windows.Controls.PasswordBox pb &&
                DataContext is LoginViewModel vm)
            {
                vm.Password = pb.Password;
            }
        }
    }
}
