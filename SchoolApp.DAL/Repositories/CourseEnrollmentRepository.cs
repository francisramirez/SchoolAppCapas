using System.Data;
using Microsoft.EntityFrameworkCore;
using SchoolApp.DAL.Data;
using SchoolApp.DAL.Models;
using SchoolApp.DAL.Models.Objects.CourseEnrollment;
using SchoolApp.DAL.Models.Result;

namespace SchoolApp.DAL.Repositories;

public class CourseEnrollmentRepository : ICourseEnrollmentRepository
{
    private readonly SchoolDbContext context;

    public CourseEnrollmentRepository(SchoolDbContext context)
    {
        this.context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<OperationResult<bool>> AddAsync(CourseEnrollmentAdd item)
    {
        try
        {
            if (item is null)
                return Failure<bool>("El objeto CourseEnrollmentAdd es requerido.");

            var validation = ValidateCourseEnrollment(item);
            if (validation is not null)
                return Failure<bool>(validation);

            await using var transaction = await context.Database.BeginTransactionAsync(IsolationLevel.Serializable);
            var relationError = await ValidateRelationsAsync(item);
            if (relationError is not null)
                return Failure<bool>(relationError);

            await context.CourseEnrollments.AddAsync(new CourseEnrollment
            {
                CourseId = item.CourseId,
                StudentId = item.StudentId,
                EnrollmentDate = item.EnrollmentDate,
                EnrollmentStatusId = item.EnrollmentStatusId,
                CreationDate = item.CreationDate,
                CreationUser = item.CreationUser
            });
            await context.SaveChangesAsync();
            await transaction.CommitAsync();
            return new OperationResult<bool> { Data = true, Message = "Registro completado." };
        }
        catch (Exception)
        {
            return Failure<bool>("No fue posible registrar el registro de inscripción.");
        }
    }

    public async Task<OperationResult<bool>> UpdateAsync(CourseEnrollmentUpdate item)
    {
        try
        {
            if (item is null)
                return Failure<bool>("El objeto CourseEnrollmentUpdate es requerido.");
            if (item.EnrollmentId <= 0)
                return Failure<bool>("El identificador debe ser mayor que cero.");

            var validation = ValidateCourseEnrollment(item);
            if (validation is not null)
                return Failure<bool>(validation);

            await using var transaction = await context.Database.BeginTransactionAsync(IsolationLevel.Serializable);
            var entity = await context.CourseEnrollments.FirstOrDefaultAsync(e => e.EnrollmentId == item.EnrollmentId && !e.Deleted);
            if (entity is null)
                return Failure<bool>("El registro no existe o fue eliminado.");

            var relationError = await ValidateRelationsAsync(item, item.EnrollmentId);
            if (relationError is not null)
                return Failure<bool>(relationError);

            entity.CourseId = item.CourseId;
            entity.StudentId = item.StudentId;
            entity.EnrollmentDate = item.EnrollmentDate;
            entity.EnrollmentStatusId = item.EnrollmentStatusId;
            entity.ModifyDate = item.ModifyDate;
            entity.UserMod = item.ModifyUser;
            await context.SaveChangesAsync();
            await transaction.CommitAsync();
            return new OperationResult<bool> { Data = true, Message = "Actualización completada." };
        }
        catch (Exception)
        {
            return Failure<bool>("No fue posible actualizar el registro de inscripción.");
        }
    }

    public async Task<OperationResult<bool>> DeleteAsync(CourseEnrollmentRemove item)
    {
        try
        {
            if (item is null)
                return Failure<bool>("El objeto CourseEnrollmentRemove es requerido.");
            if (item.EnrollmentId <= 0)
                return Failure<bool>("El identificador debe ser mayor que cero.");

            await using var transaction = await context.Database.BeginTransactionAsync(IsolationLevel.Serializable);
            var entity = await context.CourseEnrollments.FirstOrDefaultAsync(e => e.EnrollmentId == item.EnrollmentId && !e.Deleted);
            if (entity is null)
                return Failure<bool>("El registro no existe o fue eliminado.");

            if (await context.StudentGrades.AnyAsync(g => g.EnrollmentId == item.EnrollmentId && !g.Deleted))
                return Failure<bool>("No se puede eliminar una inscripción con calificaciones registradas.");
            entity.Deleted = true;
            entity.DeletedDate = item.DeletedDate;
            entity.UserDeleted = item.UserDeleted;
            await context.SaveChangesAsync();
            await transaction.CommitAsync();
            return new OperationResult<bool> { Data = true, Message = "Registro eliminado correctamente." };
        }
        catch (Exception)
        {
            return Failure<bool>("No fue posible eliminar el registro de inscripción.");
        }
    }

    public async Task<OperationResult<List<CourseEnrollment>>> GetAllAsync()
    {
        try
        {
            var data = await context.CourseEnrollments.AsNoTracking().Where(e => !e.Deleted).ToListAsync();
            return new OperationResult<List<CourseEnrollment>> { Data = data };
        }
        catch (Exception)
        {
            return Failure<List<CourseEnrollment>>("No fue posible consultar los registros de inscripciones.");
        }
    }

    public async Task<OperationResult<CourseEnrollmentModel>?> GetByIdAsync(int id)
    {
        try
        {
            if (id <= 0)
                return Failure<CourseEnrollmentModel>("El identificador debe ser mayor que cero.");

            var data = await context.CourseEnrollments.AsNoTracking()
                .Where(e => e.EnrollmentId == id && !e.Deleted)
                .Select(e => new CourseEnrollmentModel
                {
                    EnrollmentId = e.EnrollmentId,
                    CourseId = e.CourseId,
                    StudentId = e.StudentId,
                    EnrollmentDate = e.EnrollmentDate,
                    EnrollmentStatusId = e.EnrollmentStatusId
                }).FirstOrDefaultAsync();
            if (data is null)
                return Failure<CourseEnrollmentModel>("El registro no existe o fue eliminado.");

            return new OperationResult<CourseEnrollmentModel> { Data = data };
        }
        catch (Exception)
        {
            return Failure<CourseEnrollmentModel>("No fue posible consultar el registro de inscripción.");
        }
    }

    public async Task<OperationResult<List<CourseEnrollment>>> GetByStudentIdAsync(int studentId)
    {
        try
        {
            if (studentId <= 0)
                return Failure<List<CourseEnrollment>>("El identificador del estudiante debe ser mayor que cero.");

            var data = await context.CourseEnrollments.AsNoTracking()
                .Where(e => e.StudentId == studentId && !e.Deleted).ToListAsync();
            return new OperationResult<List<CourseEnrollment>> { Data = data };
        }
        catch (Exception)
        {
            return Failure<List<CourseEnrollment>>("No fue posible consultar los registros del estudiante.");
        }
    }

    private static string? ValidateCourseEnrollment(CourseEnrollmentBase item)
    {
        if (item.CourseId <= 0 || item.StudentId <= 0 || item.EnrollmentStatusId <= 0)
            return "Los identificadores de curso, estudiante y estado deben ser mayores que cero.";
        if (item.EnrollmentDate == default)
            return "La fecha de inscripción es requerida.";
        return null;
    }

    private async Task<string?> ValidateRelationsAsync(CourseEnrollmentBase item, int? exceptId = null)
    {
        if (!await context.Courses.AnyAsync(c => c.CourseId == item.CourseId && !c.Deleted))
            return "El curso no existe o fue eliminado.";
        if (!await context.Students.AnyAsync(s => s.Id == item.StudentId && !s.Deleted))
            return "El estudiante no existe o fue eliminado.";
        if (!await context.EnrollmentStatuses.AnyAsync(s => s.EnrollmentStatusId == item.EnrollmentStatusId && s.IsActive && !s.Deleted))
            return "El estado de inscripción no existe o no está habilitado.";
        if (await context.CourseEnrollments.AnyAsync(e => !e.Deleted && e.CourseId == item.CourseId && e.StudentId == item.StudentId
            && (!exceptId.HasValue || e.EnrollmentId != exceptId.Value)))
            return "Ya existe una inscripción no eliminada para el estudiante y el curso.";
        if (exceptId.HasValue && await context.StudentGrades.AnyAsync(g => g.EnrollmentId == exceptId.Value && !g.Deleted
            && (g.CourseId != item.CourseId || g.StudentId != item.StudentId)))
            return "No se puede cambiar el estudiante o curso de una inscripción con calificación registrada.";
        return null;
    }

    private static OperationResult<T> Failure<T>(string message) => new()
    {
        Success = false,
        Message = message
    };
}
