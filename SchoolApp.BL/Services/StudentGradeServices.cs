using System.Diagnostics;
using Microsoft.Extensions.Logging;
using SchoolApp.DAL.Models.Objects.StudentGrade;
using SchoolApp.DAL.Models.Result;
using SchoolApp.DAL.Repositories;
using Microsoft.Extensions.Options;
using SchoolApp.BL.Configuration;

namespace SchoolApp.BL.Services;

public class StudentGradeServices : IStudentGradeServices
{
    private readonly IStudentGradeRepository repository;
    private readonly ILogger<StudentGradeServices> logger;
    private readonly GradeScaleOptions gradeScale;

    public StudentGradeServices(IStudentGradeRepository repository, ILogger<StudentGradeServices> logger, IOptions<GradeScaleOptions> gradeScale)
    {
        this.repository = repository ?? throw new ArgumentNullException(nameof(repository));
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        this.gradeScale = gradeScale?.Value ?? throw new ArgumentNullException(nameof(gradeScale));
    }

    public async Task<OperationResult<bool>> AddAsync(StudentGradeAdd? item)
    {
        OperationResult<bool> result = new OperationResult<bool>();
        string correlationId = Activity.Current?.TraceId.ToString() ?? Guid.NewGuid().ToString("N");
        try
        {
            this.logger.LogInformation("Iniciando operación de registrar calificación.");
            if (item is null)
                return result = Failure<bool>("El objeto StudentGradeAdd es requerido.");

            var validation = ValidateStudentGrade(item);
            if (validation is not null)
                return result = Failure<bool>(validation);
            if (item.CreationUser <= 0)
                return result = Failure<bool>("El usuario de creación debe ser mayor que cero.");

            var repositoryResult = await this.repository.AddAsync(new StudentGradeAdd
            {
                EnrollmentId = item.EnrollmentId,
                CourseId = item.CourseId,
                StudentId = item.StudentId,
                Grade = item.Grade,
                CreationDate = DateTime.UtcNow,
                CreationUser = item.CreationUser
            });
            result = repositoryResult.Success && repositoryResult.Data
                ? new OperationResult<bool> { Data = true, Message = "Operación completada correctamente." }
                : Failure<bool>(repositoryResult.Message ?? "No fue posible registrar el registro de calificación.");
        }
        catch (Exception ex)
        {
            this.logger.LogError("Error en StudentGrade.Add. Tipo: {ExceptionType}. Correlación: {CorrelationId}.", ex.GetType().Name, correlationId);
            result = Failure<bool>("Ocurrió un error inesperado al registrar el registro de calificación.");
        }
        finally
        {
            this.logger.LogInformation("Operación: StudentGrade.Add. Usuario: {UserId}. Fecha UTC: {TimestampUtc}. Éxito: {Success}. Correlación: {CorrelationId}.",
                item?.CreationUser, DateTime.UtcNow, result.Success, correlationId);
        }
        return result;
    }

    public async Task<OperationResult<bool>> UpdateAsync(StudentGradeUpdate? item)
    {
        OperationResult<bool> result = new OperationResult<bool>();
        string correlationId = Activity.Current?.TraceId.ToString() ?? Guid.NewGuid().ToString("N");
        try
        {
            this.logger.LogInformation("Iniciando operación de actualizar calificación.");
            if (item is null)
                return result = Failure<bool>("El objeto StudentGradeUpdate es requerido.");

            var validation = ValidateStudentGrade(item);
            if (validation is not null)
                return result = Failure<bool>(validation);
            if (!item.ModifyUser.HasValue || item.ModifyUser.Value <= 0)
                return result = Failure<bool>("El usuario de modificación debe ser mayor que cero.");

            var repositoryResult = await this.repository.UpdateAsync(new StudentGradeUpdate
            {
                EnrollmentId = item.EnrollmentId,
                CourseId = item.CourseId,
                StudentId = item.StudentId,
                Grade = item.Grade,
                ModifyDate = DateTime.UtcNow,
                ModifyUser = item.ModifyUser
            });
            result = repositoryResult.Success && repositoryResult.Data
                ? new OperationResult<bool> { Data = true, Message = "Operación completada correctamente." }
                : Failure<bool>(repositoryResult.Message ?? "No fue posible actualizar el registro de calificación.");
        }
        catch (Exception ex)
        {
            this.logger.LogError("Error en StudentGrade.Update. Tipo: {ExceptionType}. Correlación: {CorrelationId}.", ex.GetType().Name, correlationId);
            result = Failure<bool>("Ocurrió un error inesperado al actualizar el registro de calificación.");
        }
        finally
        {
            this.logger.LogInformation("Operación: StudentGrade.Update. Usuario: {UserId}. Fecha UTC: {TimestampUtc}. Éxito: {Success}. Correlación: {CorrelationId}.",
                item?.ModifyUser, DateTime.UtcNow, result.Success, correlationId);
        }
        return result;
    }

