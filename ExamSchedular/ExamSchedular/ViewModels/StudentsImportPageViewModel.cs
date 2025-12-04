using System.Collections.ObjectModel;
using System.Threading.Tasks;
using ExamSchedular.Business;
using ExamSchedular.Core;
using Microsoft.Win32;

namespace ExamSchedular.UI.ViewModels
{
    public class StudentsImportPageViewModel : ViewModelBase
    {
        private readonly IExcelImportService _excel;
        private readonly IImportWriterService _writer;

        public StudentsImportPageViewModel(IExcelImportService excel, IImportWriterService writer)
        {
            _excel = excel;
            _writer = writer;

            ChooseFileCommand = new RelayCommand(ChooseFile);
            LoadPreviewCommand = new RelayCommand(async () => await LoadPreviewAsync(), () => !string.IsNullOrWhiteSpace(FilePath));
            SaveCommand = new RelayCommand(async () => await SaveAsync(), () => Items.Count > 0);
            ClearCommand = new RelayCommand(Clear);
        }

        private string? _filePath;
        public string? FilePath
        {
            get => _filePath;
            set { if (SetProperty(ref _filePath, value)) LoadPreviewCommand.RaiseCanExecuteChanged(); }
        }

        public ObservableCollection<StudentRow> Items { get; } = new();
        public ObservableCollection<string> Errors { get; } = new();

        private string? _status;
        public string? Status { get => _status; set => SetProperty(ref _status, value); }

        public RelayCommand ChooseFileCommand { get; }
        public RelayCommand LoadPreviewCommand { get; }
        public RelayCommand SaveCommand { get; }
        public RelayCommand ClearCommand { get; }

        private void ChooseFile()
        {
            var dlg = new OpenFileDialog
            {
                Filter = "Excel Files (*.xlsx)|*.xlsx",
                Title = "Öğrenci Excel'i Seç"
            };
            if (dlg.ShowDialog() == true)
            {
                FilePath = dlg.FileName;
            }
        }

        private async Task LoadPreviewAsync()
        {
            Items.Clear();
            Errors.Clear();
            Status = "Yükleniyor...";

            var list = await _excel.ReadStudentsAsync(FilePath!);
            foreach (var r in list) Items.Add(r);

            Status = $"Önizleme: {Items.Count} öğrenci";
            SaveCommand.RaiseCanExecuteChanged();
        }

        private async Task SaveAsync()
        {
            Errors.Clear();
            Status = "Kaydediliyor...";

            var result = await _writer.SaveStudentsAsync(new System.Collections.Generic.List<StudentRow>(Items));

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
    }
}
