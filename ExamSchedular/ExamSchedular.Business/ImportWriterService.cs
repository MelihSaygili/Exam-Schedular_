using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ExamSchedular.Data;
using ExamSchedular.Data.Entities;

namespace ExamSchedular.Business
{
    public class ImportWriterService : IImportWriterService
    {
        private readonly IDbContextFactory<AppDbContext> _factory;
        private readonly ICurrentUser _current;

        public ImportWriterService(IDbContextFactory<AppDbContext> factory, ICurrentUser current)
        {
            _factory = factory;
            _current = current;
        }

        // ----------------- helpers -----------------
        private static string Norm(string s) => (s ?? string.Empty).Trim().ToLowerInvariant();

        private static readonly Regex CourseCodeRegex =
            new Regex(@"^[A-Za-zÇĞİÖŞÜ]{2,8}\s?\d{2,4}([A-Za-zÇĞİÖŞÜ]{0,2})?([\/-]\d{1,2})?$",
                      RegexOptions.Compiled);

        private static bool LooksLikeCourseCode(string s)
            => !string.IsNullOrWhiteSpace(s) && CourseCodeRegex.IsMatch(s.Trim());

        private static bool LooksLikeHeaderOrGroup(string code, string name)
        {
            var c = Norm(code);
            var n = Norm(name);

            if (string.IsNullOrWhiteSpace(c) && string.IsNullOrWhiteSpace(n))
                return true;

            if (LooksLikeCourseCode(code)) return false;

            if (c.Contains("seçmeli") || c.Contains("sec") || c.Contains("seçimlik")) return true;
            if (n.Contains("seçmeli") || n.Contains("sec") || n.Contains("seçimlik")) return true;

            if (c is "kod" or "code" or "ders" or "derskodu" or "dersadi" or "dersadı") return true;
            if (n is "ad" or "isim" or "ders" or "dersadı" or "dersadi") return true;

            return false;
        }

        // --------------------------------------------------------------------
        // COURSES
        // --------------------------------------------------------------------
        public async Task<ImportResult> SaveCoursesAsync(List<CourseRow> rows)
        {
            var depId = await ResolveDepartmentIdOrThrow();
            return await SaveCoursesInternalAsync(rows, depId);
        }

        public async Task<ImportResult> SaveCoursesAsync(List<CourseRow> rows, int departmentId)
        {
            // Bu overload dışarıdan departman id ile çağrılırsa
            return await SaveCoursesInternalAsync(rows, departmentId);
        }

        private async Task<ImportResult> SaveCoursesInternalAsync(List<CourseRow> rows, int departmentId)
        {
            var result = new ImportResult();
            await using var db = _factory.CreateDbContext();

            var instructors = await db.Instructors.ToListAsync();
            var courses = await db.Courses.ToListAsync();

            var instByName = instructors
                .GroupBy(i => Norm(i.Name))
                .ToDictionary(g => g.Key, g => g.First());

            var courseByCode = courses
                .GroupBy(c => Norm(c.Code))
                .ToDictionary(g => g.Key, g => g.First());

            int excelRow = 0;
            foreach (var r in rows)
            {
                excelRow++;

                var code = (r?.Code ?? string.Empty).Trim();
                var name = (r?.Name ?? string.Empty).Trim();
                var instName = (r?.Instructor ?? string.Empty).Trim();

                if (string.IsNullOrWhiteSpace(code) && string.IsNullOrWhiteSpace(name)) continue;
                if (LooksLikeHeaderOrGroup(code, name)) continue;

                if (string.IsNullOrWhiteSpace(code))
                {
                    result.Errors.Add(new ImportError { RowIndex = excelRow, Message = "Ders kodu boş geçilemez." });
                    continue;
                }

                Instructor? inst = null;
                if (!string.IsNullOrWhiteSpace(instName))
                {
                    var k = Norm(instName);
                    if (!instByName.TryGetValue(k, out inst))
                    {
                        inst = new Instructor { Name = instName };
                        db.Instructors.Add(inst);
                        instByName[k] = inst;
                    }
                }

                var cKey = Norm(code);
                if (!courseByCode.TryGetValue(cKey, out var existing))
                {
                    var c = new Course
                    {
                        Code = code,
                        Name = string.IsNullOrWhiteSpace(name) ? code : name,
                        IsMandatory = r?.IsMandatory ?? false,
                        DepartmentId = departmentId,
                        Instructor = inst
                    };
                    db.Courses.Add(c);
                    courseByCode[cKey] = c;
                    result.Inserted++;
                }
                else
                {
                    bool changed = false;

                    if (!string.IsNullOrWhiteSpace(name) && existing.Name != name)
                    { existing.Name = name; changed = true; }

                    if (r?.IsMandatory is bool m && existing.IsMandatory != m)
                    { existing.IsMandatory = m; changed = true; }

                    if (inst != null && existing.InstructorId != inst.InstructorId)
                    { existing.Instructor = inst; changed = true; }

                    if (existing.DepartmentId != departmentId)
                    { existing.DepartmentId = departmentId; changed = true; }

                    if (changed) result.Updated++;
                }
            }

            try { await db.SaveChangesAsync(); }
            catch (Exception ex)
            {
                result.Errors.Add(new ImportError
                {
                    RowIndex = 0,
                    Message = $"Veritabanına kaydederken hata: {ex.Message} - {ex.InnerException?.Message}"
                });
            }

            return result;
        }

