namespace SchoolApp.DAL.Models.Objects.Course;

public sealed class CourseAdd : CourseBase
{
    public DateTime CreationDate { get; set; }
    public int CreationUser { get; set; }
}
