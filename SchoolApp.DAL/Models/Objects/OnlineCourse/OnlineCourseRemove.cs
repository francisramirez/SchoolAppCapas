namespace SchoolApp.DAL.Models.Objects.OnlineCourse;

public sealed class OnlineCourseRemove
{
    public int CourseId { get; set; }
    public bool Deleted { get; set; }
    public DateTime DeletedDate { get; set; }
    public int? UserDeleted { get; set; }
}
