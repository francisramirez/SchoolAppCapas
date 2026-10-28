namespace SchoolApp.DAL.Models.Objects.OnsiteCourse;

public sealed class OnsiteCourseAdd : OnsiteCourseBase
{
    public DateTime CreationDate { get; set; }
    public int CreationUser { get; set; }
}
