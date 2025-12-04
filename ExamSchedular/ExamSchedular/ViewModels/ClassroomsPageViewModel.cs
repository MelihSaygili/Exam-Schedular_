using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using ExamSchedular.Business;
using ExamSchedular.Core;
using ExamSchedular.Data.Entities;
using ExamSchedular.UI.Views;

namespace ExamSchedular.UI.ViewModels
{
    public class ClassroomsPageViewModel : ViewModelBase
    {
        private readonly IClassroomService _service;
        private readonly System.Func<EditClassroomDialog> _dialogFactory;

        public ClassroomsPageViewModel(IClassroomService service,
                                       System.Func<EditClassroomDialog> dialogFactory)
        {
            _service = service;
            _dialogFactory = dialogFactory;

            RefreshCommand = new RelayCommand(async () => await LoadAsync());
            AddCommand = new RelayCommand(() => OpenEditor(null));
            EditCommand = new RelayCommand(() => { if (Selected != null) OpenEditor(Selected); }, () => Selected != null);
            DeleteCommand = new RelayCommand(async () => await DeleteAsync(), () => Selected != null);

            _ = LoadAsync();
        }

        public ObservableCollection<Classroom> Items { get; } = new();

        private Classroom _selected;
        public Classroom Selected
        {
            get => _selected;
            set { if (SetProperty(ref _selected, value)) { EditCommand.RaiseCanExecuteChanged(); DeleteCommand.RaiseCanExecuteChanged(); } }
        }

        public RelayCommand RefreshCommand { get; }
        public RelayCommand AddCommand { get; }
        public RelayCommand EditCommand { get; }
        public RelayCommand DeleteCommand { get; }

        public async Task LoadAsync()
        {
            Items.Clear();
            var list = await _service.GetAsync();     // NoTracking
            foreach (var x in list) Items.Add(x);
        }

        private void OpenEditor(Classroom model)
        {
            var dlg = _dialogFactory();
            dlg.Owner = Application.Current.MainWindow;

            // ViewModel'i hazırlıyoruz; UI'dan gelen entity'i kopyalayalım (edit için).
            var vm = new EditClassroomDialogViewModel(_service);

            if (model != null)
            {
                vm.ClassroomId = model.ClassroomId;
                vm.Code = model.Code;
                vm.Name = model.Name;
                vm.Rows = model.Rows;
                vm.Columns = model.Columns;
                vm.SeatGroupSize = model.SeatGroupSize;
            }

            dlg.DataContext = vm;

            if (dlg.ShowDialog() == true)
            {
                // ÖNEMLİ: Items üzerinde ekleme/insert yapma.
                // SADECE yeniden yükle → bu sayede duplikasyon oluşmaz
                _ = LoadAsync();
            }
        }

        private async Task DeleteAsync()
        {
            if (Selected == null) return;

            var r = MessageBox.Show($"'{Selected.Code}' sınıfını silmek istiyor musun?",
                                    "Onay", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (r != MessageBoxResult.Yes) return;

            await _service.DeleteAsync(Selected.ClassroomId);
            await LoadAsync();
        }
    }
}
