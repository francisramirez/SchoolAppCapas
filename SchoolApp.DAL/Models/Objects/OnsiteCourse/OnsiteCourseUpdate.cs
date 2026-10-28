namespace SchoolApp.DAL.Models.Objects.OnsiteCourse;

public sealed class OnsiteCourseUpdate : OnsiteCourseBase
{
    public DateTime ModifyDate { get; set; }
    public int? ModifyUser { get; set; }
}
