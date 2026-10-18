#nullable disable

namespace SchoolApp.DAL.Models;

public partial class OfficeAssignment
{
    public int InstructorId { get; set; }

    public string Location { get; set; }

    public byte[] Timestamp { get; set; }
}