using System.Collections.Generic;
using System.Threading.Tasks;
using ExamSchedular.Data.Entities;

namespace ExamSchedular.Business
{
    public interface IStudentService
    {
        Task<List<Student>> SearchAsync(string search);   // <-- Liste için
        Task<List<Student>> GetAsync();                   // (opsiyonel, tam liste)
        Task<Student?> GetByIdAsync(int id);              // (opsiyonel, detay)
        Task<Student> AddOrUpdateAsync(Student s);        // (opsiyonel)
        Task<bool> DeleteAsync(int id);                   // (opsiyonel)

        Task<List<Course>> GetCoursesAsync(int studentId); // <-- Öğrencinin dersleri için
    }
}
