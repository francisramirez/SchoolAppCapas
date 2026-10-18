#nullable disable
using SchoolApp.DAL.Models.Base;

namespace SchoolApp.DAL.Models;

public partial class Student : AuditEntity
{
    public int Id { get; set; }

    public string StudentNumber { get; set; }

    public string LastName { get; set; }

    public string FirstName { get; set; }

    public DateTime? EnrollmentDate { get; set; }

    
}