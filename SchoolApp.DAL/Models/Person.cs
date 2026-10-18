#nullable disable

namespace SchoolApp.DAL.Models;

public partial class Person
{
    public int PersonId { get; set; }

    public string LastName { get; set; }

    public string FirstName { get; set; }

    public string Discriminator { get; set; }
}