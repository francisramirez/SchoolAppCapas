namespace SchoolApp.DAL.Models.Objects.Student;

public abstract class StudentBase
{
    public string? StudentNumber { get; set; }
    public string? LastName { get; set; }
    public string? FirstName { get; set; }
    public DateTime EnrollmentDate { get; set; }
}
