#nullable disable

using SchoolApp.DAL.Models.Base;

namespace SchoolApp.DAL.Models;

public partial class Course : AuditEntity
{
    public int CourseId { get; set; }

    public string Title { get; set; }

    public int Credits { get; set; }

    public int DepartmentId { get; set; }

    
}