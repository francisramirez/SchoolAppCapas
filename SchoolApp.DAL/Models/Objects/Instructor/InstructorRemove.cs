namespace SchoolApp.DAL.Models.Objects.Instructor;

public sealed class InstructorRemove
{
    public int InstructorId { get; set; }
    public bool Deleted { get; set; }
    public DateTime DeletedDate { get; set; }
    public int? UserDeleted { get; set; }
}
