using System.Collections.Generic;
using System.Threading.Tasks;
using ExamSchedular.Data.Entities;


namespace ExamSchedular.Business
{
    public interface IClassroomService
    {
        Task<List<Classroom>> GetAsync();
        Task<Classroom?> GetByIdAsync(int id);
        Task<Classroom> AddOrUpdateAsync(Classroom c);
        Task<bool> DeleteAsync(int id);
    }
}
