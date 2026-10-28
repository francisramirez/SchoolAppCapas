#nullable disable

using SchoolApp.DAL.Models.Base;

namespace SchoolApp.DAL.Models;

public partial class EnrollmentStatus : AuditEntity
{
    public int EnrollmentStatusId { get; set; }

    public string Code { get; set; }

    public string Name { get; set; }

    public string Description { get; set; }

    public bool IsActive { get; set; }

}
