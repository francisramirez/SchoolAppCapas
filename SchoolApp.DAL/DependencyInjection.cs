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
        services.AddDbContext<SchoolDbContext>(options => options.UseSqlite(configuration.GetConnectionString("SchoolDb")));
        services.AddScoped<IStudentRepository, StudentRepository>();
        return services;
    }
}
