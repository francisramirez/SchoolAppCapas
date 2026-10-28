namespace SchoolApp.DAL.Models.Objects.EnrollmentStatus;

public sealed class EnrollmentStatusAdd : EnrollmentStatusBase
{
    public DateTime CreationDate { get; set; }
    public int CreationUser { get; set; }
}