        // --------------------------------------------------------------------
        // STUDENTS  (DEPARTMAN ZORUNLU!)
        // --------------------------------------------------------------------
        public async Task<ImportResult> SaveStudentsAsync(List<StudentRow> rows)
        {
            var result = new ImportResult();
            await using var db = _factory.CreateDbContext();

            // 1) Bu import hangi departmana yazacak?
            var depId = await ResolveDepartmentIdOrThrow();

            // 2) Öğrencileri ve dersleri al
            var students = await db.Students
                                   .Include(s => s.Enrollments)
                                   .ToListAsync();

            var courseDict = await db.Courses.AsNoTracking()
                                  .ToDictionaryAsync(c => Norm(c.Code), c => c);

            var studentByNo = students.ToDictionary(s => s.StudentNo, s => s);

            int excelRow = 0;
            foreach (var r in rows)
            {
                excelRow++;

                var no = (r?.StudentNo ?? string.Empty).Trim();
                var name = (r?.Name ?? string.Empty).Trim();

                if (string.IsNullOrWhiteSpace(no) && string.IsNullOrWhiteSpace(name) &&
                    (r?.Courses == null || r.Courses.Count == 0)) continue;

                if (string.IsNullOrWhiteSpace(no))
                {
                    result.Errors.Add(new ImportError { RowIndex = excelRow, Message = "Öğrenci numarası boş." });
                    continue;
                }

                if (!studentByNo.TryGetValue(no, out var student))
                {
                    // --- YENİ ÖĞRENCİ: DepartmentId mutlaka set ediliyor
                    student = new Student
                    {
                        StudentNo = no,
                        NameSurname = string.IsNullOrWhiteSpace(name) ? no : name,
                        ClassYear = r?.ClassYear ?? 0,
                        DepartmentId = depId
                    };
                    db.Students.Add(student);
                    students.Add(student);
                    studentByNo[no] = student;
                    result.Inserted++;
                }
                else
                {
                    bool changed = false;

                    if (!string.IsNullOrWhiteSpace(name) && student.NameSurname != name)
                    { student.NameSurname = name; changed = true; }

                    if ((r?.ClassYear ?? 0) > 0 && student.ClassYear != r!.ClassYear)
                    { student.ClassYear = r.ClassYear; changed = true; }

                    // Koordinatör ise öğrenciyi kendi departmanına bağla
                    if (!_current.IsAdmin && student.DepartmentId != depId)
                    { student.DepartmentId = depId; changed = true; }

                    if (changed) result.Updated++;
                }

                // --- Ders kayıtları (Enrollment)
                if (r?.Courses != null && r.Courses.Count > 0)
                {
                    student.Enrollments ??= new List<Enrollment>();
                    foreach (var raw in r.Courses)
                    {
                        var key = Norm(raw);
                        if (string.IsNullOrWhiteSpace(key)) continue;

                        if (!courseDict.TryGetValue(key, out var course))
                        {
                            result.Errors.Add(new ImportError { RowIndex = excelRow, Message = $"Ders bulunamadı: {raw}" });
                            continue;
                        }

                        if (!student.Enrollments.Any(e => e.CourseId == course.CourseId))
                            db.Enrollments.Add(new Enrollment { Student = student, CourseId = course.CourseId });
                    }
                }
            }

            await db.SaveChangesAsync();
            return result;
        }

        // ----------------- DELETE OPS -----------------
        public async Task<int> DeleteCourseByCodeAsync(string code)
        {
            if (string.IsNullOrWhiteSpace(code)) return 0;
            await using var db = _factory.CreateDbContext();

            var n = Norm(code);
            var course = await db.Courses.FirstOrDefaultAsync(x => x.Code != null && x.Code.ToLower() == n);
            if (course == null) return 0;

            db.Courses.Remove(course);
            return await db.SaveChangesAsync();
        }

        public async Task<int> DeleteAllCoursesAsync()
        {
            await using var db = _factory.CreateDbContext();
            var all = await db.Courses.ToListAsync();
            db.Courses.RemoveRange(all);
            return await db.SaveChangesAsync();
        }

        // ----------------- Department çözümleyici -----------------
        private async Task<int> ResolveDepartmentIdOrThrow()
        {
            if (_current?.DepartmentId is int depId && depId > 0)
                return depId; // Koordinatör

            await using var db = _factory.CreateDbContext();
            var fallback = await db.Departments.FirstOrDefaultAsync();
            if (fallback != null) return fallback.DepartmentId;

            throw new InvalidOperationException(
                "DepartmentId belirlenemedi. Koordinatör olarak giriş yapın ya da geçerli bir bölüm seçimi sağlayın.");
        }
    }
}
