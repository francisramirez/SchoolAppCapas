using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using SchoolApp.BL.Configuration;
using SchoolApp.BL.Services;
using SchoolApp.DAL.Data;
using SchoolApp.DAL.Models;
using SchoolApp.DAL.Models.Objects.CourseEnrollment;
using SchoolApp.DAL.Models.Objects.EnrollmentStatus;
using SchoolApp.DAL.Models.Objects.OnlineCourse;
using SchoolApp.DAL.Models.Objects.OnsiteCourse;
using SchoolApp.DAL.Models.Objects.StudentGrade;
using SchoolApp.DAL.Repositories;
using Xunit;

namespace SchoolApp.BL.Tests;

public sealed class AcademicServicesTests : IDisposable
{
    private readonly SqliteConnection connection;
    private readonly SchoolDbContext context;

    public AcademicServicesTests()
    {
        connection = new SqliteConnection("Data Source=:memory:");
        connection.Open();
        context = new SchoolDbContext(new DbContextOptionsBuilder<SchoolDbContext>().UseSqlite(connection).Options);
        context.Database.EnsureCreated();
        context.Departments.Add(new Department { DepartmentId = 1, Name = "Ciencias" });
        context.Students.AddRange(
            new Student { Id = 1, StudentNumber = "A1", FirstName = "Ana", LastName = "Pérez" },
            new Student { Id = 2, StudentNumber = "A2", FirstName = "Luis", LastName = "Gómez" });
        context.Courses.AddRange(
            new Course { CourseId = 1, DepartmentId = 1, Title = "Matemáticas" },
            new Course { CourseId = 2, DepartmentId = 1, Title = "Física" });
        context.EnrollmentStatuses.Add(new EnrollmentStatus
        {
            EnrollmentStatusId = 1, Code = "ENROLLED", Name = "Inscrito", Description = "Estado de prueba", IsActive = true
        });
        context.SaveChanges();
    }

    private CourseEnrollmentServices Enrollments => new(new CourseEnrollmentRepository(context), NullLogger<CourseEnrollmentServices>.Instance);
    private StudentGradeServices Grades(decimal? minimum = 0, decimal? maximum = 100) =>
        new(new StudentGradeRepository(context), NullLogger<StudentGradeServices>.Instance,
            Options.Create(new GradeScaleOptions { Minimum = minimum, Maximum = maximum }));

    private static CourseEnrollmentAdd Enrollment(int studentId = 1, int courseId = 1, int statusId = 1) => new()
    {
        CourseId = courseId, StudentId = studentId, EnrollmentStatusId = statusId,
        EnrollmentDate = new DateTime(2026, 1, 1), CreationUser = 7
    };

    [Fact]
    public async Task EnrollmentRequiresExistingCourseStudentAndEnabledStatus()
    {
        Assert.False((await Enrollments.AddAsync(Enrollment(courseId: 999))).Success);
        Assert.False((await Enrollments.AddAsync(Enrollment(studentId: 999))).Success);
        Assert.False((await Enrollments.AddAsync(Enrollment(statusId: 999))).Success);
        var status = await context.EnrollmentStatuses.SingleAsync();
        status.IsActive = false;
        await context.SaveChangesAsync();
        Assert.False((await Enrollments.AddAsync(Enrollment())).Success);
        status.IsActive = true;
        (await context.Students.SingleAsync(s => s.Id == 1)).Deleted = true;
        await context.SaveChangesAsync();
        Assert.False((await Enrollments.AddAsync(Enrollment())).Success);
        Assert.Empty(await context.CourseEnrollments.ToListAsync());
    }

    [Fact]
    public async Task EnrollmentRejectsDuplicatesAndSupportsStudentQueries()
    {
        var request = Enrollment();
        var before = DateTime.UtcNow;
        Assert.True((await Enrollments.AddAsync(request)).Success);
        Assert.False((await Enrollments.AddAsync(Enrollment())).Success);
        Assert.True((await Enrollments.AddAsync(Enrollment(studentId: 2))).Success);
        var result = await Enrollments.GetByStudentIdAsync(1);
        var record = Assert.Single(result.Data!);
        Assert.Equal(1, record.StudentId);
        var saved = await context.CourseEnrollments.SingleAsync(e => e.StudentId == 1);
        Assert.Equal(7, saved.CreationUser);
        Assert.InRange(saved.CreationDate, before, DateTime.UtcNow);
        Assert.Equal(default, request.CreationDate);
        Assert.False((await Enrollments.GetByStudentIdAsync(0)).Success);
    }

