using System.Diagnostics;
using Microsoft.Extensions.Logging;
using SchoolApp.DAL.Models.Objects.OnlineCourse;
using SchoolApp.DAL.Models.Result;
using SchoolApp.DAL.Repositories;

namespace SchoolApp.BL.Services;

public class OnlineCourseServices : IOnlineCourseServices
{
    private readonly IOnlineCourseRepository repository;
    private readonly ILogger<OnlineCourseServices> logger;

    public OnlineCourseServices(IOnlineCourseRepository repository, ILogger<OnlineCourseServices> logger)
    {
        this.repository = repository ?? throw new ArgumentNullException(nameof(repository));
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<OperationResult<bool>> AddAsync(OnlineCourseAdd? item)
    {
        OperationResult<bool> result = new OperationResult<bool>();
        string correlationId = Activity.Current?.TraceId.ToString() ?? Guid.NewGuid().ToString("N");
        try
        {
            this.logger.LogInformation("Iniciando operación de registrar curso en línea.");
            if (item is null)
                return result = Failure<bool>("El objeto OnlineCourseAdd es requerido.");

            var validation = ValidateOnlineCourse(item);
            if (validation is not null)
                return result = Failure<bool>(validation);
            if (item.CreationUser <= 0)
                return result = Failure<bool>("El usuario de creación debe ser mayor que cero.");

            var repositoryResult = await this.repository.AddAsync(new OnlineCourseAdd
            {
                CourseId = item.CourseId,
                Url = item.Url!.Trim(),
                CreationDate = DateTime.UtcNow,
                CreationUser = item.CreationUser
            });
            result = repositoryResult.Success && repositoryResult.Data
                ? new OperationResult<bool> { Data = true, Message = "Operación completada correctamente." }
                : Failure<bool>(repositoryResult.Message ?? "No fue posible registrar el registro de curso en línea.");
        }
        catch (Exception ex)
        {
            this.logger.LogError("Error en OnlineCourse.Add. Tipo: {ExceptionType}. Correlación: {CorrelationId}.", ex.GetType().Name, correlationId);
            result = Failure<bool>("Ocurrió un error inesperado al registrar el registro de curso en línea.");
        }
        finally
        {
            this.logger.LogInformation("Operación: OnlineCourse.Add. Usuario: {UserId}. Fecha UTC: {TimestampUtc}. Éxito: {Success}. Correlación: {CorrelationId}.",
                item?.CreationUser, DateTime.UtcNow, result.Success, correlationId);
        }
        return result;
    }

    public async Task<OperationResult<bool>> UpdateAsync(OnlineCourseUpdate? item)
    {
        OperationResult<bool> result = new OperationResult<bool>();
        string correlationId = Activity.Current?.TraceId.ToString() ?? Guid.NewGuid().ToString("N");
        try
        {
            this.logger.LogInformation("Iniciando operación de actualizar curso en línea.");
            if (item is null)
                return result = Failure<bool>("El objeto OnlineCourseUpdate es requerido.");

            var validation = ValidateOnlineCourse(item);
            if (validation is not null)
                return result = Failure<bool>(validation);
            if (!item.ModifyUser.HasValue || item.ModifyUser.Value <= 0)
                return result = Failure<bool>("El usuario de modificación debe ser mayor que cero.");

            var repositoryResult = await this.repository.UpdateAsync(new OnlineCourseUpdate
            {
                CourseId = item.CourseId,
                Url = item.Url!.Trim(),
                ModifyDate = DateTime.UtcNow,
                ModifyUser = item.ModifyUser
            });
            result = repositoryResult.Success && repositoryResult.Data
                ? new OperationResult<bool> { Data = true, Message = "Operación completada correctamente." }
                : Failure<bool>(repositoryResult.Message ?? "No fue posible actualizar el registro de curso en línea.");
        }
        catch (Exception ex)
        {
            this.logger.LogError("Error en OnlineCourse.Update. Tipo: {ExceptionType}. Correlación: {CorrelationId}.", ex.GetType().Name, correlationId);
            result = Failure<bool>("Ocurrió un error inesperado al actualizar el registro de curso en línea.");
        }
        finally
        {
            this.logger.LogInformation("Operación: OnlineCourse.Update. Usuario: {UserId}. Fecha UTC: {TimestampUtc}. Éxito: {Success}. Correlación: {CorrelationId}.",
                item?.ModifyUser, DateTime.UtcNow, result.Success, correlationId);
        }
        return result;
    }

    public async Task<OperationResult<List<OnlineCourseModel>>> GetAllAsync()
    {
        OperationResult<List<OnlineCourseModel>> result = new OperationResult<List<OnlineCourseModel>>();
        try
        {
            this.logger.LogInformation("Iniciando consulta de cursos en línea.");
            var repositoryResult = await this.repository.GetAllAsync();
            if (!repositoryResult.Success || repositoryResult.Data is null)
                return Failure<List<OnlineCourseModel>>("No fue posible consultar los registros de cursos en línea.");

            result.Data = repositoryResult.Data.Where(e => !e.Deleted).Select(e => new OnlineCourseModel
            {
                CourseId = e.CourseId,
                Url = e.Url
            }).ToList();
            this.logger.LogInformation("Consulta de cursos en línea completada. Total: {Count}.", result.Data.Count);
        }
        catch (Exception ex)
        {
            this.logger.LogError("Error al consultar cursos en línea. Tipo: {ExceptionType}.", ex.GetType().Name);
            result = Failure<List<OnlineCourseModel>>("Ocurrió un error inesperado al consultar los registros de cursos en línea.");
        }
        return result;
    }

    public async Task<OperationResult<OnlineCourseModel>> GetByIdAsync(int id)
    {
        OperationResult<OnlineCourseModel> result = new OperationResult<OnlineCourseModel>();
        try
        {
            this.logger.LogInformation("Iniciando consulta de curso en línea. Identificador: {Id}.", id);
            if (id <= 0)
                return Failure<OnlineCourseModel>("El identificador debe ser mayor que cero.");

            var repositoryResult = await this.repository.GetByIdAsync(id);
            if (repositoryResult is null || !repositoryResult.Success || repositoryResult.Data is null)
                return Failure<OnlineCourseModel>("No fue posible consultar el registro. Verifique que exista y no esté eliminado.");

            result.Data = repositoryResult.Data;
            this.logger.LogInformation("Consulta de curso en línea completada. Identificador: {Id}.", id);
        }
        catch (Exception ex)
        {
            this.logger.LogError("Error al consultar curso en línea. Tipo: {ExceptionType}.", ex.GetType().Name);
            result = Failure<OnlineCourseModel>("Ocurrió un error inesperado al consultar el registro de curso en línea.");
        }
        return result;
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

    private OperationResult<T> Failure<T>(string message)
    {
        this.logger.LogWarning("Validación u operación fallida: {ValidationMessage}. Éxito: False.", message);
        return new OperationResult<T> { Success = false, Message = message };
    }
}
