using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SchoolApp.DAL.Data;
using SchoolApp.DAL.Repositories;
namespace SchoolApp.DAL;
public static class DependencyInjection
{
    public static IServiceCollection AddDataAccess(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<SchoolDbContext>(options => options.UseSqlServer(configuration.GetConnectionString("SchoolDb")));
        services.AddScoped<IStudentRepository, StudentRepository>();
        services.AddScoped<IDepartmentRepository, DepartmentRepository>();
        services.AddScoped<ICourseRepository, CourseRepository>();
        services.AddScoped<IInstructorRepository, InstructorRepository>();
        services.AddScoped<ICourseEnrollmentRepository, CourseEnrollmentRepository>();
        services.AddScoped<IEnrollmentStatusRepository, EnrollmentStatusRepository>();
        services.AddScoped<IOnlineCourseRepository, OnlineCourseRepository>();
        services.AddScoped<IOnsiteCourseRepository, OnsiteCourseRepository>();
        services.AddScoped<IStudentGradeRepository, StudentGradeRepository>();
        return services;
    }
}
