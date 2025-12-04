using System.Collections.Generic;
using System.Linq;

namespace ExamSchedular.Business
{
    public sealed class CourseRow
    {
        public string Code { get; set; } = "";
        public string Name { get; set; } = "";
        public string Instructor { get; set; } = "";
        public bool IsMandatory { get; set; }
    }

    public sealed class StudentRow
    {
        public string StudentNo { get; set; } = "";
        public string Name { get; set; } = "";
        public int ClassYear { get; set; }
        public List<string> Courses { get; set; } = new();

        // UI'da göstermek için:
        public string CoursesCsv => (Courses == null || Courses.Count == 0)
            ? ""
            : string.Join(", ", Courses.Distinct().OrderBy(x => x));
    }

    public sealed class ImportError
    {
        public int RowIndex { get; set; }
        public string Message { get; set; } = "";
    }

    public sealed class ImportResult
    {
        public int Inserted { get; set; }
        public int Updated { get; set; }
        public List<ImportError> Errors { get; } = new();
    }
}
