

namespace SchoolApp.DAL.Models.Objects.Student
{
    public sealed class StudentUpdate : StudentBase
    {
        public int StudentId { get; set; }
        public DateTime ModifyDate { get; set; }
        public int? ModifyUser { get; set; }
    }
}
