using System.Data;
using Microsoft.EntityFrameworkCore;
using SchoolApp.DAL.Data;
using SchoolApp.DAL.Models;
using SchoolApp.DAL.Models.Objects.EnrollmentStatus;
using SchoolApp.DAL.Models.Result;

namespace SchoolApp.DAL.Repositories;

public class EnrollmentStatusRepository : IEnrollmentStatusRepository
{
    private readonly SchoolDbContext context;

    public EnrollmentStatusRepository(SchoolDbContext context)
    {
        this.context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<OperationResult<bool>> AddAsync(EnrollmentStatusAdd item)
    {
        try
        {
            if (item is null)
                return Failure<bool>("El objeto EnrollmentStatusAdd es requerido.");

            var validation = ValidateEnrollmentStatus(item);
            if (validation is not null)
                return Failure<bool>(validation);

            await using var transaction = await context.Database.BeginTransactionAsync(IsolationLevel.Serializable);
            var relationError = await ValidateRelationsAsync(item);
            if (relationError is not null)
                return Failure<bool>(relationError);

            await context.EnrollmentStatuses.AddAsync(new EnrollmentStatus
            {
                Code = item.Code!.Trim().ToUpperInvariant(),
                Name = item.Name!.Trim(),
                Description = item.Description?.Trim() ?? string.Empty,
                IsActive = item.IsActive,
                CreationDate = item.CreationDate,
                CreationUser = item.CreationUser
            });
            await context.SaveChangesAsync();
            await transaction.CommitAsync();
            return new OperationResult<bool> { Data = true, Message = "Registro completado." };
        }
        catch (Exception)
        {
            return Failure<bool>("No fue posible registrar el registro de estado de inscripción.");
        }
    }

    public async Task<OperationResult<bool>> UpdateAsync(EnrollmentStatusUpdate item)
    {
        try
        {
            if (item is null)
                return Failure<bool>("El objeto EnrollmentStatusUpdate es requerido.");
            if (item.EnrollmentStatusId <= 0)
                return Failure<bool>("El identificador debe ser mayor que cero.");

            var validation = ValidateEnrollmentStatus(item);
            if (validation is not null)
                return Failure<bool>(validation);

            await using var transaction = await context.Database.BeginTransactionAsync(IsolationLevel.Serializable);
            var entity = await context.EnrollmentStatuses.FirstOrDefaultAsync(e => e.EnrollmentStatusId == item.EnrollmentStatusId && !e.Deleted);
            if (entity is null)
                return Failure<bool>("El registro no existe o fue eliminado.");

            var relationError = await ValidateRelationsAsync(item, item.EnrollmentStatusId);
            if (relationError is not null)
                return Failure<bool>(relationError);

            entity.Code = item.Code!.Trim().ToUpperInvariant();
            entity.Name = item.Name!.Trim();
            entity.Description = item.Description?.Trim() ?? string.Empty;
            entity.IsActive = item.IsActive;
            entity.ModifyDate = item.ModifyDate;
            entity.UserMod = item.ModifyUser;
            await context.SaveChangesAsync();
            await transaction.CommitAsync();
            return new OperationResult<bool> { Data = true, Message = "Actualización completada." };
        }
        catch (Exception)
        {
            return Failure<bool>("No fue posible actualizar el registro de estado de inscripción.");
        }
    }

    public async Task<OperationResult<bool>> DeleteAsync(EnrollmentStatusRemove item)
    {
        try
        {
            if (item is null)
                return Failure<bool>("El objeto EnrollmentStatusRemove es requerido.");
            if (item.EnrollmentStatusId <= 0)
                return Failure<bool>("El identificador debe ser mayor que cero.");

            await using var transaction = await context.Database.BeginTransactionAsync(IsolationLevel.Serializable);
            var entity = await context.EnrollmentStatuses.FirstOrDefaultAsync(e => e.EnrollmentStatusId == item.EnrollmentStatusId && !e.Deleted);
            if (entity is null)
                return Failure<bool>("El registro no existe o fue eliminado.");

            if (await context.CourseEnrollments.AnyAsync(e => e.EnrollmentStatusId == item.EnrollmentStatusId && !e.Deleted))
                return Failure<bool>("No se puede eliminar un estado utilizado por inscripciones.");
            entity.Deleted = true;
            entity.DeletedDate = item.DeletedDate;
            entity.UserDeleted = item.UserDeleted;
            await context.SaveChangesAsync();
            await transaction.CommitAsync();
            return new OperationResult<bool> { Data = true, Message = "Registro eliminado correctamente." };
        }
        catch (Exception)
        {
            return Failure<bool>("No fue posible eliminar el registro de estado de inscripción.");
        }
    }

    public async Task<OperationResult<List<EnrollmentStatus>>> GetAllAsync()
    {
        try
        {
            var data = await context.EnrollmentStatuses.AsNoTracking().Where(e => !e.Deleted).ToListAsync();
            return new OperationResult<List<EnrollmentStatus>> { Data = data };
        }
        catch (Exception)
        {
            return Failure<List<EnrollmentStatus>>("No fue posible consultar los registros de estados de inscripción.");
        }
    }

    public async Task<OperationResult<EnrollmentStatusModel>?> GetByIdAsync(int id)
    {
        try
        {
            if (id <= 0)
                return Failure<EnrollmentStatusModel>("El identificador debe ser mayor que cero.");

            var data = await context.EnrollmentStatuses.AsNoTracking()
                .Where(e => e.EnrollmentStatusId == id && !e.Deleted)
                .Select(e => new EnrollmentStatusModel
                {
                    EnrollmentStatusId = e.EnrollmentStatusId,
                    Code = e.Code,
                    Name = e.Name,
                    Description = e.Description,
                    IsActive = e.IsActive
                }).FirstOrDefaultAsync();
            if (data is null)
                return Failure<EnrollmentStatusModel>("El registro no existe o fue eliminado.");

            return new OperationResult<EnrollmentStatusModel> { Data = data };
        }
        catch (Exception)
        {
            return Failure<EnrollmentStatusModel>("No fue posible consultar el registro de estado de inscripción.");
        }
    }

    private static string? ValidateEnrollmentStatus(EnrollmentStatusBase item)
    {
        if (string.IsNullOrWhiteSpace(item.Code))
            return "El código del estado es requerido.";
        if (string.IsNullOrWhiteSpace(item.Name))
            return "El nombre del estado es requerido.";
        return null;
    }

    private async Task<string?> ValidateRelationsAsync(EnrollmentStatusBase item, int? exceptId = null)
    {
        var code = item.Code!.Trim().ToUpperInvariant();
        if (await context.EnrollmentStatuses.AnyAsync(s => !s.Deleted && s.Code.ToUpper() == code
            && (!exceptId.HasValue || s.EnrollmentStatusId != exceptId.Value)))
            return "El código del estado ya está registrado.";
        return null;
    }

    private static OperationResult<T> Failure<T>(string message) => new()
    {
        Success = false,
        Message = message
    };
}
