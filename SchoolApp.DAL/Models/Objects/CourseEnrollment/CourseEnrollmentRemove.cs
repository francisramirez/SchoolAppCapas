namespace SchoolApp.DAL.Models.Objects.CourseEnrollment;

public sealed class CourseEnrollmentRemove
{
    public int EnrollmentId { get; set; }
    public bool Deleted { get; set; }
    public DateTime DeletedDate { get; set; }
    public int? UserDeleted { get; set; }
}
