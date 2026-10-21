namespace SchoolApp.DAL.Models.Objects.Student
{
    public sealed class StudentRemove
    {
        public int StudentId { get; set; }
        public bool Deleted { get; set; }
        public DateTime DeletedDate { get; set; }
        public int? UserDeleted { get; set; }
    }
}
