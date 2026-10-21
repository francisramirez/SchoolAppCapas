namespace SchoolApp.DAL.Models.Objects.Course;

public abstract class CourseBase
{
    public string? Title { get; set; }
    public int Credits { get; set; }
    public int DepartmentId { get; set; }
}
