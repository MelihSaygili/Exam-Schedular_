using System.Collections.ObjectModel;
using System.Threading.Tasks;
using ExamSchedular.Business;
using ExamSchedular.Core;
using ExamSchedular.Data.Entities;
using ExamSchedular.UI.Views;
using System;

namespace ExamSchedular.UI.ViewModels
{
    public class UsersPageViewModel : ViewModelBase
    {
        private readonly IUserService _users;
        private readonly ICurrentUser _current;
        private readonly Func<AddUserDialog> _dialogFactory;   // <-- eklendi

        public UsersPageViewModel(IUserService users, ICurrentUser current, Func<AddUserDialog> dialogFactory)
        {
            _users = users;
            _current = current;
            _dialogFactory = dialogFactory;

            RefreshCommand = new RelayCommand(async () => await LoadAsync());
            AddCoordinatorCommand = new RelayCommand(OpenAddDialog, () => _current.IsAdmin);
        }

        public ObservableCollection<AppUser> Items { get; } = new();
        public RelayCommand RefreshCommand { get; }
        public RelayCommand AddCoordinatorCommand { get; }

        public AppUser? Selected { get => _selected; set => SetProperty(ref _selected, value); }
        private AppUser? _selected;

        public async Task LoadAsync()
        {
            Items.Clear();
            var list = await _users.GetAsync();
            foreach (var u in list) Items.Add(u);
        }

        private void OpenAddDialog()
        {
            var dlg = _dialogFactory();   // <-- DI’dan oluştur
            if (dlg.ShowDialog() == true)
            {
                _ = CreateAsync(dlg.Email!, dlg.Password!, dlg.DepartmentId!.Value);
            }
        }

        private async Task CreateAsync(string email, string password, int depId)
        {
            await _users.CreateCoordinatorAsync(email, password, depId);
            await LoadAsync();
        }
    }
}
