namespace SchoolApp.DAL.Models.Objects.EnrollmentStatus;

public class EnrollmentStatusModel
{
    public int EnrollmentStatusId { get; set; }
    public string? Code { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
    public bool IsActive { get; set; }
}
