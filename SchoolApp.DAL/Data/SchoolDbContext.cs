using Microsoft.EntityFrameworkCore;
using SchoolApp.DAL.Models;

namespace SchoolApp.DAL.Data;
public class SchoolDbContext : DbContext
{
    public SchoolDbContext(DbContextOptions<SchoolDbContext> options)
        : base(options)
    {
        
    }

    public DbSet<Student> Students { get; set; }

}
