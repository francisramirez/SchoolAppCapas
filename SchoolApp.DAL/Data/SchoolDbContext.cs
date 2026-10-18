using Microsoft.EntityFrameworkCore;
using SchoolApp.DAL.Models;

namespace SchoolApp.DAL.Data;

/// <summary>
/// Db context for the School application, representing the database context for managing student data.
/// </summary>
public class SchoolDbContext : DbContext
{
    public SchoolDbContext(DbContextOptions<SchoolDbContext> options)
        : base(options)
    {
        
    }

    public DbSet<Student> Students { get; set; }

}
