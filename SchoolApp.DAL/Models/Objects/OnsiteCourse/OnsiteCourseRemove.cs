namespace SchoolApp.DAL.Models.Objects.OnsiteCourse;

public sealed class OnsiteCourseRemove
{
    public int CourseId { get; set; }
    public bool Deleted { get; set; }
    public DateTime DeletedDate { get; set; }
    public int? UserDeleted { get; set; }
}
