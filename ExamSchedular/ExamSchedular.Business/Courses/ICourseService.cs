using System.Collections.Generic;
using System.Threading.Tasks;
using ExamSchedular.Data.Entities;

namespace ExamSchedular.Business
{
    public interface ICourseService
    {
        Task<List<Course>> GetAsync();
        Task<Course?> GetByIdAsync(int id);
        Task<Course> AddOrUpdateAsync(Course c);
        Task<bool> DeleteAsync(int id);

        Task<List<Course>> SearchAsync(string search);

        Task<List<Student>> GetStudentsAsync(int courseId);
    }
}
