using System.Collections.Generic;
using System.Threading.Tasks;

namespace ExamSchedular.Business
{
    /// <summary>
    /// Excel'den okunan ders/öğrenci verilerinin kalıcı hâle getirilmesi ve
    /// toplu silme işlemleri için sözleşme.
    /// </summary>
    public interface IImportWriterService
    {
        Task<ImportResult> SaveCoursesAsync(List<CourseRow> rows);
        Task<ImportResult> SaveStudentsAsync(List<StudentRow> rows);

        /// <summary>Ders koduna göre tek bir dersi DB'den siler.</summary>
        Task<int> DeleteCourseByCodeAsync(string code);

        /// <summary>Tüm dersleri DB'den topluca siler.</summary>
        Task<int> DeleteAllCoursesAsync();
    }
}