    [Fact]
    public async Task StatusCodesAreNormalizedAndReferencedStatusesCannotBeDeleted()
    {
        var repository = new EnrollmentStatusRepository(context);
        var service = new EnrollmentStatusServices(repository, NullLogger<EnrollmentStatusServices>.Instance);
        Assert.False((await service.AddAsync(new EnrollmentStatusAdd { Code = " ", Name = "Estado", CreationUser = 7 })).Success);
        Assert.False((await service.AddAsync(new EnrollmentStatusAdd { Code = " enrolled ", Name = "Duplicado", CreationUser = 7 })).Success);
        Assert.True((await service.AddAsync(new EnrollmentStatusAdd { Code = " pending ", Name = " Pendiente ", IsActive = true, CreationUser = 7 })).Success);
        var pending = await context.EnrollmentStatuses.SingleAsync(s => s.Code == "PENDING");
        Assert.Equal("Pendiente", pending.Name);
        Assert.True((await Enrollments.AddAsync(Enrollment())).Success);
        Assert.False((await repository.DeleteAsync(new EnrollmentStatusRemove { EnrollmentStatusId = 1 })).Success);
        Assert.True((await repository.DeleteAsync(new EnrollmentStatusRemove { EnrollmentStatusId = pending.EnrollmentStatusId, UserDeleted = 7 })).Success);
        Assert.False((await repository.GetByIdAsync(pending.EnrollmentStatusId))!.Success);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("relative/path")]
    [InlineData("javascript:alert(1)")]
    [InlineData("ftp://example.com/course")]
    public async Task OnlineCourseRejectsInvalidUrls(string? url)
    {
        var service = new OnlineCourseServices(new OnlineCourseRepository(context), NullLogger<OnlineCourseServices>.Instance);
        Assert.False((await service.AddAsync(new OnlineCourseAdd { CourseId = 1, Url = url, CreationUser = 7 })).Success);
        Assert.Empty(await context.OnlineCourses.ToListAsync());
    }

    [Fact]
    public async Task ModalitiesRequireCourseAndPreserveAuditOnUpdate()
    {
        var online = new OnlineCourseServices(new OnlineCourseRepository(context), NullLogger<OnlineCourseServices>.Instance);
        Assert.False((await online.AddAsync(new OnlineCourseAdd { CourseId = 99, Url = "https://example.com", CreationUser = 7 })).Success);
        Assert.True((await online.AddAsync(new OnlineCourseAdd { CourseId = 1, Url = " https://example.com/one ", CreationUser = 7 })).Success);
        var original = await context.OnlineCourses.SingleAsync();
        var creation = original.CreationDate;
        Assert.False((await online.AddAsync(new OnlineCourseAdd { CourseId = 1, Url = "https://example.com/two", CreationUser = 7 })).Success);
        Assert.True((await online.UpdateAsync(new OnlineCourseUpdate { CourseId = 1, Url = "https://example.com/two", ModifyUser = 8 })).Success);
        Assert.Equal(creation, original.CreationDate);
        Assert.Equal(7, original.CreationUser);
        Assert.Equal(8, original.UserMod);

        var onsiteRepository = new OnsiteCourseRepository(context);
        var onsite = new OnsiteCourseServices(onsiteRepository, NullLogger<OnsiteCourseServices>.Instance);
        Assert.False((await onsite.AddAsync(new OnsiteCourseAdd { CourseId = 2, Location = " ", Days = "Lunes", Time = DateTime.UtcNow, CreationUser = 7 })).Success);
        Assert.True((await onsite.AddAsync(new OnsiteCourseAdd { CourseId = 2, Location = " Aula 1 ", Days = " Lunes ", Time = new DateTime(2026, 1, 1, 8, 0, 0), CreationUser = 7 })).Success);
        Assert.Equal("Aula 1", (await onsite.GetByIdAsync(2)).Data!.Location);
        Assert.True((await onsiteRepository.DeleteAsync(new OnsiteCourseRemove { CourseId = 2, UserDeleted = 7 })).Success);
        Assert.Empty((await onsite.GetAllAsync()).Data!);
    }

    [Theory]
    [InlineData(null, null)]
    [InlineData(100, 0)]
    [InlineData(1, 1)]
    public async Task GradeRequiresConfiguredScale(int? minimum, int? maximum)
    {
        var service = Grades(minimum, maximum);
        var result = await service.AddAsync(new StudentGradeAdd { EnrollmentId = 1, CourseId = 1, StudentId = 1, Grade = 50, CreationUser = 7 });
        Assert.False(result.Success);
        Assert.Contains("escala", result.Message!);
        Assert.Empty(await context.StudentGrades.ToListAsync());
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(101)]
    public async Task GradeRejectsValuesOutsideConfiguredScale(int grade)
    {
        Assert.False((await Grades().AddAsync(new StudentGradeAdd { EnrollmentId = 1, CourseId = 1, StudentId = 1, Grade = grade, CreationUser = 7 })).Success);
        Assert.Empty(await context.StudentGrades.ToListAsync());
    }

