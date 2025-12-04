namespace ExamSchedular.Business
{
    public sealed class AuthorizationService : IAuthorizationService
    {
        private readonly ICurrentUser _current;

        public AuthorizationService(ICurrentUser current)
        {
            _current = current;
        }

        public bool IsAdmin() => _current.IsAdmin;

        public bool CanManageDepartmentData() =>
            _current.IsAdmin || _current.DepartmentId.HasValue;
    }
}
