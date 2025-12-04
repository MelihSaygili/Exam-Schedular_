using System.Linq;
using System.Windows;
using Microsoft.EntityFrameworkCore;
using ExamSchedular.Data;
using Microsoft.Extensions.DependencyInjection; // gerek yok artık
using Microsoft.EntityFrameworkCore.Infrastructure; // opsiyonel
using Microsoft.EntityFrameworkCore; // AsNoTracking için
using System.Threading.Tasks;

namespace ExamSchedular.UI.Views
{
    public partial class AddUserDialog : Window
    {
        private readonly IDbContextFactory<AppDbContext> _factory;

        // DI buraya factory enjekte edecek
        public AddUserDialog(IDbContextFactory<AppDbContext> factory)
        {
            InitializeComponent();
            _factory = factory;

            // UI hazır olduğunda asenkron yükle
            Loaded += async (_, __) => await LoadDepartmentsAsync();
        }

        private async Task LoadDepartmentsAsync()
        {
            try
            {
                await using var db = await _factory.CreateDbContextAsync();
                var items = await db.Departments
                                    .AsNoTracking()
                                    .OrderBy(d => d.Name)
                                    .Select(d => new { d.DepartmentId, d.Name })
                                    .ToListAsync();

                cmbDept.ItemsSource = items;
                cmbDept.DisplayMemberPath = "Name";
                cmbDept.SelectedValuePath = "DepartmentId";
            }
            catch (System.Exception ex)
            {
                MessageBox.Show("Bölümler yüklenemedi: " + ex.Message,
                    "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public string? Email { get; private set; }
        public string? Password { get; private set; }
        public int? DepartmentId { get; private set; }

        private void Ok_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtEmail.Text) ||
                string.IsNullOrWhiteSpace(pwd.Password) ||
                cmbDept.SelectedValue == null)
            {
                MessageBox.Show("E-posta, parola ve bölüm zorunludur.");
                return;
            }

            Email = txtEmail.Text.Trim();
            Password = pwd.Password;
            DepartmentId = (int)cmbDept.SelectedValue;
            DialogResult = true;
        }

        private void Cancel_Click(object sender, RoutedEventArgs e) => DialogResult = false;
    }
}
