using System.Diagnostics;
using Microsoft.Extensions.Logging;
using SchoolApp.DAL.Models.Objects.OnsiteCourse;
using SchoolApp.DAL.Models.Result;
using SchoolApp.DAL.Repositories;

namespace SchoolApp.BL.Services;

public class OnsiteCourseServices : IOnsiteCourseServices
{
    private readonly IOnsiteCourseRepository repository;
    private readonly ILogger<OnsiteCourseServices> logger;

    public OnsiteCourseServices(IOnsiteCourseRepository repository, ILogger<OnsiteCourseServices> logger)
    {
        this.repository = repository ?? throw new ArgumentNullException(nameof(repository));
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<OperationResult<bool>> AddAsync(OnsiteCourseAdd? item)
    {
        OperationResult<bool> result = new OperationResult<bool>();
        string correlationId = Activity.Current?.TraceId.ToString() ?? Guid.NewGuid().ToString("N");
        try
        {
            this.logger.LogInformation("Iniciando operación de registrar curso presencial.");
            if (item is null)
                return result = Failure<bool>("El objeto OnsiteCourseAdd es requerido.");

            var validation = ValidateOnsiteCourse(item);
            if (validation is not null)
                return result = Failure<bool>(validation);
            if (item.CreationUser <= 0)
                return result = Failure<bool>("El usuario de creación debe ser mayor que cero.");

            var repositoryResult = await this.repository.AddAsync(new OnsiteCourseAdd
            {
                CourseId = item.CourseId,
                Location = item.Location!.Trim(),
                Days = item.Days!.Trim(),
                Time = item.Time,
                CreationDate = DateTime.UtcNow,
                CreationUser = item.CreationUser
            });
            result = repositoryResult.Success && repositoryResult.Data
                ? new OperationResult<bool> { Data = true, Message = "Operación completada correctamente." }
                : Failure<bool>(repositoryResult.Message ?? "No fue posible registrar el registro de curso presencial.");
        }
        catch (Exception ex)
        {
            this.logger.LogError("Error en OnsiteCourse.Add. Tipo: {ExceptionType}. Correlación: {CorrelationId}.", ex.GetType().Name, correlationId);
            result = Failure<bool>("Ocurrió un error inesperado al registrar el registro de curso presencial.");
        }
        finally
        {
            this.logger.LogInformation("Operación: OnsiteCourse.Add. Usuario: {UserId}. Fecha UTC: {TimestampUtc}. Éxito: {Success}. Correlación: {CorrelationId}.",
                item?.CreationUser, DateTime.UtcNow, result.Success, correlationId);
        }
        return result;
    }

    public async Task<OperationResult<bool>> UpdateAsync(OnsiteCourseUpdate? item)
    {
        OperationResult<bool> result = new OperationResult<bool>();
        string correlationId = Activity.Current?.TraceId.ToString() ?? Guid.NewGuid().ToString("N");
        try
        {
            this.logger.LogInformation("Iniciando operación de actualizar curso presencial.");
            if (item is null)
                return result = Failure<bool>("El objeto OnsiteCourseUpdate es requerido.");

            var validation = ValidateOnsiteCourse(item);
            if (validation is not null)
                return result = Failure<bool>(validation);
            if (!item.ModifyUser.HasValue || item.ModifyUser.Value <= 0)
                return result = Failure<bool>("El usuario de modificación debe ser mayor que cero.");

            var repositoryResult = await this.repository.UpdateAsync(new OnsiteCourseUpdate
            {
                CourseId = item.CourseId,
                Location = item.Location!.Trim(),
                Days = item.Days!.Trim(),
                Time = item.Time,
                ModifyDate = DateTime.UtcNow,
                ModifyUser = item.ModifyUser
            });
            result = repositoryResult.Success && repositoryResult.Data
                ? new OperationResult<bool> { Data = true, Message = "Operación completada correctamente." }
                : Failure<bool>(repositoryResult.Message ?? "No fue posible actualizar el registro de curso presencial.");
        }
        catch (Exception ex)
        {
            this.logger.LogError("Error en OnsiteCourse.Update. Tipo: {ExceptionType}. Correlación: {CorrelationId}.", ex.GetType().Name, correlationId);
            result = Failure<bool>("Ocurrió un error inesperado al actualizar el registro de curso presencial.");
        }
        finally
        {
            this.logger.LogInformation("Operación: OnsiteCourse.Update. Usuario: {UserId}. Fecha UTC: {TimestampUtc}. Éxito: {Success}. Correlación: {CorrelationId}.",
                item?.ModifyUser, DateTime.UtcNow, result.Success, correlationId);
        }
        return result;
    }

    public async Task<OperationResult<List<OnsiteCourseModel>>> GetAllAsync()
    {
        OperationResult<List<OnsiteCourseModel>> result = new OperationResult<List<OnsiteCourseModel>>();
        try
        {
            this.logger.LogInformation("Iniciando consulta de cursos presenciales.");
            var repositoryResult = await this.repository.GetAllAsync();
            if (!repositoryResult.Success || repositoryResult.Data is null)
                return Failure<List<OnsiteCourseModel>>("No fue posible consultar los registros de cursos presenciales.");

            result.Data = repositoryResult.Data.Where(e => !e.Deleted).Select(e => new OnsiteCourseModel
            {
                CourseId = e.CourseId,
                Location = e.Location,
                Days = e.Days,
                Time = e.Time
            }).ToList();
            this.logger.LogInformation("Consulta de cursos presenciales completada. Total: {Count}.", result.Data.Count);
        }
        catch (Exception ex)
        {
            this.logger.LogError("Error al consultar cursos presenciales. Tipo: {ExceptionType}.", ex.GetType().Name);
            result = Failure<List<OnsiteCourseModel>>("Ocurrió un error inesperado al consultar los registros de cursos presenciales.");
        }
        return result;
    }

    public async Task<OperationResult<OnsiteCourseModel>> GetByIdAsync(int id)
    {
        OperationResult<OnsiteCourseModel> result = new OperationResult<OnsiteCourseModel>();
        try
        {
            this.logger.LogInformation("Iniciando consulta de curso presencial. Identificador: {Id}.", id);
            if (id <= 0)
                return Failure<OnsiteCourseModel>("El identificador debe ser mayor que cero.");

            var repositoryResult = await this.repository.GetByIdAsync(id);
            if (repositoryResult is null || !repositoryResult.Success || repositoryResult.Data is null)
                return Failure<OnsiteCourseModel>("No fue posible consultar el registro. Verifique que exista y no esté eliminado.");

            result.Data = repositoryResult.Data;
            this.logger.LogInformation("Consulta de curso presencial completada. Identificador: {Id}.", id);
        }
        catch (Exception ex)
        {
            this.logger.LogError("Error al consultar curso presencial. Tipo: {ExceptionType}.", ex.GetType().Name);
            result = Failure<OnsiteCourseModel>("Ocurrió un error inesperado al consultar el registro de curso presencial.");
        }
        return result;
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

    private OperationResult<T> Failure<T>(string message)
    {
        this.logger.LogWarning("Validación u operación fallida: {ValidationMessage}. Éxito: False.", message);
        return new OperationResult<T> { Success = false, Message = message };
    }
}
