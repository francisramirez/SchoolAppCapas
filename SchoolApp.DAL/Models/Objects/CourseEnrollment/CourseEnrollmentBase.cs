namespace SchoolApp.DAL.Models.Objects.CourseEnrollment;

public abstract class CourseEnrollmentBase
{
    public int CourseId { get; set; }
    public int StudentId { get; set; }
    public DateTime EnrollmentDate { get; set; }
    public int EnrollmentStatusId { get; set; }
}
