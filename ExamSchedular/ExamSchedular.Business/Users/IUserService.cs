using System.Collections.Generic;
using System.Threading.Tasks;
using ExamSchedular.Data.Entities;

namespace ExamSchedular.Business
{
    public interface IUserService
    {
        Task<List<AppUser>> GetAsync();
        Task<AppUser> CreateCoordinatorAsync(string email, string password, int departmentId);
        Task<bool> DeleteAsync(int userId);
        Task<bool> ResetPasswordAsync(int userId, string newPassword);
    }
}
