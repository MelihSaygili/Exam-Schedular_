using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using ExamSchedular.Business;
using ExamSchedular.Core;
using ExamSchedular.UI.Views;

namespace ExamSchedular.UI.ViewModels
{
    public class LoginViewModel : ViewModelBase
    {
        private readonly IAuthService _auth;
        private readonly Func<MainWindow> _mainFactory;
        private readonly ICurrentUser _currentUser;

        public LoginViewModel(IAuthService auth, Func<MainWindow> mainFactory, ICurrentUser currentUser)
        {
            _auth = auth;
            _mainFactory = mainFactory;
            _currentUser = currentUser;

            LoginCommand = new RelayCommand(async () => await DoLoginAsync(), CanLogin);
        }

        private string _email;
        public string Email
        {
            get => _email;
            set { if (SetProperty(ref _email, value)) LoginCommand.RaiseCanExecuteChanged(); }
        }

        private string _password;
        public string Password
        {
            get => _password;
            set { if (SetProperty(ref _password, value)) LoginCommand.RaiseCanExecuteChanged(); }
        }

        private string _error;
        public string Error { get => _error; set => SetProperty(ref _error, value); }

        private bool _isBusy;
        public bool IsBusy { get => _isBusy; set { if (SetProperty(ref _isBusy, value)) LoginCommand.RaiseCanExecuteChanged(); } }

        public RelayCommand LoginCommand { get; }

        private bool CanLogin() =>
            !IsBusy && !string.IsNullOrWhiteSpace(Email) && !string.IsNullOrWhiteSpace(Password);

        private async Task DoLoginAsync()
        {
            IsBusy = true;
            Error = null;
            try
            {
                var user = await _auth.LoginAsync(Email, Password);
                if (user == null)
                {
                    Error = "Geçersiz e-posta veya şifre.";
                    return;
                }

                // CurrentUser set
                TryPopulateCurrentUserFrom(user);

                var main = _mainFactory();
                Application.Current.MainWindow = main;
                main.Show();

                var login = Application.Current.Windows.OfType<LoginWindow>().FirstOrDefault();
                login?.Close();

                Application.Current.ShutdownMode = ShutdownMode.OnMainWindowClose;
            }
            catch (Exception ex)
            {
                Error = ex.Message;
            }
            finally
            {
                IsBusy = false;
            }
        }

        private void TryPopulateCurrentUserFrom(object user)
        {
            object Get(string name)
            {
                var pi = user.GetType().GetProperty(name, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
                return pi?.GetValue(user);
            }

            var idObj = Get("AppUserId") ?? Get("UserId") ?? Get("Id");
            var mailObj = Get("Email");

            var isAdminObj = Get("IsAdmin");
            var roleObj = Get("Role") ?? Get("UserRole") ?? Get("RoleName");

            var deptObj = Get("DepartmentId");

            var userId = idObj != null ? Convert.ToInt32(idObj) : 0;
            var email = mailObj?.ToString() ?? Email;

            bool isAdmin = false;
            if (isAdminObj != null)
                isAdmin = Convert.ToBoolean(isAdminObj);
            else if (roleObj != null)
                isAdmin = roleObj.ToString()?.Equals("Admin", StringComparison.OrdinalIgnoreCase) == true;

            int? deptId = null;
            if (deptObj != null)
            {
                if (deptObj is int i) deptId = i;
                else if (int.TryParse(deptObj.ToString(), out var parsed)) deptId = parsed;
            }

            _currentUser.Set(userId, email, isAdmin, deptId);
        }
    }
}
