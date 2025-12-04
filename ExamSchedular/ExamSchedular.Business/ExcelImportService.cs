using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ClosedXML.Excel;

namespace ExamSchedular.Business
{
    public class ExcelImportService : IExcelImportService
    {
        // Örn. BLM401, MUH403, AIT109, YDB117 ... (Türkçe harfleri de kapsayalım)
        private static readonly Regex CodeRegex =
            new Regex(@"^[A-Za-zÇĞİÖŞÜ]{2,5}\s?\d{3,4}$", RegexOptions.Compiled);

        public Task<List<CourseRow>> ReadCoursesAsync(
            string path,
            int colCode = 1, int colName = 2, int colInstructor = 3, int colMandatory = 4,
            bool hasHeader = true)
        {
            var list = new List<CourseRow>();
            using var wb = new XLWorkbook(path);
            var ws = wb.Worksheets.First();

            var firstRow = ws.FirstRowUsed().RowNumber();
            var lastRow = ws.LastRowUsed().RowNumber();

            var startRow = firstRow;
            if (hasHeader && LooksLikeCourseHeader(ws.Row(firstRow))) startRow++;

            bool currentMandatory = true; // default: zorunlu
            for (int r = startRow; r <= lastRow; r++)
            {
                var row = ws.Row(r);

                string a = row.Cell(colCode).GetString().Trim();       // Kod / ayraç yazısı
                string b = row.Cell(colName).GetString().Trim();       // Ad  / ayraç yazısı
                string c = row.Cell(colInstructor).GetString().Trim(); // Hoca
                string d = row.Cell(colMandatory).GetString().Trim();  // Zorunlu bilgisi metin ise

                if (string.IsNullOrWhiteSpace(a) &&
                    string.IsNullOrWhiteSpace(b) &&
                    string.IsNullOrWhiteSpace(c) &&
                    string.IsNullOrWhiteSpace(d))
                {
                    continue;
                }

                if (IsSeparatorOrHeader(a) || IsSeparatorOrHeader(b))
                {
                    if (ContainsSecmeli(a) || ContainsSecmeli(b))
                        currentMandatory = false;

                    if (ContainsZorunlu(a) || ContainsZorunlu(b))
                        currentMandatory = true;

                    continue;
                }

                if (!CodeRegex.IsMatch(a))
                    continue;

                bool isMandatory = currentMandatory;
                if (TryParseMandatory(d, out bool parsed))
                    isMandatory = parsed;

                list.Add(new CourseRow
                {
                    Code = a,
                    Name = b,
                    Instructor = c,
                    IsMandatory = isMandatory
                });
            }

            return Task.FromResult(list);
        }

        public Task<List<StudentRow>> ReadStudentsAsync(
            string path, int colNo = 1, int colName = 2, int colClass = 3, int colCourses = 4,
            bool hasHeader = true)
        {
            var list = new List<StudentRow>();
            using var wb = new XLWorkbook(path);
            var ws = wb.Worksheets.First();

            var firstRow = ws.FirstRowUsed().RowNumber();
            var lastRow = ws.LastRowUsed().RowNumber();

            int startRow = firstRow;
            if (hasHeader && LooksLikeStudentHeader(ws.Row(firstRow))) startRow++;

            for (int r = startRow; r <= lastRow; r++)
            {
                var row = ws.Row(r);

                string no = row.Cell(colNo).GetString().Trim();
                string name = row.Cell(colName).GetString().Trim();
                int classYear = ParseInt(row.Cell(colClass).GetString());

                var codes = new List<string>();
                var firstCoursesCell = row.Cell(colCourses).GetString();

                if (!string.IsNullOrWhiteSpace(firstCoursesCell) &&
                    (firstCoursesCell.Contains(',') || firstCoursesCell.Contains(';')))
                {
                    codes.AddRange(firstCoursesCell
                        .Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries)
                        .Select(s => s.Trim()));
                }
                else
                {
                    int c = colCourses;
                    while (true)
                    {
                        var val = row.Cell(c).GetString().Trim();
                        if (string.IsNullOrWhiteSpace(val)) break;
                        codes.Add(val);
                        c++;
                    }
                }

                if (string.IsNullOrWhiteSpace(no) &&
                    string.IsNullOrWhiteSpace(name) &&
                    codes.Count == 0)
                    continue;

                list.Add(new StudentRow
                {
                    StudentNo = no,
                    Name = name,
                    ClassYear = classYear,
                    Courses = codes
                });
            }

            return Task.FromResult(list);
        }

        // ---------------- helpers ----------------

        private static bool LooksLikeCourseHeader(IXLRow row)
        {
            var s1 = row.Cell(1).GetString().ToLowerInvariant();
            var s2 = row.Cell(2).GetString().ToLowerInvariant();
            return s1.Contains("ders") || s1.Contains("kod") || s2.Contains("ders");
        }

        private static bool LooksLikeStudentHeader(IXLRow row)
        {
            var s1 = row.Cell(1).GetString().ToLowerInvariant();
            var s2 = row.Cell(2).GetString().ToLowerInvariant();
            return s1.Contains("öğrenci") || s1.Contains("ogrenci") || s2.Contains("ad");
        }

        private static bool IsSeparatorOrHeader(string s)
        {
            if (string.IsNullOrWhiteSpace(s)) return false;
            var t = s.Trim().ToLowerInvariant();

            return t.Contains("sınıf") ||
                   t.Contains("seçmeli") || t.Contains("seçimlik") ||
                   t.Contains("ders kodu") || t.Contains("dersin adı") ||
                   t.Contains("öğretim üyesi") || t.Contains("dersi veren") ||
                   t == "ders kodu" || t == "ders" || t == "kod";
        }

        private static bool ContainsSecmeli(string s)
        {
            if (string.IsNullOrWhiteSpace(s)) return false;
            var t = s.ToLowerInvariant();
            return t.Contains("seçmeli") || t.Contains("seçimlik");
        }

        private static bool ContainsZorunlu(string s)
        {
            if (string.IsNullOrWhiteSpace(s)) return false;
            return s.ToLowerInvariant().Contains("zorunlu");
        }

        private static bool TryParseMandatory(string s, out bool val)
        {
            val = false;
            if (string.IsNullOrWhiteSpace(s)) return false;
            var t = s.Trim().ToLowerInvariant();
            if (t == "z" || t.StartsWith("zorun")) { val = true; return true; }
            if (t == "s" || t.StartsWith("seç") || t.StartsWith("sec")) { val = false; return true; }
            if (bool.TryParse(t, out var b)) { val = b; return true; }
            if (t == "1") { val = true; return true; }
            if (t == "0") { val = false; return true; }
            return false;
        }

        private static int ParseInt(string s)
        {
            if (int.TryParse(s, NumberStyles.Integer, CultureInfo.InvariantCulture, out var n))
                return n;

            var t = (s ?? "").ToLowerInvariant();
            for (int i = 1; i <= 8; i++)
                if (t.Contains($"{i}.") || t.Contains($"{i} "))
                    return i;
            return 0;
        }
    }
}
