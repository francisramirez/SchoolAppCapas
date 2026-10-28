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
    public DbSet<Department> Departments { get; set; }
    public DbSet<Course> Courses { get; set; }
    public DbSet<Instructor> Instructors { get; set; }
    public DbSet<CourseEnrollment> CourseEnrollments { get; set; }
    public DbSet<EnrollmentStatus> EnrollmentStatuses { get; set; }
    public DbSet<OnlineCourse> OnlineCourses { get; set; }
    public DbSet<OnsiteCourse> OnsiteCourses { get; set; }
    public DbSet<StudentGrade> StudentGrades { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<CourseEnrollment>().HasKey(e => e.EnrollmentId);
        modelBuilder.Entity<CourseEnrollment>().HasOne<Course>().WithMany()
            .HasForeignKey(e => e.CourseId).OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<CourseEnrollment>().HasOne<Student>().WithMany()
            .HasForeignKey(e => e.StudentId).OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<CourseEnrollment>().HasOne<EnrollmentStatus>().WithMany()
            .HasForeignKey(e => e.EnrollmentStatusId).OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<CourseEnrollment>().HasIndex(e => new { e.CourseId, e.StudentId });

        modelBuilder.Entity<OnlineCourse>().HasKey(e => e.CourseId);
        modelBuilder.Entity<OnlineCourse>().Property(e => e.CourseId).ValueGeneratedNever();
        modelBuilder.Entity<OnlineCourse>().HasOne<Course>().WithOne()
            .HasForeignKey<OnlineCourse>(e => e.CourseId).OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<OnsiteCourse>().HasKey(e => e.CourseId);
        modelBuilder.Entity<OnsiteCourse>().Property(e => e.CourseId).ValueGeneratedNever();
        modelBuilder.Entity<OnsiteCourse>().HasOne<Course>().WithOne()
            .HasForeignKey<OnsiteCourse>(e => e.CourseId).OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<StudentGrade>().HasKey(e => e.EnrollmentId);
        modelBuilder.Entity<StudentGrade>().Property(e => e.EnrollmentId).ValueGeneratedNever();
        modelBuilder.Entity<StudentGrade>().Property(e => e.Grade).HasPrecision(18, 4);
        modelBuilder.Entity<StudentGrade>().HasOne<CourseEnrollment>().WithOne()
            .HasForeignKey<StudentGrade>(e => e.EnrollmentId).OnDelete(DeleteBehavior.Restrict);
    }
}
