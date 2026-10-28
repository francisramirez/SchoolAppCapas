using System.Data;
using Microsoft.EntityFrameworkCore;
using SchoolApp.DAL.Data;
using SchoolApp.DAL.Models;
using SchoolApp.DAL.Models.Objects.OnlineCourse;
using SchoolApp.DAL.Models.Result;

namespace SchoolApp.DAL.Repositories;

public class OnlineCourseRepository : IOnlineCourseRepository
{
    private readonly SchoolDbContext context;

    public OnlineCourseRepository(SchoolDbContext context)
    {
        this.context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<OperationResult<bool>> AddAsync(OnlineCourseAdd item)
    {
        try
        {
            if (item is null)
                return Failure<bool>("El objeto OnlineCourseAdd es requerido.");

            var validation = ValidateOnlineCourse(item);
            if (validation is not null)
                return Failure<bool>(validation);

            if (await context.OnlineCourses.AnyAsync(e => e.CourseId == item.CourseId))
                return Failure<bool>("Ya existe un registro para este identificador, incluso si fue eliminado.");

            var relationError = await ValidateRelationsAsync(item);
            if (relationError is not null)
                return Failure<bool>(relationError);

            await context.OnlineCourses.AddAsync(new OnlineCourse
            {
                CourseId = item.CourseId,
                Url = item.Url!.Trim(),
                CreationDate = item.CreationDate,
                CreationUser = item.CreationUser
            });
            await context.SaveChangesAsync();
            return new OperationResult<bool> { Data = true, Message = "Registro completado." };
        }
        catch (Exception)
        {
            return Failure<bool>("No fue posible registrar el registro de curso en línea.");
        }
    }

    public async Task<OperationResult<bool>> UpdateAsync(OnlineCourseUpdate item)
    {
        try
        {
            if (item is null)
                return Failure<bool>("El objeto OnlineCourseUpdate es requerido.");

            var validation = ValidateOnlineCourse(item);
            if (validation is not null)
                return Failure<bool>(validation);

            var entity = await context.OnlineCourses.FirstOrDefaultAsync(e => e.CourseId == item.CourseId && !e.Deleted);
            if (entity is null)
                return Failure<bool>("El registro no existe o fue eliminado.");

            var relationError = await ValidateRelationsAsync(item, item.CourseId);
            if (relationError is not null)
                return Failure<bool>(relationError);

            entity.Url = item.Url!.Trim();
            entity.ModifyDate = item.ModifyDate;
            entity.UserMod = item.ModifyUser;
            await context.SaveChangesAsync();
            return new OperationResult<bool> { Data = true, Message = "Actualización completada." };
        }
        catch (Exception)
        {
            return Failure<bool>("No fue posible actualizar el registro de curso en línea.");
        }
    }

    public async Task<OperationResult<bool>> DeleteAsync(OnlineCourseRemove item)
    {
        try
        {
            if (item is null)
                return Failure<bool>("El objeto OnlineCourseRemove es requerido.");
            if (item.CourseId <= 0)
                return Failure<bool>("El identificador debe ser mayor que cero.");

            var entity = await context.OnlineCourses.FirstOrDefaultAsync(e => e.CourseId == item.CourseId && !e.Deleted);
            if (entity is null)
                return Failure<bool>("El registro no existe o fue eliminado.");

            entity.Deleted = true;
            entity.DeletedDate = item.DeletedDate;
            entity.UserDeleted = item.UserDeleted;
            await context.SaveChangesAsync();
            return new OperationResult<bool> { Data = true, Message = "Registro eliminado correctamente." };
        }
        catch (Exception)
        {
            return Failure<bool>("No fue posible eliminar el registro de curso en línea.");
        }
    }

    public async Task<OperationResult<List<OnlineCourse>>> GetAllAsync()
    {
        try
        {
            var data = await context.OnlineCourses.AsNoTracking().Where(e => !e.Deleted).ToListAsync();
            return new OperationResult<List<OnlineCourse>> { Data = data };
        }
        catch (Exception)
        {
            return Failure<List<OnlineCourse>>("No fue posible consultar los registros de cursos en línea.");
        }
    }

    public async Task<OperationResult<OnlineCourseModel>?> GetByIdAsync(int id)
    {
        try
        {
            if (id <= 0)
                return Failure<OnlineCourseModel>("El identificador debe ser mayor que cero.");

            var data = await context.OnlineCourses.AsNoTracking()
                .Where(e => e.CourseId == id && !e.Deleted)
                .Select(e => new OnlineCourseModel
                {
                    CourseId = e.CourseId,
                    Url = e.Url
                }).FirstOrDefaultAsync();
            if (data is null)
                return Failure<OnlineCourseModel>("El registro no existe o fue eliminado.");

            return new OperationResult<OnlineCourseModel> { Data = data };
        }
        catch (Exception)
        {
            return Failure<OnlineCourseModel>("No fue posible consultar el registro de curso en línea.");
        }
    }

    private static string? ValidateOnlineCourse(OnlineCourseBase item)
    {
        if (item.CourseId <= 0)
            return "El identificador del curso debe ser mayor que cero.";
        if (!Uri.TryCreate(item.Url?.Trim(), UriKind.Absolute, out var uri)
            || (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
            return "La URL debe ser una dirección absoluta HTTP o HTTPS.";
        return null;
    }

    private async Task<string?> ValidateRelationsAsync(OnlineCourseBase item, int? exceptId = null)
    {
        if (!await context.Courses.AnyAsync(c => c.CourseId == item.CourseId && !c.Deleted))
            return "El curso no existe o fue eliminado.";
        return null;
    }

    private static OperationResult<T> Failure<T>(string message) => new()
    {
        Success = false,
        Message = message
    };
}
