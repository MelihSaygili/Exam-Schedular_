using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ClosedXML.Excel;
using ExamSchedular.Business.DTOs;
using ExamSchedular.Data;
using ExamSchedular.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace ExamSchedular.Business
{
    public class SchedulingService : ISchedulingService
    {
        private readonly IDbContextFactory<AppDbContext> _factory;
        private readonly ICurrentUser _current;

        public SchedulingService(IDbContextFactory<AppDbContext> factory, ICurrentUser current)
        {
            _factory = factory;
            _current = current;
        }

        private sealed record CourseInfo(int Id, string Code, string Name, int StudentCount, int DominantClassYear);

        private sealed class LoadedData
        {
            public List<CourseInfo> Courses { get; init; } = new();
            public Dictionary<int, HashSet<int>> StudentsByCourse { get; init; } = new();
        }

        // -------------------------------------------------------
        // Ders listesi (gerekirse UI sol panel için)
        public async Task<List<CoursePickDto>> GetSchedulableCoursesAsync(int departmentId)
        {
            await using var db = _factory.CreateDbContext();
            if (!_current.IsAdmin && _current.DepartmentId != departmentId)
                return new List<CoursePickDto>();

            var list = await db.Courses
                               .AsNoTracking()
                               .Include(c => c.Enrollments).ThenInclude(e => e.Student)
                               .Where(c => c.DepartmentId == departmentId)
                               .ToListAsync();

            // AYNI KODA SAHİP DERSLERİ TEK SATIRDA BİRLEŞTİR
            var result = new List<CoursePickDto>();
            foreach (var g in list.GroupBy(c => c.Code.Trim().ToUpperInvariant()))
            {
                var first = g.OrderBy(x => x.CourseId).First();
                var studentCount = g.SelectMany(x => x.Enrollments)
                                    .Select(e => e.StudentId)
                                    .Distinct().Count();

                var dom = g.SelectMany(x => x.Enrollments)
                           .GroupBy(e => e.Student.ClassYear)
                           .OrderByDescending(x => x.Count())
                           .Select(x => x.Key)
                           .FirstOrDefault();

                result.Add(new CoursePickDto
                {
                    CourseId = first.CourseId, // temsilci
                    Code = first.Code,
                    Name = first.Name,
                    StudentCount = studentCount,
                    DominantClassYear = dom
                });
            }
            return result.OrderBy(x => x.Code).ToList();
        }

        // -------------------------------------------------------
        // Önizleme (DB yazmaz)
        public async Task<ScheduleResult> GenerateAsync(ScheduleRequest req)
        {
            var res = new ScheduleResult();
            if (!AuthorizeDepartment(req.DepartmentId, res)) return res;

            var data = await LoadCoursesForRequest(req, res);
            if (res.Errors.Any()) return res;

            var conflicts = BuildConflictGraph(data.StudentsByCourse);
            var slots = GenerateTimeslots(req);
            if (slots.Count == 0)
            {
                res.Errors.Add("Seçili tarih aralığında uygun zaman dilimi yok.");
                return res;
            }

            // ZOR DERSLER ÖNCE
            var placed = new Dictionary<int, (DateOnly d, TimeOnly t)>();
            foreach (var c in OrderByDifficulty(data.Courses, conflicts))
            {
                var ok = FindFeasibleSlot(c, slots, placed, conflicts, req, data.Courses);
                if (ok == null)
                {
                    res.Errors.Add($"Uygun slot bulunamadı: {c.Code} - {c.Name}");
                    continue;
                }
                placed[c.Id] = ok.Value;
            }
            if (res.Errors.Any()) return res;

            // Derslikleri getir (hepsi)
            await using var db = _factory.CreateDbContext();
            var rooms = await db.Classrooms.AsNoTracking()
                                           .Where(r => r.DepartmentId == req.DepartmentId)
                                           .OrderByDescending(r => r.Capacity)
                                           .ThenBy(r => r.Code)
                                           .ToListAsync();

            // Aynı slotta odaların tekrar kullanılmasını engelle
            var usedRoomsBySlot = new Dictionary<(DateOnly d, TimeOnly t), HashSet<int>>();
            // *** GLOBAL RR INDEX ***
            var rrIndexGlobal = 0;

            foreach (var c in data.Courses)
            {
                var (d, t) = placed[c.Id];

                if (!usedRoomsBySlot.TryGetValue((d, t), out var used))
                    usedRoomsBySlot[(d, t)] = used = new HashSet<int>();

                var rr = rrIndexGlobal;
                var chosen = SelectRoomsBestFitBalanced(rooms, c.StudentCount, used, ref rr);
                rrIndexGlobal = rr;

                string? roomsCsv = null;

                if (chosen is null)
                {
                    res.Errors.Add($"Derslik kapasitesi yetersiz (dry-run): {c.Code}");
                }
                else
                {
                    foreach (var r in chosen) used.Add(r.ClassroomId);
                    roomsCsv = string.Join(", ", chosen.Select(r => r.Code));
                }

                var dur = 90;
                if (req.DurationOverrideMinutes != null &&
                    req.DurationOverrideMinutes.TryGetValue(c.Id, out var dmin) &&
                    dmin > 0) dur = dmin;

                res.Preview.Add(new PreviewAssignmentDto
                {
                    CourseId = c.Id,
                    CourseCode = c.Code,
                    CourseName = c.Name,
                    Date = d,
                    StartTime = t,
                    StudentCount = c.StudentCount,
                    RoomsCsv = roomsCsv,
                    SuggestedDurationMinutes = dur
                });
            }
            return res;
        }

        // -------------------------------------------------------
        // Kalıcı yazım
        public async Task<ScheduleResult> BuildAsync(ScheduleRequest req)
        {
            var res = new ScheduleResult();
            if (!AuthorizeDepartment(req.DepartmentId, res)) return res;

            var data = await LoadCoursesForRequest(req, res);
            if (res.Errors.Any()) return res;

            var conflicts = BuildConflictGraph(data.StudentsByCourse);
            var slots = GenerateTimeslots(req);
            if (slots.Count == 0)
            {
                res.Errors.Add("Seçili tarih aralığında uygun zaman dilimi yok.");
                return res;
            }

            // *** TEMİZLEME: Bu departman + tarih aralığındaki eski programı kaldır ***
            await using (var cleanDb = _factory.CreateDbContext())
            await using (var cleanTx = await cleanDb.Database.BeginTransactionAsync())
            {
                var slotIds = await cleanDb.Timeslots
                    .Where(ts => ts.DepartmentId == req.DepartmentId
                              && ts.Date >= req.StartDate
                              && ts.Date <= req.EndDate)
                    .Select(ts => ts.TimeslotId)
                    .ToListAsync();

                if (slotIds.Count > 0)
                {
                    var oldExamIds = await cleanDb.Exams
                        .Where(e => slotIds.Contains(e.TimeslotId))
                        .Select(e => e.ExamId)
                        .ToListAsync();

                    if (oldExamIds.Count > 0)
                    {
                        var oldAssigns = await cleanDb.ExamAssignments
                            .Where(a => oldExamIds.Contains(a.ExamId))
                            .ToListAsync();
                        if (oldAssigns.Count > 0)
                            cleanDb.ExamAssignments.RemoveRange(oldAssigns);

                        var oldExams = await cleanDb.Exams
                            .Where(e => oldExamIds.Contains(e.ExamId))
                            .ToListAsync();
                        if (oldExams.Count > 0)
                            cleanDb.Exams.RemoveRange(oldExams);

                        await cleanDb.SaveChangesAsync();
                    }

                    // Boş kalan timeslot’ları (yalnızca bu aralık için) sil
                    var stillUsedSlotIds = await cleanDb.Exams
                        .Where(e => slotIds.Contains(e.TimeslotId))
                        .Select(e => e.TimeslotId)
                        .Distinct()
                        .ToListAsync();

                    var orphanSlotIds = slotIds.Except(stillUsedSlotIds).ToList();
                    if (orphanSlotIds.Count > 0)
                    {
                        var orphanSlots = await cleanDb.Timeslots
                            .Where(ts => orphanSlotIds.Contains(ts.TimeslotId))
                            .ToListAsync();

                        cleanDb.Timeslots.RemoveRange(orphanSlots);
                        await cleanDb.SaveChangesAsync();
                    }
                }

                await cleanTx.CommitAsync();
            }
            // *** TEMİZLİK BİTTİ ***

            // Yerleştirme
            var placed = new Dictionary<int, (DateOnly d, TimeOnly t)>();
            foreach (var c in OrderByDifficulty(data.Courses, conflicts))
            {
                if (req.IncludedCourseIds?.Any() == true && !req.IncludedCourseIds.Contains(c.Id))
                    continue;
                if (req.ExcludedCourseIds?.Any() == true && req.ExcludedCourseIds.Contains(c.Id))
                    continue;

                var ok = FindFeasibleSlot(c, slots, placed, conflicts, req, data.Courses);
                if (ok == null)
                {
                    res.Errors.Add($"Slot bulunamadı: {c.Code} - {c.Name}");
                    continue;
                }
                placed[c.Id] = ok.Value;
            }
            if (res.Errors.Any()) return res;

            await using var db = _factory.CreateDbContext();
            var rooms = await db.Classrooms
                                .Where(r => r.DepartmentId == req.DepartmentId)
                                .OrderByDescending(r => r.Capacity)
                                .ThenBy(r => r.Code)
                                .ToListAsync();

            var assigns = new List<(CourseInfo c, DateOnly d, TimeOnly t, List<Classroom> rooms)>();
            var usedRoomsBySlot = new Dictionary<(DateOnly d, TimeOnly t), HashSet<int>>();
            // *** GLOBAL RR INDEX ***
            var rrIndexGlobal = 0;

            foreach (var c in data.Courses)
            {
                if (!placed.TryGetValue(c.Id, out var slot)) continue;

                if (!usedRoomsBySlot.TryGetValue(slot, out var used))
                    usedRoomsBySlot[slot] = used = new HashSet<int>();

                var rr = rrIndexGlobal;
                var selected = SelectRoomsBestFitBalanced(rooms, c.StudentCount, used, ref rr);
                rrIndexGlobal = rr;

                if (selected is null || selected.Count == 0)
                {
                    res.Errors.Add($"Derslik kapasitesi yetersiz: {c.Code}");
                    continue;
                }

                if (DistributeCapacity(selected, c.StudentCount) is null)
                {
                    res.Errors.Add($"Dersliklere kapasite dağıtılamadı: {c.Code}");
                    continue;
                }

                assigns.Add((c, slot.d, slot.t, selected));
                foreach (var r in selected) used.Add(r.ClassroomId);
            }
            if (res.Errors.Any()) return res;

            await using var tx = await db.Database.BeginTransactionAsync();
            try
            {
                // Timeslot
                var slotKeys = assigns.Select(a => (a.d, a.t)).Distinct().ToList();
                var needDates = slotKeys.Select(k => k.d).Distinct().ToList();

                var existing = await db.Timeslots
                                       .Where(ts => ts.DepartmentId == req.DepartmentId
                                                 && needDates.Contains(ts.Date))
                                       .ToListAsync();

                var slotIdMap = new Dictionary<(DateOnly, TimeOnly), int>();
                foreach (var ts in existing)
                    slotIdMap[(ts.Date, ts.StartTime)] = ts.TimeslotId;

                var toAdd = new List<Timeslot>();
                foreach (var key in slotKeys)
                    if (!slotIdMap.ContainsKey(key))
                        toAdd.Add(new Timeslot { DepartmentId = req.DepartmentId, Date = key.Item1, StartTime = key.Item2 });

                if (toAdd.Count > 0)
                {
                    db.Timeslots.AddRange(toAdd);
                    await db.SaveChangesAsync();
                    foreach (var ts in toAdd)
                        slotIdMap[(ts.Date, ts.StartTime)] = ts.TimeslotId;
                }

                // Exams
                var examsToAdd = new List<Exam>();
                var courseExamIndex = new Dictionary<int, Exam>();
                foreach (var a in assigns)
                {
                    var dur = 90;
                    if (req.DurationOverrideMinutes != null &&
                        req.DurationOverrideMinutes.TryGetValue(a.c.Id, out var dmin) &&
                        dmin > 0) dur = dmin;

                    var exam = new Exam
                    {
                        CourseId = a.c.Id,
                        TimeslotId = slotIdMap[(a.d, a.t)],
                        ExamType = "Final",
                        DefaultDurationMinutes = dur,
                        DepartmentId = req.DepartmentId
                    };
                    examsToAdd.Add(exam);
                    courseExamIndex[a.c.Id] = exam;
                }
                db.Exams.AddRange(examsToAdd);
                await db.SaveChangesAsync();

                // ExamAssignments
                var assignsToAdd = new List<ExamAssignment>();
                foreach (var a in assigns)
                {
                    var exam = courseExamIndex[a.c.Id];
                    var dist = DistributeCapacity(a.rooms, a.c.StudentCount)!;
                    var tsId = slotIdMap[(a.d, a.t)];
                    foreach (var (room, used) in dist)
                    {
                        assignsToAdd.Add(new ExamAssignment
                        {
                            ExamId = exam.ExamId,
                            TimeslotId = tsId,
                            ClassroomId = room.ClassroomId,
                            CapacityUsed = used
                        });
                    }
                }
                db.ExamAssignments.AddRange(assignsToAdd);
                await db.SaveChangesAsync();

                res.CreatedExamIds.AddRange(examsToAdd.Select(e => e.ExamId));
                await tx.CommitAsync();
            }
            catch (Exception ex)
            {
                await tx.RollbackAsync();
                res.Errors.Add("Program kaydedilemedi: " + (ex.InnerException?.GetBaseException().Message ?? ex.GetBaseException().Message));
            }

            return res;
        }

        // -------------------------------------------------------
        // Plan okuma + export
        public async Task<List<PlannedExamItemDto>> GetPlanAsync(int departmentId, DateOnly start, DateOnly end)
        {
            await using var db = _factory.CreateDbContext();
            if (!_current.IsAdmin && _current.DepartmentId != departmentId)
                return new List<PlannedExamItemDto>();

            var exams = await db.Exams.AsNoTracking()
                .Where(e => e.Course.DepartmentId == departmentId
                         && e.Timeslot.Date >= start
                         && e.Timeslot.Date <= end)
                .Select(e => new
                {
                    e.ExamId,
                    e.CourseId,
                    CourseCode = e.Course.Code,
                    CourseName = e.Course.Name,
                    e.Timeslot.Date,
                    e.Timeslot.StartTime
                })
                .ToListAsync();

            if (exams.Count == 0) return new List<PlannedExamItemDto>();

            var examIds = exams.Select(x => x.ExamId).ToList();
            var courseIds = exams.Select(x => x.CourseId).Distinct().ToList();

            var assigns = await db.ExamAssignments.AsNoTracking()
                .Where(a => examIds.Contains(a.ExamId))
                .Select(a => new
                {
                    a.ExamId,
                    RoomCode = a.Classroom.Code,
                    RoomCapacity = (int?)a.Classroom.Capacity,
                    CapacityUsed = (int?)a.CapacityUsed
                })
                .ToListAsync();

            var assignMap = assigns.GroupBy(a => a.ExamId).ToDictionary(
                g => g.Key,
                g =>
                {
                    var ordered = g.OrderBy(x => x.RoomCode).ToList();
                    var roomsCsv = string.Join(", ", ordered.Select(x => x.RoomCode));
                    var roomsDetailCsv = string.Join("; ",
                        ordered.Select(x => $"{x.RoomCode} ({(x.CapacityUsed ?? 0)}/{(x.RoomCapacity ?? 0)})"));
                    return new
                    {
                        RoomsCsv = roomsCsv,
                        RoomsDetailCsv = roomsDetailCsv,
                        TotalCapacity = ordered.Sum(x => x.RoomCapacity ?? 0),
                        TotalCapacityUsed = ordered.Sum(x => x.CapacityUsed ?? 0)
                    };
                });

            var counts = await db.Enrollments.AsNoTracking()
                .Where(en => courseIds.Contains(en.CourseId))
                .GroupBy(en => en.CourseId)
                .Select(g => new { CourseId = g.Key, Count = g.Select(x => x.StudentId).Distinct().Count() })
                .ToDictionaryAsync(x => x.CourseId, x => x.Count);

            var rows = new List<PlannedExamItemDto>(exams.Count);
            foreach (var e in exams)
            {
                var hasAssign = assignMap.TryGetValue(e.ExamId, out var ainfo);
                rows.Add(new PlannedExamItemDto
                {
                    Date = e.Date,
                    StartTime = e.StartTime,
                    CourseCode = e.CourseCode,
                    CourseName = e.CourseName,
                    StudentCount = counts.TryGetValue(e.CourseId, out var sc) ? sc : 0,
                    RoomsCsv = hasAssign ? ainfo!.RoomsCsv : "",
                    RoomsDetailCsv = hasAssign ? ainfo!.RoomsDetailCsv : "",
                    TotalCapacity = hasAssign ? ainfo!.TotalCapacity : 0,
                    TotalCapacityUsed = hasAssign ? ainfo!.TotalCapacityUsed : 0
                });
            }

            return rows.OrderBy(x => x.Date).ThenBy(x => x.StartTime).ThenBy(x => x.CourseCode).ToList();
        }

        public async Task<byte[]> ExportExcelAsync(int departmentId, DateOnly start, DateOnly end)
        {
            var rows = await GetPlanAsync(departmentId, start, end);

            using var wb = new XLWorkbook();
            var ws = wb.Worksheets.Add("Program");

            ws.Cell(1, 1).Value = "Tarih";
            ws.Cell(1, 2).Value = "Saat";
            ws.Cell(1, 3).Value = "Ders Kodu";
            ws.Cell(1, 4).Value = "Ders Adı";
            ws.Cell(1, 5).Value = "Öğrenci Sayısı";
            ws.Cell(1, 6).Value = "Derslik(ler)";
            ws.Cell(1, 7).Value = "Derslik Detay";
            ws.Cell(1, 8).Value = "Kapasite (Kull./Top.)";

            ws.Range(1, 1, 1, 8).Style.Font.Bold = true;

            int r = 2;
            foreach (var x in rows)
            {
                ws.Cell(r, 1).Value = x.Date.ToString("yyyy-MM-dd");
                ws.Cell(r, 2).Value = x.StartTime.ToString("HH\\:mm");
                ws.Cell(r, 3).Value = x.CourseCode;
                ws.Cell(r, 4).Value = x.CourseName;
                ws.Cell(r, 5).Value = x.StudentCount;
                ws.Cell(r, 6).Value = x.RoomsCsv;
                ws.Cell(r, 7).Value = x.RoomsDetailCsv;
                ws.Cell(r, 8).Value = $"{x.TotalCapacityUsed}/{x.TotalCapacity}";
                r++;
            }

            ws.Columns().AdjustToContents();
            using var ms = new MemoryStream();
            wb.SaveAs(ms);
            return ms.ToArray();
        }

        // -------------------------------------------------------
        // Helpers

        private bool AuthorizeDepartment(int departmentId, ScheduleResult res)
        {
            if (departmentId <= 0) { res.Errors.Add("DepartmentId geçersiz."); return false; }
            if (!_current.IsAdmin && _current.DepartmentId != departmentId)
            { res.Errors.Add("Bu departman için yetkiniz yok."); return false; }
            return true;
        }

        // Dersleri CODE’a göre birleştirerek yükle (tek satır/tek ders)
        private async Task<LoadedData> LoadCoursesForRequest(ScheduleRequest req, ScheduleResult res)
        {
            await using var db = _factory.CreateDbContext();

            var cq = db.Courses.Include(c => c.Enrollments).ThenInclude(e => e.Student)
                               .Where(c => c.DepartmentId == req.DepartmentId);

            if (req.IncludedCourseIds?.Any() == true)
                cq = cq.Where(c => req.IncludedCourseIds.Contains(c.CourseId));
            if (req.ExcludedCourseIds?.Any() == true)
                cq = cq.Where(c => !req.ExcludedCourseIds.Contains(c.CourseId));

            var courses = await cq.AsNoTracking().ToListAsync();
            if (courses.Count == 0)
            {
                res.Errors.Add("Programa alınacak ders bulunamadı.");
                return new LoadedData();
            }

            var result = new LoadedData();

            foreach (var g in courses.GroupBy(c => c.Code.Trim().ToUpperInvariant()))
            {
                var rep = g.OrderBy(x => x.CourseId).First(); // temsilci
                var stuSet = g.SelectMany(x => x.Enrollments).Select(e => e.StudentId).ToHashSet();

                var dom = g.SelectMany(x => x.Enrollments)
                           .GroupBy(e => e.Student.ClassYear)
                           .OrderByDescending(x => x.Count())
                           .Select(x => x.Key)
                           .FirstOrDefault();

                result.Courses.Add(new CourseInfo(rep.CourseId, rep.Code, rep.Name, stuSet.Count, dom));
                result.StudentsByCourse[rep.CourseId] = stuSet;
            }
            return result;
        }

        private Dictionary<int, HashSet<int>> BuildConflictGraph(Dictionary<int, HashSet<int>> studentsByCourse)
        {
            var ids = studentsByCourse.Keys.ToList();
            var dic = ids.ToDictionary(id => id, _ => new HashSet<int>());
            for (int i = 0; i < ids.Count; i++)
            {
                var ci = ids[i];
                var si = studentsByCourse[ci];
                for (int j = i + 1; j < ids.Count; j++)
                {
                    var cj = ids[j];
                    if (si.Overlaps(studentsByCourse[cj]))
                    {
                        dic[ci].Add(cj);
                        dic[cj].Add(ci);
                    }
                }
            }
            return dic;
        }

        private List<(DateOnly d, TimeOnly t)> GenerateTimeslots(ScheduleRequest req)
        {
            var list = new List<(DateOnly, TimeOnly)>();
            for (var d = req.StartDate; d <= req.EndDate; d = d.AddDays(1))
            {
                if (req.ExcludedDays.Contains(d.DayOfWeek)) continue;
                for (var t = req.DayStart; t < req.DayEnd; t = t.AddMinutes(req.SlotStepMinutes))
                    list.Add((d, t));
            }
            return list;
        }

        private IEnumerable<CourseInfo> OrderByDifficulty(List<CourseInfo> courses, Dictionary<int, HashSet<int>> conflicts)
            => courses.OrderByDescending(c => conflicts[c.Id].Count)
                      .ThenByDescending(c => c.StudentCount);

        private (DateOnly d, TimeOnly t)? FindFeasibleSlot(
            CourseInfo c,
            List<(DateOnly d, TimeOnly t)> slots,
            Dictionary<int, (DateOnly d, TimeOnly t)> placed,
            Dictionary<int, HashSet<int>> conflicts,
            ScheduleRequest req,
            List<CourseInfo> all)
        {
            foreach (var s in slots)
            {
                if (conflicts[c.Id].Any(n => placed.TryGetValue(n, out var ps) && ps.d == s.d && ps.t == s.t))
                    continue;

                if (req.SpreadByClassYear)
                {
                    var sameDayClass = placed.Any(p =>
                    {
                        var cc = all.First(x => x.Id == p.Key);
                        return p.Value.d == s.d && cc.DominantClassYear == c.DominantClassYear;
                    });
                    if (sameDayClass) continue;
                }

                if (req.EnforceNoOverlapForSameStudent)
                {
                    bool violation = placed.Any(p => conflicts[c.Id].Contains(p.Key) &&
                                                     p.Value.d == s.d && p.Value.t == s.t);
                    if (violation) continue;
                }
                return s;
            }
            return null;
        }

        // --- DERSLİK SEÇİMİ: adil kullanım / round-robin başlangıç noktası + kapasiteye göre ekleme
        private static List<Classroom>? SelectRoomsBestFitBalanced(
            List<Classroom> rooms, int need, HashSet<int> excludeIds, ref int roundRobinIndex)
        {
            if (need <= 0) return new List<Classroom>();

            // rooms capacity DESC; burada başlangıcı kaydırıyoruz (tüm odalar zamanla kullanılsın)
            var rotated = new List<Classroom>(rooms.Count);
            for (int i = 0; i < rooms.Count; i++)
            {
                var idx = (roundRobinIndex + i) % rooms.Count;
                rotated.Add(rooms[idx]);
            }
            roundRobinIndex = (roundRobinIndex + 1) % rooms.Count;

            var chosen = new List<Classroom>();
            var remain = need;

            foreach (var r in rotated)
            {
                if (excludeIds.Contains(r.ClassroomId)) continue;
                if (remain <= 0) break;

                chosen.Add(r);
                remain -= r.Capacity;
            }

            return remain <= 0 ? chosen : null;
        }

        private static List<(Classroom room, int used)>? DistributeCapacity(List<Classroom> rooms, int studentCount)
        {
            if (studentCount <= 0) return new List<(Classroom, int)>();
            var result = new List<(Classroom room, int used)>();
            var remaining = studentCount;

            foreach (var r in rooms)
            {
                if (remaining <= 0) break;
                var take = Math.Min(r.Capacity, remaining);
                if (take > 0)
                {
                    result.Add((r, take));
                    remaining -= take;
                }
            }
            return remaining > 0 ? null : result;
        }

        public async Task<int> ResolveDefaultDepartmentIdAsync()
        {
            await using var db = _factory.CreateDbContext();
            return await db.Departments.OrderBy(d => d.DepartmentId)
                                       .Select(d => d.DepartmentId)
                                       .FirstOrDefaultAsync();
        }
    }
}
