#nullable disable

using SchoolApp.DAL.Models.Base;

namespace SchoolApp.DAL.Models;

public partial class CourseEnrollment : AuditEntity
{
    public int EnrollmentId { get; set; }

    public int CourseId { get; set; }

    public int StudentId { get; set; }

    public DateTime EnrollmentDate { get; set; }

    public int EnrollmentStatusId { get; set; }

    
}