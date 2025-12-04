using System.Collections.Generic;
using System.Threading.Tasks;

namespace ExamSchedular.Business
{
    public interface IExcelImportService
    {
        // varsayılan kolon eşleşmesi: Code(A)=1, Name(B)=2, Instructor(C)=3, Mandatory(D)=4
        Task<List<CourseRow>> ReadCoursesAsync(string path,
            int colCode = 1, int colName = 2, int colInstructor = 3, int colMandatory = 4,
            bool hasHeader = true);

        // varsayılan: No(A)=1, Name(B)=2, Class(C)=3, Dersler(D'den itibaren CSV ya da çoklu hücre)
        Task<List<StudentRow>> ReadStudentsAsync(string path,
            int colNo = 1, int colName = 2, int colClass = 3, int colCourses = 4,
            bool hasHeader = true);
    }
}
