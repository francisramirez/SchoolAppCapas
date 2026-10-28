namespace SchoolApp.DAL.Models.Objects.StudentGrade;

public sealed class StudentGradeRemove
{
    public int EnrollmentId { get; set; }
    public bool Deleted { get; set; }
    public DateTime DeletedDate { get; set; }
    public int? UserDeleted { get; set; }
}
