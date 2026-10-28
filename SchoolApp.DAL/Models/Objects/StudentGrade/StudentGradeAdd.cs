namespace SchoolApp.DAL.Models.Objects.StudentGrade;

public sealed class StudentGradeAdd : StudentGradeBase
{
    public DateTime CreationDate { get; set; }
    public int CreationUser { get; set; }
}
