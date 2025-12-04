namespace ExamSchedular.Business
{
    public class CurrentUser : ICurrentUser
    {
        public int? UserId { get; private set; }
        public string? Email { get; private set; }
        public bool IsAdmin { get; private set; }
        public int? DepartmentId { get; private set; }

        public void Set(int userId, string email, bool isAdmin, int? departmentId)
        {
            UserId = userId;
            Email = email;
            IsAdmin = isAdmin;
            DepartmentId = departmentId;
        }

        public void Clear()
        {
            UserId = null;
            Email = null;
            IsAdmin = false;
            DepartmentId = null;
        }
    }
}
