using Microsoft.EntityFrameworkCore;
using SchoolApp.DAL.Data;
using SchoolApp.DAL.Models;
using SchoolApp.DAL.Models.Objects.Course;
using SchoolApp.DAL.Models.Result;

namespace SchoolApp.DAL.Repositories;

public class CourseRepository : ICourseRepository
{
    private readonly SchoolDbContext context;

    public CourseRepository(SchoolDbContext context)
    {
        this.context = context;
    }

    public async Task<OperationResult<bool>> AddAsync(CourseAdd course)
    {
        try
        {
            if (course is null)
                return Failure<bool>("El objeto CourseAdd es requerido.");

            var validation = ValidateCourse(course.Title, course.Credits, course.DepartmentId);
            if (validation is not null)
                return Failure<bool>(validation);

            if (!await context.Departments.AnyAsync(d => d.DepartmentId == course.DepartmentId && !d.Deleted))
                return Failure<bool>("El departamento no se encuentra registrado o fue eliminado.");

            await context.Courses.AddAsync(new Course
            {
                Title = course.Title!.Trim(),
                Credits = course.Credits,
                DepartmentId = course.DepartmentId,
                CreationDate = course.CreationDate,
                CreationUser = course.CreationUser
            });
            await context.SaveChangesAsync();
            return new OperationResult<bool> { Data = true, Message = "Curso registrado correctamente." };
        }
        catch (Exception ex)
        {
            return Failure<bool>($"Error guardando los datos del curso. {ex.Message}");
        }
    }

    public async Task<OperationResult<bool>> UpdateAsync(CourseUpdate course)
    {
        try
        {
            if (course is null)
                return Failure<bool>("El objeto CourseUpdate es requerido.");
            if (course.CourseId <= 0)
                return Failure<bool>("El identificador del curso debe ser mayor que cero.");

            var validation = ValidateCourse(course.Title, course.Credits, course.DepartmentId);
            if (validation is not null)
                return Failure<bool>(validation);

            var entity = await context.Courses.FirstOrDefaultAsync(c => c.CourseId == course.CourseId && !c.Deleted);
            if (entity is null)
                return Failure<bool>("El curso no se encuentra registrado o fue eliminado.");

            if (!await context.Departments.AnyAsync(d => d.DepartmentId == course.DepartmentId && !d.Deleted))
                return Failure<bool>("El departamento no se encuentra registrado o fue eliminado.");

            entity.Title = course.Title!.Trim();
            entity.Credits = course.Credits;
            entity.DepartmentId = course.DepartmentId;
            entity.ModifyDate = course.ModifyDate;
            entity.UserMod = course.ModifyUser;
            await context.SaveChangesAsync();
            return new OperationResult<bool> { Data = true, Message = "Curso actualizado correctamente." };
        }
        catch (Exception ex)
        {
            return Failure<bool>($"Error actualizando los datos del curso. {ex.Message}");
        }
    }

    public async Task<OperationResult<bool>> DeleteAsync(CourseRemove course)
    {
        try
        {
            if (course is null)
                return Failure<bool>("El objeto CourseRemove es requerido.");
            if (course.CourseId <= 0)
                return Failure<bool>("El identificador del curso debe ser mayor que cero.");

            var entity = await context.Courses.FirstOrDefaultAsync(c => c.CourseId == course.CourseId && !c.Deleted);
            if (entity is null)
                return Failure<bool>("El curso no se encuentra registrado o fue eliminado.");

            entity.Deleted = true;
            entity.DeletedDate = course.DeletedDate;
            entity.UserDeleted = course.UserDeleted;
            await context.SaveChangesAsync();
            return new OperationResult<bool> { Data = true, Message = "Curso eliminado correctamente." };
        }
        catch (Exception ex)
        {
            return Failure<bool>($"Error eliminando los datos del curso. {ex.Message}");
        }
    }

    public async Task<OperationResult<List<Course>>> GetAllAsync()
    {
        try
        {
            var courses = await context.Courses.AsNoTracking()
                .Where(c => !c.Deleted)
                .ToListAsync();
            return new OperationResult<List<Course>> { Data = courses };
        }
        catch (Exception ex)
        {
            return Failure<List<Course>>($"Error consultando los cursos. {ex.Message}");
        }
    }

    public async Task<OperationResult<CourseModel>?> GetByIdAsync(int id)
    {
        try
        {
            if (id <= 0)
                return Failure<CourseModel>("El identificador del curso debe ser mayor que cero.");

            var course = await context.Courses.AsNoTracking()
                .Where(c => c.CourseId == id && !c.Deleted)
                .Select(c => new CourseModel
                {
                    CourseId = c.CourseId,
                    Title = c.Title,
                    Credits = c.Credits,
                    DepartmentId = c.DepartmentId
                })
                .FirstOrDefaultAsync();
            if (course is null)
                return Failure<CourseModel>("El curso no se encuentra registrado o fue eliminado.");

            return new OperationResult<CourseModel> { Data = course };
        }
        catch (Exception ex)
        {
            return Failure<CourseModel>($"Error consultando los datos del curso. {ex.Message}");
        }
    }

    private static string? ValidateCourse(string? title, int credits, int departmentId)
    {
        if (string.IsNullOrWhiteSpace(title))
            return "El título del curso es requerido.";
        if (credits < 0)
            return "Los créditos no pueden ser negativos.";
        if (departmentId <= 0)
            return "El identificador del departamento debe ser mayor que cero.";
        return null;
    }

    private static OperationResult<T> Failure<T>(string message) => new()
    {
        Success = false,
        Message = message
    };
}
