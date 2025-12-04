using System;
using System.Threading.Tasks;
using ExamSchedular.Business;
using ExamSchedular.Core;
using ExamSchedular.Data.Entities;

namespace ExamSchedular.UI.ViewModels
{
    public class EditClassroomDialogViewModel : ViewModelBase
    {
        private readonly IClassroomService _service;

        // Diyalog kapama callback
        public Action<bool?>? RequestClose { get; set; }

        public EditClassroomDialogViewModel(IClassroomService service)
        {
            _service = service;
            SaveCommand = new RelayCommand(async () => await SaveAsync(), CanSave);
            CancelCommand = new RelayCommand(() => RequestClose?.Invoke(false));
        }

        public int ClassroomId { get; set; }

        private string _code = "";
        public string Code
        {
            get => _code;
            set { if (SetProperty(ref _code, value)) SaveCommand.RaiseCanExecuteChanged(); }
        }

        private string _name = "";
        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }

        private int _rows;
        public int Rows
        {
            get => _rows;
            set
            {
                if (SetProperty(ref _rows, value))
                {
                    RecalcCapacityIfNotTouched();
                    SaveCommand.RaiseCanExecuteChanged();
                }
            }
        }

        private int _columns;
        public int Columns
        {
            get => _columns;
            set
            {
                if (SetProperty(ref _columns, value))
                {
                    RecalcCapacityIfNotTouched();
                    SaveCommand.RaiseCanExecuteChanged();
                }
            }
        }

        private int _seatGroupSize = 1;
        public int SeatGroupSize
        {
            get => _seatGroupSize;
            set { if (SetProperty(ref _seatGroupSize, value)) SaveCommand.RaiseCanExecuteChanged(); }
        }

        private bool _capacityTouched;     // kullanıcı elle mi değiştirdi?
        private int _capacity;
        public int Capacity
        {
            get => _capacity;
            set
            {
                if (SetProperty(ref _capacity, value))
                {
                    _capacityTouched = true; // bundan sonra otomatik hesaplama bozma
                }
            }
        }

        public int DepartmentId { get; set; }

        public string Title => ClassroomId == 0 ? "Sınıf Ekle" : "Sınıf Düzenle";

        public RelayCommand SaveCommand { get; }
        public RelayCommand CancelCommand { get; }

        public async Task LoadAsync(int classroomId)
        {
            if (classroomId <= 0) return;
            var c = await _service.GetByIdAsync(classroomId);
            if (c == null) return;

            ClassroomId = c.ClassroomId;
            Code = c.Code;
            Name = c.Name;
            Rows = c.Rows;
            Columns = c.Columns;
            SeatGroupSize = c.SeatGroupSize;
            Capacity = c.Capacity;
            DepartmentId = c.DepartmentId;

            _capacityTouched = (Capacity > 0); // var olan kapasite varsa dokunma
        }

        private bool CanSave()
            => !string.IsNullOrWhiteSpace(Code)
               && Rows > 0 && Columns > 0 && SeatGroupSize > 0;

        private void RecalcCapacityIfNotTouched()
        {
            if (!_capacityTouched)
            {
                // Basit hesap: Rows * Columns (istersen SeatGroupSize mantığı eklersin)
                Capacity = Math.Max(0, Rows) * Math.Max(0, Columns);
            }
        }

        private async Task SaveAsync()
        {
            // son güvenli hesap (kullanıcı boş bıraktıysa)
            if (Capacity <= 0) Capacity = Math.Max(0, Rows) * Math.Max(0, Columns);

            var model = new Classroom
            {
                ClassroomId = ClassroomId,
                Code = Code?.Trim(),
                Name = string.IsNullOrWhiteSpace(Name) ? Code : Name,
                Rows = Rows,
                Columns = Columns,
                SeatGroupSize = SeatGroupSize,
                Capacity = Capacity,
                DepartmentId = DepartmentId
            };

            var saved = await _service.AddOrUpdateAsync(model);
            // diyalogu OK ile kapat
            RequestClose?.Invoke(true);
        }
    }
}
