namespace SchoolApp.DAL.Models.Objects.EnrollmentStatus;

public abstract class EnrollmentStatusBase
{
    public string? Code { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
    public bool IsActive { get; set; }
}
