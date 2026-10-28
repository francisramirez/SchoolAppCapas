#nullable disable

using SchoolApp.DAL.Models.Base;

namespace SchoolApp.DAL.Models;

public partial class OnsiteCourse : AuditEntity
{
    public int CourseId { get; set; }

    public string Location { get; set; }

    public string Days { get; set; }

    public DateTime Time { get; set; }
}
