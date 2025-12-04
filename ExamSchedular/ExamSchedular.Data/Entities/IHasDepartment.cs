namespace ExamSchedular.Data.Entities
{
    public interface IHasDepartment
    {
        int DepartmentId { get; set; }
        Department Department { get; set; }
    }
}
