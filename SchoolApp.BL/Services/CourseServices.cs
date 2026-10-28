using System.Diagnostics;
using Microsoft.Extensions.Logging;
using SchoolApp.DAL.Models.Objects.Course;
using SchoolApp.DAL.Models.Result;
using SchoolApp.DAL.Repositories;

namespace SchoolApp.BL.Services;

public class CourseServices : ICourseServices
{
    private readonly ICourseRepository repository;
    private readonly ILogger<CourseServices> logger;

    public CourseServices(ICourseRepository repository, ILogger<CourseServices> logger)
    {
        this.repository = repository ?? throw new ArgumentNullException(nameof(repository));
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<OperationResult<bool>> AddAsync(CourseAdd? course)
    {
        OperationResult<bool> result = new OperationResult<bool>();

        try
        {
            this.logger.LogInformation("Iniciando operación de registrar curso.");

            if (course is null)
                return Failure<bool>("El objeto CourseAdd es requerido.");

            var validation = ValidateCourse(course);
            if (validation is not null)
                return Failure<bool>(validation);

            if (course.CreationUser <= 0)
                return Failure<bool>("El usuario de creación debe ser mayor que cero.");

            var repositoryResult = await this.repository.AddAsync(new CourseAdd
            {
                Title = course.Title!.Trim(),
                Credits = course.Credits,
                DepartmentId = course.DepartmentId,
                CreationDate = DateTime.UtcNow,
                CreationUser = course.CreationUser
            });

            result = repositoryResult.Success && repositoryResult.Data
                ? new OperationResult<bool> { Data = true, Message = "Curso registrado correctamente." }
                : Failure<bool>("No fue posible registrar el curso.");

            this.logger.LogInformation("Operación de registrar curso. Usuario: {UserId}. Fecha UTC: {TimestampUtc}. Éxito: {Success}. Correlación: {CorrelationId}.",
                course.CreationUser, DateTime.UtcNow, result.Success,
                Activity.Current?.TraceId.ToString() ?? Guid.NewGuid().ToString("N"));
        }
        catch (Exception ex)
        {
            this.logger.LogError("Error al registrar curso. Tipo: {ExceptionType}.", ex.GetType().Name);
            result = Failure<bool>("Ocurrió un error inesperado al registrar el curso.");
        }

        return result;
    }

    public async Task<OperationResult<bool>> UpdateAsync(CourseUpdate? course)
    {
        OperationResult<bool> result = new OperationResult<bool>();

        try
        {
            this.logger.LogInformation("Iniciando operación de actualizar curso.");

            if (course is null)
                return Failure<bool>("El objeto CourseUpdate es requerido.");

            if (course.CourseId <= 0)
                return Failure<bool>("El identificador del curso debe ser mayor que cero.");

            var validation = ValidateCourse(course);
            if (validation is not null)
                return Failure<bool>(validation);

            if (!course.ModifyUser.HasValue || course.ModifyUser.Value <= 0)
                return Failure<bool>("El usuario de modificación debe ser mayor que cero.");

            var repositoryResult = await this.repository.UpdateAsync(new CourseUpdate
            {
                CourseId = course.CourseId,
                Title = course.Title!.Trim(),
                Credits = course.Credits,
                DepartmentId = course.DepartmentId,
                ModifyDate = DateTime.UtcNow,
                ModifyUser = course.ModifyUser
            });

            result = repositoryResult.Success && repositoryResult.Data
                ? new OperationResult<bool> { Data = true, Message = "Curso actualizado correctamente." }
                : Failure<bool>("No fue posible actualizar el curso.");

            this.logger.LogInformation("Operación de actualizar curso. Usuario: {UserId}. Fecha UTC: {TimestampUtc}. Éxito: {Success}. Correlación: {CorrelationId}.",
                course.ModifyUser, DateTime.UtcNow, result.Success,
                Activity.Current?.TraceId.ToString() ?? Guid.NewGuid().ToString("N"));
        }
        catch (Exception ex)
        {
            this.logger.LogError("Error al actualizar curso. Tipo: {ExceptionType}.", ex.GetType().Name);
            result = Failure<bool>("Ocurrió un error inesperado al actualizar el curso.");
        }

        return result;
    }

    public async Task<OperationResult<List<CourseModel>>> GetAllAsync()
    {
        OperationResult<List<CourseModel>> result = new OperationResult<List<CourseModel>>();

        try
        {
            this.logger.LogInformation("Iniciando consulta de cursos.");
            var repositoryResult = await this.repository.GetAllAsync();
            if (!repositoryResult.Success || repositoryResult.Data is null)
                return Failure<List<CourseModel>>("No fue posible consultar los cursos.");

            result.Data = repositoryResult.Data.Where(d => !d.Deleted)
                .Select(d => new CourseModel
                {
                    CourseId = d.CourseId,
                    Title = d.Title,
                    Credits = d.Credits,
                    DepartmentId = d.DepartmentId
                }).ToList();
            this.logger.LogInformation("Consulta de cursos completada. Total: {Count}.", result.Data.Count);
        }
        catch (Exception ex)
        {
            this.logger.LogError("Error al consultar cursos. Tipo: {ExceptionType}.", ex.GetType().Name);
            result = Failure<List<CourseModel>>("Ocurrió un error inesperado al consultar los cursos.");
        }

        return result;
    }

    public async Task<OperationResult<CourseModel>> GetByIdAsync(int id)
    {
        OperationResult<CourseModel> result = new OperationResult<CourseModel>();

        try
        {
            this.logger.LogInformation("Iniciando consulta de curso. Identificador: {Id}.", id);
            if (id <= 0)
                return Failure<CourseModel>("El identificador del curso debe ser mayor que cero.");

            var repositoryResult = await this.repository.GetByIdAsync(id);
            if (repositoryResult is null || !repositoryResult.Success || repositoryResult.Data is null)
                return Failure<CourseModel>("No fue posible consultar el curso. Verifique que exista y no esté eliminado.");

            result.Data = repositoryResult.Data;
            this.logger.LogInformation("Consulta de curso completada. Identificador: {Id}.", id);
        }
        catch (Exception ex)
        {
            this.logger.LogError("Error al consultar curso. Tipo: {ExceptionType}.", ex.GetType().Name);
            result = Failure<CourseModel>("Ocurrió un error inesperado al consultar el curso.");
        }

        return result;
    }

    private static string? ValidateCourse(CourseBase course)
    {
        if (string.IsNullOrWhiteSpace(course.Title))
            return "El título del curso es requerido.";
        if (course.Credits < 0)
            return "Los créditos no pueden ser negativos.";
        if (course.DepartmentId <= 0)
            return "El identificador del departamento debe ser mayor que cero.";
        return null;
    }

    private OperationResult<T> Failure<T>(string message)
    {
        this.logger.LogWarning("Validación u operación fallida: {ValidationMessage}. Éxito: False.", message);
        return new OperationResult<T> { Success = false, Message = message };
    }
}
