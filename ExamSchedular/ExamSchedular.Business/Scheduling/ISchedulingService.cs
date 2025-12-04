using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ExamSchedular.Business.DTOs;

namespace ExamSchedular.Business
{
    public interface ISchedulingService
    {
        Task<List<CoursePickDto>> GetSchedulableCoursesAsync(int departmentId);

        // Önizleme (DB yazmaz)
        Task<ScheduleResult> GenerateAsync(ScheduleRequest req);

        // Kalıcı yazım
        Task<ScheduleResult> BuildAsync(ScheduleRequest req);

        // Okuma & dışa aktarım
        Task<List<PlannedExamItemDto>> GetPlanAsync(int departmentId, DateOnly start, DateOnly end);
        Task<byte[]> ExportExcelAsync(int departmentId, DateOnly start, DateOnly end);

        Task<int> ResolveDefaultDepartmentIdAsync();
    }
}
