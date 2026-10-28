#nullable disable

using SchoolApp.DAL.Models.Base;

namespace SchoolApp.DAL.Models;

public partial class OnlineCourse : AuditEntity
{
    public int CourseId { get; set; }

    public string Url { get; set; }
}
