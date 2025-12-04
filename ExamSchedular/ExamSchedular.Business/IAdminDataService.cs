using System.Threading.Tasks;

namespace ExamSchedular.Business
{
    public interface IAdminDataService
    {
        // Course
        Task<int> DeleteCourseByCodeAsync(string code);
        Task<int> DeleteAllCoursesAsync();

        // Student
        Task<int> DeleteStudentByNoAsync(string studentNo);
        Task<int> DeleteAllStudentsAsync();
    }
}
