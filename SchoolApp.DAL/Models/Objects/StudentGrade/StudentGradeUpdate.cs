namespace SchoolApp.DAL.Models.Objects.StudentGrade;

public sealed class StudentGradeUpdate : StudentGradeBase
{
    public DateTime ModifyDate { get; set; }
    public int? ModifyUser { get; set; }
}
