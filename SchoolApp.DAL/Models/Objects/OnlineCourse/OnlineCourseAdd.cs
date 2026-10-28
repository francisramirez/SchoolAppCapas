namespace SchoolApp.DAL.Models.Objects.OnlineCourse;

public sealed class OnlineCourseAdd : OnlineCourseBase
{
    public DateTime CreationDate { get; set; }
    public int CreationUser { get; set; }
}
