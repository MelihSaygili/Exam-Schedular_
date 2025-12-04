// Business/ScopeExtensions.cs
using System;
using System.Linq;
using ExamSchedular.Data.Entities;

namespace ExamSchedular.Business
{
    public static class ScopeExtensions
    {
        // Admin: tüm kayıtlar, Koordinatör: DepartmentId ile filtre
        public static IQueryable<T> WhereByScope<T>(this IQueryable<T> q, ICurrentUser current)
            where T : class, IHasDepartment
        {
            if (current?.IsAdmin == true) return q;
            if (current?.DepartmentId is int dep && dep > 0)
                return q.Where(x => x.DepartmentId == dep);

            // emniyet: user yoksa hiçbir şey göstermeyelim (istersen q return edebilirsin)
            return q.Where(_ => false);
        }

        public static void EnsureCanAccess<T>(ICurrentUser current, T entity) where T : class, IHasDepartment
        {
            if (current?.IsAdmin == true) return;
            if (current?.DepartmentId is int dep && dep > 0 && entity.DepartmentId == dep) return;
            throw new InvalidOperationException("Bu kaynağa erişim yetkiniz yok.");
        }
    }
}
