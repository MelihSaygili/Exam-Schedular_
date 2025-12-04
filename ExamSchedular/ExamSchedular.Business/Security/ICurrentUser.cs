namespace ExamSchedular.Business
{
    public interface ICurrentUser
    {
        int? UserId { get; }
        string? Email { get; }
        bool IsAdmin { get; }
        int? DepartmentId { get; }

        void Set(int userId, string email, bool isAdmin, int? departmentId);
        void Clear();
    }
}
