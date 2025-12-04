using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ExamSchedular.Business;
using ExamSchedular.Business.DTOs;
using ExamSchedular.Core;

namespace ExamSchedular.UI.ViewModels
{
    public class ProgramCreatePageViewModel : ViewModelBase
    {
        private readonly ISchedulingService _sched;
        private readonly ICurrentUser _current;

        public ProgramCreatePageViewModel(ISchedulingService sched, ICurrentUser current)
        {
            _sched = sched;
            _current = current;

            Title = "Program Oluştur";
            StartDate = DateTime.Today.AddDays(7);
            Days = 5;
            StartTimeText = "09:00";
            EndTimeText = "17:00";
            SlotMinutes = 90;
            AvoidStudentConflict = true;
            SpreadByDay = true;

            PreviewCommand = new RelayCommand(async () => await DoPreviewAsync());
            SaveCommand = new RelayCommand(async () => await SaveAsync(), () => PreviewItems.Any(p => p.Include));
            ExportXlsxCommand = new RelayCommand(async () => await ExportXlsxAsync());

            // Sayfa açıldığında mevcut kaydedilmiş planı getir
            _ = LoadExistingAsync();
        }

        // ---------------- Header / Parametreler ----------------
        public string Title { get; }

        public DateTime StartDate
        {
            get => _startDate;
            set
            {
                if (SetProperty(ref _startDate, value))
                {
                    OnPropertyChanged(nameof(SummaryText));
                    // Önizleme kapalıysa mevcut kaydı otomatik yenile
                    if (!IsPreviewMode) _ = LoadExistingAsync();
                }
            }
        }
        private DateTime _startDate;

        public int Days
        {
            get => _days;
            set
            {
                if (SetProperty(ref _days, value))
                {
                    OnPropertyChanged(nameof(SummaryText));
                    if (!IsPreviewMode) _ = LoadExistingAsync();
                }
            }
        }
        private int _days;

        public string StartTimeText { get => _startTimeText; set => SetProperty(ref _startTimeText, value); }
        private string _startTimeText;

        public string EndTimeText { get => _endTimeText; set => SetProperty(ref _endTimeText, value); }
        private string _endTimeText;

        public int SlotMinutes { get => _slotMinutes; set => SetProperty(ref _slotMinutes, value); }
        private int _slotMinutes;

        public bool AvoidStudentConflict { get => _avoidStudentConflict; set => SetProperty(ref _avoidStudentConflict, value); }
        private bool _avoidStudentConflict;

        public bool SpreadByDay { get => _spreadByDay; set => SetProperty(ref _spreadByDay, value); }
        private bool _spreadByDay;

        // ---------------- Koleksiyonlar / Durum ----------------
        public ObservableCollection<PlannedExamItemDto> Items { get; } = new();
        public ObservableCollection<PreviewRowVm> PreviewItems { get; } = new();
        public ObservableCollection<string> Conflicts { get; } = new();

        public bool IsPreviewMode
        {
            get => _isPreviewMode;
            set
            {
                if (SetProperty(ref _isPreviewMode, value))
                {
                    OnPropertyChanged(nameof(SummaryText));
                    SaveCommand.RaiseCanExecuteChanged();
                }
            }
        }
        private bool _isPreviewMode;

        public string Status { get => _status; set => SetProperty(ref _status, value); }
        private string _status;

        public string SummaryText =>
            IsPreviewMode
                ? $"Önizleme {PreviewItems.Count} ders · Not {Conflicts.Count}"
                : $"Toplam {Items.Count} sınav · Uyarı/Hata {Conflicts.Count}";

        // ---------------- Komutlar ----------------
        public RelayCommand PreviewCommand { get; }
        public RelayCommand SaveCommand { get; }
        public RelayCommand ExportXlsxCommand { get; }

