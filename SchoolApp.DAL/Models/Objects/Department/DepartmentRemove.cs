namespace SchoolApp.DAL.Models.Objects.Department;

public sealed class DepartmentRemove
{
    public int DepartmentId { get; set; }
    public bool Deleted { get; set; }
    public DateTime DeletedDate { get; set; }
    public int? UserDeleted { get; set; }
}
