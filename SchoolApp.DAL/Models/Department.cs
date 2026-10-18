#nullable disable

using SchoolApp.DAL.Models.Base;

namespace SchoolApp.DAL.Models;

public partial class Department : AuditEntity
{
    public int DepartmentId { get; set; }

    public string Name { get; set; }

    public decimal Budget { get; set; }

    public DateTime StartDate { get; set; }

    public int? Administrator { get; set; }

    
}