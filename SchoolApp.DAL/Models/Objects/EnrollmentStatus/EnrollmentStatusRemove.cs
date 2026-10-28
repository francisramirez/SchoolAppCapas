namespace SchoolApp.DAL.Models.Objects.EnrollmentStatus;

public sealed class EnrollmentStatusRemove
{
    public int EnrollmentStatusId { get; set; }
    public bool Deleted { get; set; }
    public DateTime DeletedDate { get; set; }
    public int? UserDeleted { get; set; }
}
