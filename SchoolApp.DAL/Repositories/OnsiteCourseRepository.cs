using System.Data;
using Microsoft.EntityFrameworkCore;
using SchoolApp.DAL.Data;
using SchoolApp.DAL.Models;
using SchoolApp.DAL.Models.Objects.OnsiteCourse;
using SchoolApp.DAL.Models.Result;

namespace SchoolApp.DAL.Repositories;

public class OnsiteCourseRepository : IOnsiteCourseRepository
{
    private readonly SchoolDbContext context;

    public OnsiteCourseRepository(SchoolDbContext context)
    {
        this.context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<OperationResult<bool>> AddAsync(OnsiteCourseAdd item)
    {
        try
        {
            if (item is null)
                return Failure<bool>("El objeto OnsiteCourseAdd es requerido.");

            var validation = ValidateOnsiteCourse(item);
            if (validation is not null)
                return Failure<bool>(validation);

            if (await context.OnsiteCourses.AnyAsync(e => e.CourseId == item.CourseId))
                return Failure<bool>("Ya existe un registro para este identificador, incluso si fue eliminado.");

            var relationError = await ValidateRelationsAsync(item);
            if (relationError is not null)
                return Failure<bool>(relationError);

            await context.OnsiteCourses.AddAsync(new OnsiteCourse
            {
                CourseId = item.CourseId,
                Location = item.Location!.Trim(),
                Days = item.Days!.Trim(),
                Time = item.Time,
                CreationDate = item.CreationDate,
                CreationUser = item.CreationUser
            });
            await context.SaveChangesAsync();
            return new OperationResult<bool> { Data = true, Message = "Registro completado." };
        }
        catch (Exception)
        {
            return Failure<bool>("No fue posible registrar el registro de curso presencial.");
        }
    }

    public async Task<OperationResult<bool>> UpdateAsync(OnsiteCourseUpdate item)
    {
        try
        {
            if (item is null)
                return Failure<bool>("El objeto OnsiteCourseUpdate es requerido.");

            var validation = ValidateOnsiteCourse(item);
            if (validation is not null)
                return Failure<bool>(validation);

            var entity = await context.OnsiteCourses.FirstOrDefaultAsync(e => e.CourseId == item.CourseId && !e.Deleted);
            if (entity is null)
                return Failure<bool>("El registro no existe o fue eliminado.");

            var relationError = await ValidateRelationsAsync(item, item.CourseId);
            if (relationError is not null)
                return Failure<bool>(relationError);

            entity.Location = item.Location!.Trim();
            entity.Days = item.Days!.Trim();
            entity.Time = item.Time;
            entity.ModifyDate = item.ModifyDate;
            entity.UserMod = item.ModifyUser;
            await context.SaveChangesAsync();
            return new OperationResult<bool> { Data = true, Message = "Actualización completada." };
        }
        catch (Exception)
        {
            return Failure<bool>("No fue posible actualizar el registro de curso presencial.");
        }
    }

    public async Task<OperationResult<bool>> DeleteAsync(OnsiteCourseRemove item)
    {
        try
        {
            if (item is null)
                return Failure<bool>("El objeto OnsiteCourseRemove es requerido.");
            if (item.CourseId <= 0)
                return Failure<bool>("El identificador debe ser mayor que cero.");

            var entity = await context.OnsiteCourses.FirstOrDefaultAsync(e => e.CourseId == item.CourseId && !e.Deleted);
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
            return Failure<bool>("No fue posible eliminar el registro de curso presencial.");
        }
    }

    public async Task<OperationResult<List<OnsiteCourse>>> GetAllAsync()
    {
        try
        {
            var data = await context.OnsiteCourses.AsNoTracking().Where(e => !e.Deleted).ToListAsync();
            return new OperationResult<List<OnsiteCourse>> { Data = data };
        }
        catch (Exception)
        {
            return Failure<List<OnsiteCourse>>("No fue posible consultar los registros de cursos presenciales.");
        }
    }

    public async Task<OperationResult<OnsiteCourseModel>?> GetByIdAsync(int id)
    {
        try
        {
            if (id <= 0)
                return Failure<OnsiteCourseModel>("El identificador debe ser mayor que cero.");

            var data = await context.OnsiteCourses.AsNoTracking()
                .Where(e => e.CourseId == id && !e.Deleted)
                .Select(e => new OnsiteCourseModel
                {
                    CourseId = e.CourseId,
                    Location = e.Location,
                    Days = e.Days,
                    Time = e.Time
                }).FirstOrDefaultAsync();
            if (data is null)
                return Failure<OnsiteCourseModel>("El registro no existe o fue eliminado.");

            return new OperationResult<OnsiteCourseModel> { Data = data };
        }
        catch (Exception)
        {
            return Failure<OnsiteCourseModel>("No fue posible consultar el registro de curso presencial.");
        }
    }

    private static string? ValidateOnsiteCourse(OnsiteCourseBase item)
    {
        if (item.CourseId <= 0)
            return "El identificador del curso debe ser mayor que cero.";
        if (string.IsNullOrWhiteSpace(item.Location))
            return "La ubicación es requerida.";
        if (string.IsNullOrWhiteSpace(item.Days))
            return "Los días de clase son requeridos.";
        if (item.Time == default)
            return "La hora de clase es requerida.";
        return null;
    }

    private async Task<string?> ValidateRelationsAsync(OnsiteCourseBase item, int? exceptId = null)
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
