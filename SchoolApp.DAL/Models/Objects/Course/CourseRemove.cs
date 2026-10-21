namespace SchoolApp.DAL.Models.Objects.Course;

public sealed class CourseRemove
{
    public int CourseId { get; set; }
    public bool Deleted { get; set; }
    public DateTime DeletedDate { get; set; }
    public int? UserDeleted { get; set; }
}
