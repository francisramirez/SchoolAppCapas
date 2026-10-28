using Microsoft.Extensions.DependencyInjection;
using SchoolApp.BL.Services;
using SchoolApp.BL.Configuration;
namespace SchoolApp.BL;
public static class DependencyInjection
{
    public static IServiceCollection AddBusinessLogic(this IServiceCollection services)
    {
        services.AddScoped<IDepartmentServices, DepartmentServices>();
        services.AddScoped<IStudentService, StudentService>();
        services.AddScoped<ICourseServices, CourseServices>();
        services.AddScoped<IInstructorServices, InstructorServices>();
        services.AddOptions<GradeScaleOptions>();
        services.AddScoped<ICourseEnrollmentServices, CourseEnrollmentServices>();
        services.AddScoped<IEnrollmentStatusServices, EnrollmentStatusServices>();
        services.AddScoped<IOnlineCourseServices, OnlineCourseServices>();
        services.AddScoped<IOnsiteCourseServices, OnsiteCourseServices>();
        services.AddScoped<IStudentGradeServices, StudentGradeServices>();
        return services;
    }
}
