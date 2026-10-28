using System.Diagnostics;
using Microsoft.Extensions.Logging;
using SchoolApp.DAL.Models.Objects.EnrollmentStatus;
using SchoolApp.DAL.Models.Result;
using SchoolApp.DAL.Repositories;

namespace SchoolApp.BL.Services;

public class EnrollmentStatusServices : IEnrollmentStatusServices
{
    private readonly IEnrollmentStatusRepository repository;
    private readonly ILogger<EnrollmentStatusServices> logger;

    public EnrollmentStatusServices(IEnrollmentStatusRepository repository, ILogger<EnrollmentStatusServices> logger)
    {
        this.repository = repository ?? throw new ArgumentNullException(nameof(repository));
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<OperationResult<bool>> AddAsync(EnrollmentStatusAdd? item)
    {
        OperationResult<bool> result = new OperationResult<bool>();
        string correlationId = Activity.Current?.TraceId.ToString() ?? Guid.NewGuid().ToString("N");
        try
        {
            this.logger.LogInformation("Iniciando operación de registrar estado de inscripción.");
            if (item is null)
                return result = Failure<bool>("El objeto EnrollmentStatusAdd es requerido.");

            var validation = ValidateEnrollmentStatus(item);
            if (validation is not null)
                return result = Failure<bool>(validation);
            if (item.CreationUser <= 0)
                return result = Failure<bool>("El usuario de creación debe ser mayor que cero.");

            var repositoryResult = await this.repository.AddAsync(new EnrollmentStatusAdd
            {
                Code = item.Code!.Trim().ToUpperInvariant(),
                Name = item.Name!.Trim(),
                Description = item.Description?.Trim() ?? string.Empty,
                IsActive = item.IsActive,
                CreationDate = DateTime.UtcNow,
                CreationUser = item.CreationUser
            });
            result = repositoryResult.Success && repositoryResult.Data
                ? new OperationResult<bool> { Data = true, Message = "Operación completada correctamente." }
                : Failure<bool>(repositoryResult.Message ?? "No fue posible registrar el registro de estado de inscripción.");
        }
        catch (Exception ex)
        {
            this.logger.LogError("Error en EnrollmentStatus.Add. Tipo: {ExceptionType}. Correlación: {CorrelationId}.", ex.GetType().Name, correlationId);
            result = Failure<bool>("Ocurrió un error inesperado al registrar el registro de estado de inscripción.");
        }
        finally
        {
            this.logger.LogInformation("Operación: EnrollmentStatus.Add. Usuario: {UserId}. Fecha UTC: {TimestampUtc}. Éxito: {Success}. Correlación: {CorrelationId}.",
                item?.CreationUser, DateTime.UtcNow, result.Success, correlationId);
        }
        return result;
    }

    public async Task<OperationResult<bool>> UpdateAsync(EnrollmentStatusUpdate? item)
    {
        OperationResult<bool> result = new OperationResult<bool>();
        string correlationId = Activity.Current?.TraceId.ToString() ?? Guid.NewGuid().ToString("N");
        try
        {
            this.logger.LogInformation("Iniciando operación de actualizar estado de inscripción.");
            if (item is null)
                return result = Failure<bool>("El objeto EnrollmentStatusUpdate es requerido.");
            if (item.EnrollmentStatusId <= 0)
                return result = Failure<bool>("El identificador debe ser mayor que cero.");

            var validation = ValidateEnrollmentStatus(item);
            if (validation is not null)
                return result = Failure<bool>(validation);
            if (!item.ModifyUser.HasValue || item.ModifyUser.Value <= 0)
                return result = Failure<bool>("El usuario de modificación debe ser mayor que cero.");

            var repositoryResult = await this.repository.UpdateAsync(new EnrollmentStatusUpdate
            {
                EnrollmentStatusId = item.EnrollmentStatusId,
                Code = item.Code!.Trim().ToUpperInvariant(),
                Name = item.Name!.Trim(),
                Description = item.Description?.Trim() ?? string.Empty,
                IsActive = item.IsActive,
                ModifyDate = DateTime.UtcNow,
                ModifyUser = item.ModifyUser
            });
            result = repositoryResult.Success && repositoryResult.Data
                ? new OperationResult<bool> { Data = true, Message = "Operación completada correctamente." }
                : Failure<bool>(repositoryResult.Message ?? "No fue posible actualizar el registro de estado de inscripción.");
        }
        catch (Exception ex)
        {
            this.logger.LogError("Error en EnrollmentStatus.Update. Tipo: {ExceptionType}. Correlación: {CorrelationId}.", ex.GetType().Name, correlationId);
            result = Failure<bool>("Ocurrió un error inesperado al actualizar el registro de estado de inscripción.");
        }
        finally
        {
            this.logger.LogInformation("Operación: EnrollmentStatus.Update. Usuario: {UserId}. Fecha UTC: {TimestampUtc}. Éxito: {Success}. Correlación: {CorrelationId}.",
                item?.ModifyUser, DateTime.UtcNow, result.Success, correlationId);
        }
        return result;
    }

    public async Task<OperationResult<List<EnrollmentStatusModel>>> GetAllAsync()
    {
        OperationResult<List<EnrollmentStatusModel>> result = new OperationResult<List<EnrollmentStatusModel>>();
        try
        {
            this.logger.LogInformation("Iniciando consulta de estados de inscripción.");
            var repositoryResult = await this.repository.GetAllAsync();
            if (!repositoryResult.Success || repositoryResult.Data is null)
                return Failure<List<EnrollmentStatusModel>>("No fue posible consultar los registros de estados de inscripción.");

            result.Data = repositoryResult.Data.Where(e => !e.Deleted).Select(e => new EnrollmentStatusModel
            {
                EnrollmentStatusId = e.EnrollmentStatusId,
                Code = e.Code,
                Name = e.Name,
                Description = e.Description,
                IsActive = e.IsActive
            }).ToList();
            this.logger.LogInformation("Consulta de estados de inscripción completada. Total: {Count}.", result.Data.Count);
        }
        catch (Exception ex)
        {
            this.logger.LogError("Error al consultar estados de inscripción. Tipo: {ExceptionType}.", ex.GetType().Name);
            result = Failure<List<EnrollmentStatusModel>>("Ocurrió un error inesperado al consultar los registros de estados de inscripción.");
        }
        return result;
    }

    public async Task<OperationResult<EnrollmentStatusModel>> GetByIdAsync(int id)
    {
        OperationResult<EnrollmentStatusModel> result = new OperationResult<EnrollmentStatusModel>();
        try
        {
            this.logger.LogInformation("Iniciando consulta de estado de inscripción. Identificador: {Id}.", id);
            if (id <= 0)
                return Failure<EnrollmentStatusModel>("El identificador debe ser mayor que cero.");

            var repositoryResult = await this.repository.GetByIdAsync(id);
            if (repositoryResult is null || !repositoryResult.Success || repositoryResult.Data is null)
                return Failure<EnrollmentStatusModel>("No fue posible consultar el registro. Verifique que exista y no esté eliminado.");

            result.Data = repositoryResult.Data;
            this.logger.LogInformation("Consulta de estado de inscripción completada. Identificador: {Id}.", id);
        }
        catch (Exception ex)
        {
            this.logger.LogError("Error al consultar estado de inscripción. Tipo: {ExceptionType}.", ex.GetType().Name);
            result = Failure<EnrollmentStatusModel>("Ocurrió un error inesperado al consultar el registro de estado de inscripción.");
        }
        return result;
    }

    private static string? ValidateEnrollmentStatus(EnrollmentStatusBase item)
    {
        if (string.IsNullOrWhiteSpace(item.Code))
            return "El código del estado es requerido.";
        if (string.IsNullOrWhiteSpace(item.Name))
            return "El nombre del estado es requerido.";
        return null;
    }

    private OperationResult<T> Failure<T>(string message)
    {
        this.logger.LogWarning("Validación u operación fallida: {ValidationMessage}. Éxito: False.", message);
        return new OperationResult<T> { Success = false, Message = message };
    }
}
