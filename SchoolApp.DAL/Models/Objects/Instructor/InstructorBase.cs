namespace SchoolApp.DAL.Models.Objects.Instructor;

public abstract class InstructorBase
{
    public string? LastName { get; set; }
    public string? FirstName { get; set; }
    public DateTime? HireDate { get; set; }
}