    public async Task<OperationResult<List<StudentGradeModel>>> GetAllAsync()
    {
        OperationResult<List<StudentGradeModel>> result = new OperationResult<List<StudentGradeModel>>();
        try
        {
            this.logger.LogInformation("Iniciando consulta de calificaciones.");
            var repositoryResult = await this.repository.GetAllAsync();
            if (!repositoryResult.Success || repositoryResult.Data is null)
                return Failure<List<StudentGradeModel>>("No fue posible consultar los registros de calificaciones.");

            result.Data = repositoryResult.Data.Where(e => !e.Deleted).Select(e => new StudentGradeModel
            {
                EnrollmentId = e.EnrollmentId,
                CourseId = e.CourseId,
                StudentId = e.StudentId,
                Grade = e.Grade
            }).ToList();
            this.logger.LogInformation("Consulta de calificaciones completada. Total: {Count}.", result.Data.Count);
        }
        catch (Exception ex)
        {
            this.logger.LogError("Error al consultar calificaciones. Tipo: {ExceptionType}.", ex.GetType().Name);
            result = Failure<List<StudentGradeModel>>("Ocurrió un error inesperado al consultar los registros de calificaciones.");
        }
        return result;
    }

    public async Task<OperationResult<List<StudentGradeModel>>> GetByStudentIdAsync(int studentId)
    {
        OperationResult<List<StudentGradeModel>> result = new OperationResult<List<StudentGradeModel>>();
        try
        {
            this.logger.LogInformation("Iniciando consulta de calificaciones.");
            if (studentId <= 0)
                return Failure<List<StudentGradeModel>>("El identificador del estudiante debe ser mayor que cero.");
            var repositoryResult = await this.repository.GetByStudentIdAsync(studentId);
            if (!repositoryResult.Success || repositoryResult.Data is null)
                return Failure<List<StudentGradeModel>>("No fue posible consultar los registros de calificaciones.");

            result.Data = repositoryResult.Data.Where(e => !e.Deleted && e.StudentId == studentId).Select(e => new StudentGradeModel
            {
                EnrollmentId = e.EnrollmentId,
                CourseId = e.CourseId,
                StudentId = e.StudentId,
                Grade = e.Grade
            }).ToList();
            this.logger.LogInformation("Consulta de calificaciones completada. Total: {Count}.", result.Data.Count);
        }
        catch (Exception ex)
        {
            this.logger.LogError("Error al consultar calificaciones. Tipo: {ExceptionType}.", ex.GetType().Name);
            result = Failure<List<StudentGradeModel>>("Ocurrió un error inesperado al consultar los registros de calificaciones.");
        }
        return result;
    }

    public async Task<OperationResult<StudentGradeModel>> GetByIdAsync(int id)
    {
        OperationResult<StudentGradeModel> result = new OperationResult<StudentGradeModel>();
        try
        {
            this.logger.LogInformation("Iniciando consulta de calificación. Identificador: {Id}.", id);
            if (id <= 0)
                return Failure<StudentGradeModel>("El identificador debe ser mayor que cero.");

            var repositoryResult = await this.repository.GetByIdAsync(id);
            if (repositoryResult is null || !repositoryResult.Success || repositoryResult.Data is null)
                return Failure<StudentGradeModel>("No fue posible consultar el registro. Verifique que exista y no esté eliminado.");

            result.Data = repositoryResult.Data;
            this.logger.LogInformation("Consulta de calificación completada. Identificador: {Id}.", id);
        }
        catch (Exception ex)
        {
            this.logger.LogError("Error al consultar calificación. Tipo: {ExceptionType}.", ex.GetType().Name);
            result = Failure<StudentGradeModel>("Ocurrió un error inesperado al consultar el registro de calificación.");
        }
        return result;
    }

    private string? ValidateStudentGrade(StudentGradeBase item)
    {
        if (item.EnrollmentId <= 0 || item.CourseId <= 0 || item.StudentId <= 0)
            return "Los identificadores de inscripción, curso y estudiante deben ser mayores que cero.";
        if (!item.Grade.HasValue)
            return "La calificación es requerida.";
        if (!gradeScale.Minimum.HasValue || !gradeScale.Maximum.HasValue || gradeScale.Minimum.Value >= gradeScale.Maximum.Value)
            return "La escala institucional de calificaciones no está configurada correctamente.";
        if (item.Grade.Value < gradeScale.Minimum.Value || item.Grade.Value > gradeScale.Maximum.Value)
            return "La calificación está fuera de la escala institucional configurada.";
        if (decimal.Round(item.Grade.Value, 4) != item.Grade.Value)
            return "La calificación admite hasta cuatro decimales.";
        return null;
    }

    private OperationResult<T> Failure<T>(string message)
    {
        this.logger.LogWarning("Validación u operación fallida: {ValidationMessage}. Éxito: False.", message);
        return new OperationResult<T> { Success = false, Message = message };
    }
}
