namespace SchoolApp.DAL.Models.Objects.CourseEnrollment;

public sealed class CourseEnrollmentAdd : CourseEnrollmentBase
{
    public DateTime CreationDate { get; set; }
    public int CreationUser { get; set; }
}
