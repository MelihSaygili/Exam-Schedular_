using System;
using System.Collections.Generic;

namespace ExamSchedular.Business.DTOs
{
    // Sol panelde/listelemelerde kullanılabilir
    public sealed class CoursePickDto
    {
        public int CourseId { get; set; }
        public string Code { get; set; } = "";
        public string Name { get; set; } = "";
        public int StudentCount { get; set; }
        public int DominantClassYear { get; set; }
    }

    // Önizleme satırı
    public sealed class PreviewAssignmentDto
    {
        public int CourseId { get; set; }          // temsilci CourseId (aynı koda sahip olanlar birleşik)
        public string CourseCode { get; set; } = "";
        public string CourseName { get; set; } = "";
        public DateOnly Date { get; set; }
        public TimeOnly StartTime { get; set; }
        public int StudentCount { get; set; }
        public string? RoomsCsv { get; set; }
        public int SuggestedDurationMinutes { get; set; } = 90;
    }

    // Kaydedilmiş plan satırı
    public sealed class PlannedExamItemDto
    {
        public DateOnly Date { get; set; }
        public TimeOnly StartTime { get; set; }
        public string CourseCode { get; set; } = "";
        public string CourseName { get; set; } = "";
        public int StudentCount { get; set; }
        public string RoomsCsv { get; set; } = "";
        public string RoomsDetailCsv { get; set; } = "";
        public int TotalCapacity { get; set; }
        public int TotalCapacityUsed { get; set; }
    }

    // İstek
    public sealed class ScheduleRequest
    {
        public int DepartmentId { get; set; }
        public DateOnly StartDate { get; set; }
        public DateOnly EndDate { get; set; }
        public TimeOnly DayStart { get; set; }
        public TimeOnly DayEnd { get; set; }
        public int SlotStepMinutes { get; set; } = 90;
        public bool EnforceNoOverlapForSameStudent { get; set; } = true;
        public bool SpreadByClassYear { get; set; } = true;

        public HashSet<DayOfWeek> ExcludedDays { get; } = new();

        // UI’dan seçimler
        public List<int>? IncludedCourseIds { get; set; }
        public List<int>? ExcludedCourseIds { get; set; }
        public Dictionary<int, int>? DurationOverrideMinutes { get; set; }  // CourseId -> dakika
    }

    // Sonuç
    public sealed class ScheduleResult
    {
        public List<string> Errors { get; set; } = new();
        public List<string> Warnings { get; set; } = new();
        public List<int> CreatedExamIds { get; set; } = new();
        public bool Success => Errors.Count == 0;
        public List<PreviewAssignmentDto> Preview { get; set; } = new();
    }
}
