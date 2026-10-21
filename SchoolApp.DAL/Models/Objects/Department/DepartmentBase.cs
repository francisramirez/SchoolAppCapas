namespace SchoolApp.DAL.Models.Objects.Department;

public abstract class DepartmentBase
{
    public string? Name { get; set; }
    public decimal Budget { get; set; }
    public DateTime StartDate { get; set; }
    public int? Administrator { get; set; }
}
