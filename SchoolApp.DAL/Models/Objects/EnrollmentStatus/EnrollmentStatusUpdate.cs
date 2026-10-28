namespace SchoolApp.DAL.Models.Objects.EnrollmentStatus;

public sealed class EnrollmentStatusUpdate : EnrollmentStatusBase
{
    public int EnrollmentStatusId { get; set; }
    public DateTime ModifyDate { get; set; }
    public int? ModifyUser { get; set; }
}
