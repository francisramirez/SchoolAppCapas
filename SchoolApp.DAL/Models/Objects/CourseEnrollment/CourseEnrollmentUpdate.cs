namespace SchoolApp.DAL.Models.Objects.CourseEnrollment;

public sealed class CourseEnrollmentUpdate : CourseEnrollmentBase
{
    public int EnrollmentId { get; set; }
    public DateTime ModifyDate { get; set; }
    public int? ModifyUser { get; set; }
}