    [Fact]
    public async Task GradeRequiresMatchingEnrollmentAndProtectsHistory()
    {
        Assert.True((await Enrollments.AddAsync(Enrollment())).Success);
        var enrollment = await context.CourseEnrollments.SingleAsync();
        var grade = new StudentGradeAdd { EnrollmentId = enrollment.EnrollmentId, CourseId = 1, StudentId = 2, Grade = 75, CreationUser = 7 };
        Assert.False((await Grades().AddAsync(grade)).Success);
        grade.StudentId = 1;
        Assert.True((await Grades().AddAsync(grade)).Success);
        Assert.False((await Grades().AddAsync(grade)).Success);
        Assert.Single((await Grades().GetByStudentIdAsync(1)).Data!);
        Assert.Empty((await Grades().GetByStudentIdAsync(2)).Data!);

        Assert.False((await Enrollments.UpdateAsync(new CourseEnrollmentUpdate
        {
            EnrollmentId = enrollment.EnrollmentId, CourseId = 2, StudentId = 1, EnrollmentStatusId = 1,
            EnrollmentDate = enrollment.EnrollmentDate, ModifyUser = 7
        })).Success);
        Assert.False((await new CourseEnrollmentRepository(context).DeleteAsync(new CourseEnrollmentRemove { EnrollmentId = enrollment.EnrollmentId })).Success);
        Assert.True((await Grades().UpdateAsync(new StudentGradeUpdate
        {
            EnrollmentId = enrollment.EnrollmentId, CourseId = 1, StudentId = 1, Grade = 90, ModifyUser = 8
        })).Success);
        var saved = await context.StudentGrades.SingleAsync();
        Assert.Equal(90m, saved.Grade);
        Assert.Equal(7, saved.CreationUser);
        Assert.Equal(8, saved.UserMod);
    }

    [Fact]
    public async Task ServicesRejectNullRequestsAndInvalidIds()
    {
        Assert.False((await Enrollments.AddAsync(null)).Success);
        Assert.False((await Enrollments.UpdateAsync(null)).Success);
        Assert.False((await Enrollments.GetByIdAsync(0)).Success);
        Assert.False((await Grades().AddAsync(null)).Success);
        Assert.False((await Grades().UpdateAsync(null)).Success);
        Assert.False((await Grades().GetByStudentIdAsync(-1)).Success);
        Assert.False((await new StudentGradeRepository(context).GetByIdAsync(999))!.Success);
    }

    [Fact]
    public async Task PersistenceFailuresAreReturnedAsControlledErrors()
    {
        context.Dispose();
        Assert.False((await Enrollments.GetAllAsync()).Success);
        Assert.False((await Enrollments.AddAsync(Enrollment())).Success);
        Assert.False((await Grades().GetAllAsync()).Success);
        Assert.False((await new OnlineCourseServices(new OnlineCourseRepository(context),
            NullLogger<OnlineCourseServices>.Instance).GetAllAsync()).Success);
        Assert.False((await new OnsiteCourseServices(new OnsiteCourseRepository(context),
            NullLogger<OnsiteCourseServices>.Instance).GetAllAsync()).Success);
        Assert.False((await new EnrollmentStatusServices(new EnrollmentStatusRepository(context),
            NullLogger<EnrollmentStatusServices>.Instance).GetAllAsync()).Success);
    }

    [Fact]
    public void NewServicesResolveThroughDependencyInjection()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton(context);
        services.AddScoped<ICourseEnrollmentRepository, CourseEnrollmentRepository>();
        services.AddScoped<IEnrollmentStatusRepository, EnrollmentStatusRepository>();
        services.AddScoped<IOnlineCourseRepository, OnlineCourseRepository>();
        services.AddScoped<IOnsiteCourseRepository, OnsiteCourseRepository>();
        services.AddScoped<IStudentGradeRepository, StudentGradeRepository>();
        services.AddBusinessLogic();
        using var provider = services.BuildServiceProvider();
        using var scope = provider.CreateScope();
        Assert.IsType<CourseEnrollmentServices>(scope.ServiceProvider.GetRequiredService<ICourseEnrollmentServices>());
        Assert.IsType<EnrollmentStatusServices>(scope.ServiceProvider.GetRequiredService<IEnrollmentStatusServices>());
        Assert.IsType<OnlineCourseServices>(scope.ServiceProvider.GetRequiredService<IOnlineCourseServices>());
        Assert.IsType<OnsiteCourseServices>(scope.ServiceProvider.GetRequiredService<IOnsiteCourseServices>());
        Assert.IsType<StudentGradeServices>(scope.ServiceProvider.GetRequiredService<IStudentGradeServices>());
    }

    public void Dispose()
    {
        context.Dispose();
        connection.Dispose();
    }
}
