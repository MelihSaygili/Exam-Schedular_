using System.Collections.Generic;
using System.Threading.Tasks;
using ExamSchedular.Data.Entities;

namespace ExamSchedular.Business
{
    public interface ITimeslotService
    {
        Task<List<Timeslot>> GetAsync();
        Task<Timeslot?> GetByIdAsync(int id);
        Task<Timeslot> AddOrUpdateAsync(Timeslot t);
        Task<bool> DeleteAsync(int id);
    }
}
