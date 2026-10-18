#nullable disable

namespace SchoolApp.DAL.Models;

public partial class EnrollmentStatus
{
    public int EnrollmentStatusId { get; set; }

    public string Code { get; set; }

    public string Name { get; set; }

    public string Description { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreationDate { get; set; }

    public int CreationUser { get; set; }

    public DateTime? ModifyDate { get; set; }

    public int? UserMod { get; set; }
}