using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using ExamSchedular.Business;
using ExamSchedular.Core;
using ExamSchedular.Data.Entities;

namespace ExamSchedular.UI.ViewModels
{
    public class CoursesListPageViewModel : ViewModelBase
    {
        private readonly ICourseService _courses;

        public CoursesListPageViewModel(ICourseService courses)
        {
            _courses = courses;
            SearchCommand = new RelayCommand(async () => await LoadAsync());
            ClearCommand = new RelayCommand(async () => { Search = null; await LoadAsync(); });

            _ = LoadAsync();
        }

        private string _search;
        public string Search
        {
            get => _search;
            set => SetProperty(ref _search, value);
        }

        public ObservableCollection<Course> Items { get; } = new();

        private Course _selectedItem;
        public Course SelectedItem
        {
            get => _selectedItem;
            set => SetProperty(ref _selectedItem, value);
        }

        public RelayCommand SearchCommand { get; }
        public RelayCommand ClearCommand { get; }

        public async Task LoadAsync()
        {
            Items.Clear();
            var list = await _courses.SearchAsync(Search);
            foreach (var c in list) Items.Add(c);
        }
    }
}
