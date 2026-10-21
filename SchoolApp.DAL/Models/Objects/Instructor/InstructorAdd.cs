namespace SchoolApp.DAL.Models.Objects.Instructor;

public sealed class InstructorAdd : InstructorBase
{
    public DateTime CreationDate { get; set; }
    public int CreationUser { get; set; }
}
