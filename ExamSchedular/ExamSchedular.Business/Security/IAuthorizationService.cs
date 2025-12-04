namespace ExamSchedular.Business
{
    public interface IAuthorizationService
    {
        // Admin sayfalarý (Users) için
        bool IsAdmin();

        // Admin veya bir bölüme atanmýþ kullanýcý (koordinatör) için
        bool CanManageDepartmentData();
    }
}