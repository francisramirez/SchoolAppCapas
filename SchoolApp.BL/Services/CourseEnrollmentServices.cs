using System.Diagnostics;
using Microsoft.Extensions.Logging;
using SchoolApp.DAL.Models.Objects.CourseEnrollment;
using SchoolApp.DAL.Models.Result;
using SchoolApp.DAL.Repositories;

namespace SchoolApp.BL.Services;

public class CourseEnrollmentServices : ICourseEnrollmentServices
{
    private readonly ICourseEnrollmentRepository repository;
    private readonly ILogger<CourseEnrollmentServices> logger;

    public CourseEnrollmentServices(ICourseEnrollmentRepository repository, ILogger<CourseEnrollmentServices> logger)
    {
        this.repository = repository ?? throw new ArgumentNullException(nameof(repository));
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<OperationResult<bool>> AddAsync(CourseEnrollmentAdd? item)
    {
        OperationResult<bool> result = new OperationResult<bool>();
        string correlationId = Activity.Current?.TraceId.ToString() ?? Guid.NewGuid().ToString("N");
        try
        {
            this.logger.LogInformation("Iniciando operación de registrar inscripción.");
            if (item is null)
                return result = Failure<bool>("El objeto CourseEnrollmentAdd es requerido.");

            var validation = ValidateCourseEnrollment(item);
            if (validation is not null)
                return result = Failure<bool>(validation);
            if (item.CreationUser <= 0)
                return result = Failure<bool>("El usuario de creación debe ser mayor que cero.");

            var repositoryResult = await this.repository.AddAsync(new CourseEnrollmentAdd
            {
                CourseId = item.CourseId,
                StudentId = item.StudentId,
                EnrollmentDate = item.EnrollmentDate,
                EnrollmentStatusId = item.EnrollmentStatusId,
                CreationDate = DateTime.UtcNow,
                CreationUser = item.CreationUser
            });
            result = repositoryResult.Success && repositoryResult.Data
                ? new OperationResult<bool> { Data = true, Message = "Operación completada correctamente." }
                : Failure<bool>(repositoryResult.Message ?? "No fue posible registrar el registro de inscripción.");
        }
        catch (Exception ex)
        {
            this.logger.LogError("Error en CourseEnrollment.Add. Tipo: {ExceptionType}. Correlación: {CorrelationId}.", ex.GetType().Name, correlationId);
            result = Failure<bool>("Ocurrió un error inesperado al registrar el registro de inscripción.");
        }
        finally
        {
            this.logger.LogInformation("Operación: CourseEnrollment.Add. Usuario: {UserId}. Fecha UTC: {TimestampUtc}. Éxito: {Success}. Correlación: {CorrelationId}.",
                item?.CreationUser, DateTime.UtcNow, result.Success, correlationId);
        }
        return result;
    }

    public async Task<OperationResult<bool>> UpdateAsync(CourseEnrollmentUpdate? item)
    {
        OperationResult<bool> result = new OperationResult<bool>();
        string correlationId = Activity.Current?.TraceId.ToString() ?? Guid.NewGuid().ToString("N");
        try
        {
            this.logger.LogInformation("Iniciando operación de actualizar inscripción.");
            if (item is null)
                return result = Failure<bool>("El objeto CourseEnrollmentUpdate es requerido.");
            if (item.EnrollmentId <= 0)
                return result = Failure<bool>("El identificador debe ser mayor que cero.");

            var validation = ValidateCourseEnrollment(item);
            if (validation is not null)
                return result = Failure<bool>(validation);
            if (!item.ModifyUser.HasValue || item.ModifyUser.Value <= 0)
                return result = Failure<bool>("El usuario de modificación debe ser mayor que cero.");

            var repositoryResult = await this.repository.UpdateAsync(new CourseEnrollmentUpdate
            {
                EnrollmentId = item.EnrollmentId,
                CourseId = item.CourseId,
                StudentId = item.StudentId,
                EnrollmentDate = item.EnrollmentDate,
                EnrollmentStatusId = item.EnrollmentStatusId,
                ModifyDate = DateTime.UtcNow,
                ModifyUser = item.ModifyUser
            });
            result = repositoryResult.Success && repositoryResult.Data
                ? new OperationResult<bool> { Data = true, Message = "Operación completada correctamente." }
                : Failure<bool>(repositoryResult.Message ?? "No fue posible actualizar el registro de inscripción.");
        }
        catch (Exception ex)
        {
            this.logger.LogError("Error en CourseEnrollment.Update. Tipo: {ExceptionType}. Correlación: {CorrelationId}.", ex.GetType().Name, correlationId);
            result = Failure<bool>("Ocurrió un error inesperado al actualizar el registro de inscripción.");
        }
        finally
        {
            this.logger.LogInformation("Operación: CourseEnrollment.Update. Usuario: {UserId}. Fecha UTC: {TimestampUtc}. Éxito: {Success}. Correlación: {CorrelationId}.",
                item?.ModifyUser, DateTime.UtcNow, result.Success, correlationId);
        }
        return result;
    }

    public async Task<OperationResult<List<CourseEnrollmentModel>>> GetAllAsync()
    {
        OperationResult<List<CourseEnrollmentModel>> result = new OperationResult<List<CourseEnrollmentModel>>();
        try
        {
            this.logger.LogInformation("Iniciando consulta de inscripciones.");
            var repositoryResult = await this.repository.GetAllAsync();
            if (!repositoryResult.Success || repositoryResult.Data is null)
                return Failure<List<CourseEnrollmentModel>>("No fue posible consultar los registros de inscripciones.");

            result.Data = repositoryResult.Data.Where(e => !e.Deleted).Select(e => new CourseEnrollmentModel
            {
                EnrollmentId = e.EnrollmentId,
                CourseId = e.CourseId,
                StudentId = e.StudentId,
                EnrollmentDate = e.EnrollmentDate,
                EnrollmentStatusId = e.EnrollmentStatusId
            }).ToList();
            this.logger.LogInformation("Consulta de inscripciones completada. Total: {Count}.", result.Data.Count);
        }
        catch (Exception ex)
        {
            this.logger.LogError("Error al consultar inscripciones. Tipo: {ExceptionType}.", ex.GetType().Name);
            result = Failure<List<CourseEnrollmentModel>>("Ocurrió un error inesperado al consultar los registros de inscripciones.");
        }
        return result;
    }

    public async Task<OperationResult<List<CourseEnrollmentModel>>> GetByStudentIdAsync(int studentId)
    {
        OperationResult<List<CourseEnrollmentModel>> result = new OperationResult<List<CourseEnrollmentModel>>();
        try
        {
            this.logger.LogInformation("Iniciando consulta de inscripciones.");
            if (studentId <= 0)
                return Failure<List<CourseEnrollmentModel>>("El identificador del estudiante debe ser mayor que cero.");
            var repositoryResult = await this.repository.GetByStudentIdAsync(studentId);
            if (!repositoryResult.Success || repositoryResult.Data is null)
                return Failure<List<CourseEnrollmentModel>>("No fue posible consultar los registros de inscripciones.");

            result.Data = repositoryResult.Data.Where(e => !e.Deleted && e.StudentId == studentId).Select(e => new CourseEnrollmentModel
            {
                EnrollmentId = e.EnrollmentId,
                CourseId = e.CourseId,
                StudentId = e.StudentId,
                EnrollmentDate = e.EnrollmentDate,
                EnrollmentStatusId = e.EnrollmentStatusId
            }).ToList();
            this.logger.LogInformation("Consulta de inscripciones completada. Total: {Count}.", result.Data.Count);
        }
        catch (Exception ex)
        {
            this.logger.LogError("Error al consultar inscripciones. Tipo: {ExceptionType}.", ex.GetType().Name);
            result = Failure<List<CourseEnrollmentModel>>("Ocurrió un error inesperado al consultar los registros de inscripciones.");
        }
        return result;
    }

    public async Task<OperationResult<CourseEnrollmentModel>> GetByIdAsync(int id)
    {
        OperationResult<CourseEnrollmentModel> result = new OperationResult<CourseEnrollmentModel>();
        try
        {
            this.logger.LogInformation("Iniciando consulta de inscripción. Identificador: {Id}.", id);
            if (id <= 0)
                return Failure<CourseEnrollmentModel>("El identificador debe ser mayor que cero.");

            var repositoryResult = await this.repository.GetByIdAsync(id);
            if (repositoryResult is null || !repositoryResult.Success || repositoryResult.Data is null)
                return Failure<CourseEnrollmentModel>("No fue posible consultar el registro. Verifique que exista y no esté eliminado.");

            result.Data = repositoryResult.Data;
            this.logger.LogInformation("Consulta de inscripción completada. Identificador: {Id}.", id);
        }
        catch (Exception ex)
        {
            this.logger.LogError("Error al consultar inscripción. Tipo: {ExceptionType}.", ex.GetType().Name);
            result = Failure<CourseEnrollmentModel>("Ocurrió un error inesperado al consultar el registro de inscripción.");
        }
        return result;
    }

    private static string? ValidateCourseEnrollment(CourseEnrollmentBase item)
    {
        if (item.CourseId <= 0 || item.StudentId <= 0 || item.EnrollmentStatusId <= 0)
            return "Los identificadores de curso, estudiante y estado deben ser mayores que cero.";
        if (item.EnrollmentDate == default)
            return "La fecha de inscripción es requerida.";
        return null;
    }

    private OperationResult<T> Failure<T>(string message)
    {
        this.logger.LogWarning("Validación u operación fallida: {ValidationMessage}. Éxito: False.", message);
        return new OperationResult<T> { Success = false, Message = message };
    }
}
