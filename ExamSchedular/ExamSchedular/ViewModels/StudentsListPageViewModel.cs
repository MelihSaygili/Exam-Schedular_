using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ExamSchedular.Business;
using ExamSchedular.Core;
using ExamSchedular.Data.Entities;

namespace ExamSchedular.UI.ViewModels
{
    public sealed class StudentsListPageViewModel : ViewModelBase
    {
        private readonly IStudentService _students;

        public StudentsListPageViewModel(IStudentService students)
        {
            _students = students;
            RefreshCommand = new RelayCommand(async () => await LoadAsync());
            SearchCommand = new RelayCommand(async () => await SearchAsync());
            ClearCommand = new RelayCommand(async () => await ClearAsync());
        }

        // --- Öğrenci listesi ---
        public ObservableCollection<Student> Items { get; } = new();

        private string? _searchText;
        public string? SearchText
        {
            get => _searchText;
            set => SetProperty(ref _searchText, value);
        }

        public RelayCommand RefreshCommand { get; }
        public RelayCommand SearchCommand { get; }
        public RelayCommand ClearCommand { get; }

        private string? _status;
        public string? Status
        {
            get => _status;
            set => SetProperty(ref _status, value);
        }

        // --- Seçilen öğrencinin dersleri ---
        public ObservableCollection<CourseListItem> CoursesOfSelected { get; } = new();

        private Student? _selected;
        public Student? SelectedStudent
        {
            get => _selected;
            set
            {
                if (SetProperty(ref _selected, value))
                {
                    _ = LoadCoursesForSelectedAsync();
                }
            }
        }

        private CancellationTokenSource? _ctsCourses;

        public async Task LoadAsync()
        {
            Status = "Yükleniyor...";
            Items.Clear();

            var list = await _students.GetAsync(); // Koordinatörse kendi departmanı
            foreach (var s in list) Items.Add(s);

            Status = $"Toplam {Items.Count} öğrenci";
        }

        public async Task SearchAsync()
        {
            Status = "Aranıyor...";
            Items.Clear();

            var list = await _students.SearchAsync(SearchText ?? string.Empty);
            foreach (var s in list) Items.Add(s);

            Status = $"Bulunan {Items.Count} öğrenci";
        }

        private async Task ClearAsync()
        {
            SearchText = string.Empty;
            await LoadAsync();
        }

        private async Task LoadCoursesForSelectedAsync()
        {
            _ctsCourses?.Cancel();
            _ctsCourses = new CancellationTokenSource();
            var ct = _ctsCourses.Token;

            CoursesOfSelected.Clear();
            if (SelectedStudent == null) { Status = "Öğrenci seçiniz."; return; }

            Status = $"{SelectedStudent.NameSurname} dersleri yükleniyor...";

            var list = await _students.GetCoursesAsync(SelectedStudent.StudentId);
            if (ct.IsCancellationRequested) return;

            foreach (var c in list.OrderBy(x => x.Code))
                CoursesOfSelected.Add(new CourseListItem
                {
                    Code = c.Code,
                    Name = c.Name,
                    Instructor = c.Instructor?.Name ?? "-"
                });

            Status = $"{SelectedStudent.NameSurname} -> {CoursesOfSelected.Count} ders";
        }

        // UI’ya sade veri
        public sealed class CourseListItem
        {
            public string Code { get; set; } = "";
            public string Name { get; set; } = "";
            public string Instructor { get; set; } = "-";
        }
    }
}
