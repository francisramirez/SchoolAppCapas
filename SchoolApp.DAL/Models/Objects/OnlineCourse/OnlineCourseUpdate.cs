namespace SchoolApp.DAL.Models.Objects.OnlineCourse;

public sealed class OnlineCourseUpdate : OnlineCourseBase
{
    public DateTime ModifyDate { get; set; }
    public int? ModifyUser { get; set; }
}
