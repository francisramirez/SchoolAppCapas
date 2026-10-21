namespace SchoolApp.DAL.Models.Objects.Instructor;

public sealed class InstructorUpdate : InstructorBase
{
    public int InstructorId { get; set; }
    public DateTime ModifyDate { get; set; }
    public int? ModifyUser { get; set; }
}
