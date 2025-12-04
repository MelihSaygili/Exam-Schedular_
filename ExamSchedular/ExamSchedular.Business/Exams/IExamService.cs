using System.Collections.Generic;
using System.Threading.Tasks;
using ExamSchedular.Data.Entities;

namespace ExamSchedular.Business
{
    public interface IExamService
    {
        Task<List<Exam>> GetAsync();
        Task<Exam?> GetByIdAsync(int id);
        Task<Exam> AddOrUpdateAsync(Exam e);
        Task<bool> DeleteAsync(int id);
    }
}
