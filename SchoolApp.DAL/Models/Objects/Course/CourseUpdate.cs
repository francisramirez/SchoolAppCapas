namespace SchoolApp.DAL.Models.Objects.Course;

public sealed class CourseUpdate : CourseBase
{
    public int CourseId { get; set; }
    public DateTime ModifyDate { get; set; }
    public int? ModifyUser { get; set; }
}
