namespace SchoolApp.DAL.Models.Objects.Department;

public sealed class DepartmentUpdate : DepartmentBase
{
    public int DepartmentId { get; set; }
    public DateTime ModifyDate { get; set; }
    public int? ModifyUser { get; set; }
}
