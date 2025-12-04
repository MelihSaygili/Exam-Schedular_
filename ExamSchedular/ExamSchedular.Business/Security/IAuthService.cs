using ExamSchedular.Data.Entities;
using System.Threading.Tasks;

public interface IAuthService
{
    Task<AppUser?> LoginAsync(string email, string password);
}