        // ---------------- Yardımcılar ----------------
        private async Task LoadExistingAsync()
        {
            try
            {
                var depId = _current.DepartmentId ?? 0;
                if (depId <= 0 && !_current.IsAdmin) return;

                var start = DateOnly.FromDateTime(StartDate.Date);
                var end = DateOnly.FromDateTime(StartDate.Date.AddDays(Math.Max(0, Days - 1)));

                var rows = await _sched.GetPlanAsync(depId, start, end);

                Items.Clear();
                foreach (var r in rows.OrderBy(x => x.Date).ThenBy(x => x.StartTime))
                    Items.Add(r);

                IsPreviewMode = false;
                Status = $"Yüklendi: {Items.Count} sınav";
            }
            catch (Exception ex)
            {
                Status = "Plan yüklenemedi: " + ex.GetBaseException().Message;
            }
        }

        // ---------------- Önizleme ----------------
        private async Task DoPreviewAsync()
        {
            Conflicts.Clear();
            PreviewItems.Clear();
            Items.Clear();
            IsPreviewMode = true;
            Status = "Önizleme hazırlanıyor...";

            if (!TimeSpan.TryParse(StartTimeText, out var t1)) { Status = "Başlama saati hatalı (örn: 09:00)."; return; }
            if (!TimeSpan.TryParse(EndTimeText, out var t2)) { Status = "Bitiş saati hatalı (örn: 17:00)."; return; }
            if (t2 <= t1) { Status = "Bitiş saati başlangıçtan sonra olmalı."; return; }
            if (SlotMinutes < 30) { Status = "Slot süresi en az 30 dk olmalı."; return; }
            if (Days <= 0) { Status = "Gün sayısı pozitif olmalı."; return; }

            var depId = _current.DepartmentId ?? 0;
            if (depId <= 0 && !_current.IsAdmin) { Status = "Departman belirlenemedi."; return; }

            var req = new ScheduleRequest
            {
                DepartmentId = depId,
                StartDate = DateOnly.FromDateTime(StartDate.Date),
                EndDate = DateOnly.FromDateTime(StartDate.Date.AddDays(Days - 1)),
                DayStart = TimeOnly.FromTimeSpan(t1),
                DayEnd = TimeOnly.FromTimeSpan(t2),
                SlotStepMinutes = SlotMinutes,
                EnforceNoOverlapForSameStudent = AvoidStudentConflict,
                SpreadByClassYear = SpreadByDay
            };
            req.ExcludedDays.Add(DayOfWeek.Sunday);

            var result = await _sched.GenerateAsync(req);
            foreach (var e in result.Errors) Conflicts.Add("[HATA] " + e);
            foreach (var w in result.Warnings) Conflicts.Add("[UYARI] " + w);

            Action includeChanged = () => SaveCommand.RaiseCanExecuteChanged();

            foreach (var p in result.Preview.OrderBy(x => x.Date)
                                            .ThenBy(x => x.StartTime)
                                            .ThenBy(x => x.CourseCode))
            {
                PreviewItems.Add(new PreviewRowVm(includeChanged)
                {
                    Include = true,
                    CourseId = p.CourseId,
                    CourseCode = p.CourseCode,
                    CourseName = p.CourseName,
                    Date = p.Date,
                    StartTime = p.StartTime,
                    StudentCount = p.StudentCount,
                    RoomsCsv = p.RoomsCsv ?? string.Empty,
                    Duration = p.SuggestedDurationMinutes > 0 ? p.SuggestedDurationMinutes : 90
                });
            }

            Status = $"Önizleme: {PreviewItems.Count} ders · Not: {Conflicts.Count}";
            OnPropertyChanged(nameof(SummaryText));
            SaveCommand.RaiseCanExecuteChanged();
        }

