namespace ExamSchedular.Business
{
    public interface IAuthorizationService
    {
        // Admin veya bir bölüme atanmýþ kullanýcýlar (koordinatörler) eriþebilir
        bool CanManageDepartmentData();
        bool IsAdmin();
    }
}