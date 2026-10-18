#nullable disable

using SchoolApp.DAL.Models.Base;

namespace SchoolApp.DAL.Models;

public partial class Instructor : AuditEntity
{
    public int Id { get; set; }

    public string LastName { get; set; }

    public string FirstName { get; set; }

    public DateTime? HireDate { get; set; }

    
}