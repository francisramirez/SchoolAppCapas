namespace SchoolApp.DAL.Models.Objects.CourseEnrollment;

public class CourseEnrollmentModel
{
    public int EnrollmentId { get; set; }
    public int CourseId { get; set; }
    public int StudentId { get; set; }
    public DateTime EnrollmentDate { get; set; }
    public int EnrollmentStatusId { get; set; }
}
