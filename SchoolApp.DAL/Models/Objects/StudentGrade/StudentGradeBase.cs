namespace SchoolApp.DAL.Models.Objects.StudentGrade;

public abstract class StudentGradeBase
{
    public int EnrollmentId { get; set; }
    public int CourseId { get; set; }
    public int StudentId { get; set; }
    public decimal? Grade { get; set; }
}
