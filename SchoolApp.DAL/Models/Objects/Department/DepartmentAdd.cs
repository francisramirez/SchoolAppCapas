namespace SchoolApp.DAL.Models.Objects.Department;

public sealed class DepartmentAdd : DepartmentBase
{
    public DateTime CreationDate { get; set; }
    public int CreationUser { get; set; }
}
