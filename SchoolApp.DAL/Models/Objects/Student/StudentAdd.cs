namespace SchoolApp.DAL.Models.Objects.Student
{
    public sealed class StudentAdd : StudentBase
    {
        public DateTime CreationDate { get; set; }
        public int CreationUser { get; set; }
    }
}
