using System.Linq;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using ExamSchedular.Business;
using ExamSchedular.Core;
using Microsoft.Win32;

namespace ExamSchedular.UI.ViewModels
{
    public class CoursesImportPageViewModel : ViewModelBase
    {
        private readonly IExcelImportService _excel;
        private readonly IImportWriterService _writer;
        private readonly IAdminDataService _admin;

        public CoursesImportPageViewModel(
            IExcelImportService excel,
            IImportWriterService writer,
            IAdminDataService admin)
        {
            _excel = excel;
            _writer = writer;
            _admin = admin;

            ChooseFileCommand = new RelayCommand(ChooseFile);
            LoadPreviewCommand = new RelayCommand(async () => await LoadPreviewAsync(),
                                                   () => !string.IsNullOrWhiteSpace(FilePath));
            SaveCommand = new RelayCommand(async () => await SaveAsync(),
                                                   () => Items.Any(x => !string.IsNullOrWhiteSpace(x.Code) ||
                                                                        !string.IsNullOrWhiteSpace(x.Name)));
            ClearCommand = new RelayCommand(Clear);

            AddEmptyRowCommand = new RelayCommand(AddEmptyRow);
            DeleteRowCommand = new RelayCommand<CourseRow>(DeleteRow);

            DeleteSelectedFromDbCommand = new RelayCommand(async () => await DeleteSelectedFromDb(),
                                                           () => SelectedItem != null && !string.IsNullOrWhiteSpace(SelectedItem?.Code));
            DeleteAllFromDbCommand = new RelayCommand(async () => await DeleteAllFromDb());
        }

        private string _filePath;
        public string FilePath
        {
            get => _filePath;
            set
            {
                if (SetProperty(ref _filePath, value))
                    LoadPreviewCommand.RaiseCanExecuteChanged();
            }
        }

        public ObservableCollection<CourseRow> Items { get; } = new();
        public ObservableCollection<string> Errors { get; } = new();

        private CourseRow _selectedItem;
        public CourseRow SelectedItem
        {
            get => _selectedItem;
            set
            {
                if (SetProperty(ref _selectedItem, value))
                    DeleteSelectedFromDbCommand.RaiseCanExecuteChanged();
            }
        }

        private string _status;
        public string Status { get => _status; set => SetProperty(ref _status, value); }

        public RelayCommand ChooseFileCommand { get; }
        public RelayCommand LoadPreviewCommand { get; }
        public RelayCommand SaveCommand { get; }
        public RelayCommand ClearCommand { get; }
        public RelayCommand AddEmptyRowCommand { get; }
        public RelayCommand<CourseRow> DeleteRowCommand { get; }

        public RelayCommand DeleteSelectedFromDbCommand { get; }
        public RelayCommand DeleteAllFromDbCommand { get; }

        private void ChooseFile()
        {
            var dlg = new OpenFileDialog
            {
                Filter = "Excel Files (*.xlsx)|*.xlsx",
                Title = "Ders Excel'i Seç"
            };
            if (dlg.ShowDialog() == true)
                FilePath = dlg.FileName;
        }

        private async Task LoadPreviewAsync()
        {
            Items.Clear();
            Errors.Clear();
            Status = "Yükleniyor...";

            var list = await _excel.ReadCoursesAsync(FilePath);
            foreach (var r in list) Items.Add(r);

            Status = $"Önizleme: {Items.Count} satır";
            SaveCommand.RaiseCanExecuteChanged();
        }

        private async Task SaveAsync()
        {
            Errors.Clear();
            Status = "Kaydediliyor...";

            var toSave = Items
                .Where(r => !string.IsNullOrWhiteSpace(r.Code) || !string.IsNullOrWhiteSpace(r.Name))
                .ToList();

            var result = await _writer.SaveCoursesAsync(toSave);

            foreach (var e in result.Errors)
                Errors.Add($"Satır {e.RowIndex}: {e.Message}");

            Status = $"Ekle: {result.Inserted}, Güncelle: {result.Updated}, Hata: {result.Errors.Count}";
        }

        private void Clear()
        {
            FilePath = null;
            Items.Clear();
            Errors.Clear();
            Status = null;
            SaveCommand.RaiseCanExecuteChanged();
            LoadPreviewCommand.RaiseCanExecuteChanged();
        }

        private void AddEmptyRow()
        {
            Items.Add(new CourseRow());
            SaveCommand.RaiseCanExecuteChanged();
        }

        private void DeleteRow(CourseRow row)
        {
            if (row == null) return;
            Items.Remove(row);
            SaveCommand.RaiseCanExecuteChanged();
        }

        // ---- DB Delete Commands ----
        private async Task DeleteSelectedFromDb()
        {
            if (SelectedItem == null || string.IsNullOrWhiteSpace(SelectedItem.Code)) return;

            var affected = await _admin.DeleteCourseByCodeAsync(SelectedItem.Code);
            Status = affected > 0
                ? $"DB: '{SelectedItem.Code}' silindi."
                : $"DB: '{SelectedItem.Code}' bulunamadı.";
        }

        private async Task DeleteAllFromDb()
        {
            var affected = await _admin.DeleteAllCoursesAsync();
            Status = $"DB: Tüm dersler silindi (etkilenen kayıt: {affected}).";
        }
    }
}