        // ---------------- Kaydet ----------------
        private async Task SaveAsync()
        {
            var depId = _current.DepartmentId ?? 0;
            if (depId <= 0 && !_current.IsAdmin) { Status = "Departman belirlenemedi."; return; }

            var include = PreviewItems.Where(p => p.Include)
                                      .Select(p => p.CourseId)
                                      .Distinct()
                                      .ToList();

            if (include.Count == 0)
            {
                Status = "Seçili ders yok.";
                return;
            }

            var overrides = PreviewItems
                .Where(p => p.Include && p.Duration > 0)
                .GroupBy(p => p.CourseId)
                .ToDictionary(g => g.Key, g => g.First().Duration);

            var req = new ScheduleRequest
            {
                DepartmentId = depId,
                StartDate = DateOnly.FromDateTime(StartDate.Date),
                EndDate = DateOnly.FromDateTime(StartDate.Date.AddDays(Days - 1)),
                DayStart = TimeOnly.Parse(StartTimeText),
                DayEnd = TimeOnly.Parse(EndTimeText),
                SlotStepMinutes = SlotMinutes,
                EnforceNoOverlapForSameStudent = AvoidStudentConflict,
                SpreadByClassYear = SpreadByDay,
                IncludedCourseIds = include,
                DurationOverrideMinutes = overrides
            };
            req.ExcludedDays.Add(DayOfWeek.Sunday);

            var result = await _sched.BuildAsync(req);

            Conflicts.Clear();
            foreach (var e in result.Errors) Conflicts.Add("[HATA] " + e);
            foreach (var w in result.Warnings) Conflicts.Add("[UYARI] " + w);

            if (!result.Success)
            {
                Status = $"Program kaydedilemedi. Not: {Conflicts.Count}";
                return;
            }

            // Kaydedilen planı göster
            Items.Clear();
            var rows = await _sched.GetPlanAsync(depId, req.StartDate, req.EndDate);
            foreach (var r in rows.OrderBy(x => x.Date).ThenBy(x => x.StartTime))
                Items.Add(r);

            // Önizlemeyi kapat ve temizle
            PreviewItems.Clear();
            IsPreviewMode = false;
            Status = $"Kaydedildi: {Items.Count} sınav · Not: {Conflicts.Count}";
            OnPropertyChanged(nameof(SummaryText));
            SaveCommand.RaiseCanExecuteChanged();
        }

        // ---------------- XLSX Export ----------------
        private async Task ExportXlsxAsync()
        {
            var depId = _current.DepartmentId ?? 0;
            if (depId <= 0 && !_current.IsAdmin) { Status = "Departman belirlenemedi."; return; }

            var start = DateOnly.FromDateTime(StartDate.Date);
            var end = DateOnly.FromDateTime(StartDate.Date.AddDays(Math.Max(0, Days - 1)));

            var bytes = await _sched.ExportExcelAsync(depId, start, end);
            var path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
                                    $"Plan_{DateTime.Now:yyyyMMdd_HHmm}.xlsx");
            File.WriteAllBytes(path, bytes);
            Status = $"Excel kaydedildi: {path}";
        }

        // ---------------- Önizleme satırı VM ----------------
        public sealed class PreviewRowVm : ViewModelBase
        {
            private readonly Action _onIncludeChanged;
            public PreviewRowVm(Action onIncludeChanged) { _onIncludeChanged = onIncludeChanged; }

            public bool Include
            {
                get => _include;
                set { if (SetProperty(ref _include, value)) _onIncludeChanged?.Invoke(); }
            }
            private bool _include = true;

            public int CourseId { get; set; }
            public string CourseCode { get; set; } = string.Empty;
            public string CourseName { get; set; } = string.Empty;
            public DateOnly Date { get; set; }
            public TimeOnly StartTime { get; set; }
            public int StudentCount { get; set; }
            public string RoomsCsv { get; set; } = string.Empty;

            public int Duration
            {
                get => _duration;
                set => SetProperty(ref _duration, value);
            }
            private int _duration = 90;

            public string DateText => Date.ToString("yyyy-MM-dd");
            public string TimeText => StartTime.ToString("HH:mm");
        }
    }
}
