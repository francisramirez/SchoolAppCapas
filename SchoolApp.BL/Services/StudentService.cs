using System.Diagnostics;
using Microsoft.Extensions.Logging;
using SchoolApp.DAL.Models.Objects.Student;
using SchoolApp.DAL.Models.Result;
using SchoolApp.DAL.Repositories;

namespace SchoolApp.BL.Services;

public class StudentService : IStudentService
{
    private readonly IStudentRepository repository;
    private readonly ILogger<StudentService> logger;

    public StudentService(IStudentRepository repository, ILogger<StudentService> logger)
    {
        this.repository = repository ?? throw new ArgumentNullException(nameof(repository));
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<OperationResult<bool>> AddAsync(StudentAdd? student)
    {
        OperationResult<bool> result = new OperationResult<bool>();

        try
        {
            this.logger.LogInformation("Iniciando operación de registrar estudiante.");

            if (student is null)
                return Failure<bool>("El objeto StudentAdd es requerido.");

            var validation = ValidateStudent(student);
            if (validation is not null)
                return Failure<bool>(validation);

            if (student.CreationUser <= 0)
                return Failure<bool>("El usuario de creación debe ser mayor que cero.");

            var repositoryResult = await this.repository.AddAsync(new StudentAdd
            {
                StudentNumber = student.StudentNumber!.Trim(),
                FirstName = student.FirstName!.Trim(),
                LastName = student.LastName!.Trim(),
                EnrollmentDate = student.EnrollmentDate,
                CreationDate = DateTime.UtcNow,
                CreationUser = student.CreationUser
            });

            result = repositoryResult.Success && repositoryResult.Data
                ? new OperationResult<bool> { Data = true, Message = "Estudiante registrado correctamente." }
                : Failure<bool>("No fue posible registrar el estudiante.");

            this.logger.LogInformation("Operación de registrar estudiante. Usuario: {UserId}. Fecha UTC: {TimestampUtc}. Éxito: {Success}. Correlación: {CorrelationId}.",
                student.CreationUser, DateTime.UtcNow, result.Success,
                Activity.Current?.TraceId.ToString() ?? Guid.NewGuid().ToString("N"));
        }
        catch (Exception ex)
        {
            this.logger.LogError("Error al registrar estudiante. Tipo: {ExceptionType}.", ex.GetType().Name);
            result = Failure<bool>("Ocurrió un error inesperado al registrar el estudiante.");
        }

        return result;
    }

    public async Task<OperationResult<bool>> UpdateAsync(StudentUpdate? student)
    {
        OperationResult<bool> result = new OperationResult<bool>();

        try
        {
            this.logger.LogInformation("Iniciando operación de actualizar estudiante.");

            if (student is null)
                return Failure<bool>("El objeto StudentUpdate es requerido.");

            if (student.StudentId <= 0)
                return Failure<bool>("El identificador del estudiante debe ser mayor que cero.");

            var validation = ValidateStudent(student);
            if (validation is not null)
                return Failure<bool>(validation);

            if (!student.ModifyUser.HasValue || student.ModifyUser.Value <= 0)
                return Failure<bool>("El usuario de modificación debe ser mayor que cero.");

            var repositoryResult = await this.repository.UpdateAsync(new StudentUpdate
            {
                StudentId = student.StudentId,
                StudentNumber = student.StudentNumber!.Trim(),
                FirstName = student.FirstName!.Trim(),
                LastName = student.LastName!.Trim(),
                EnrollmentDate = student.EnrollmentDate,
                ModifyDate = DateTime.UtcNow,
                ModifyUser = student.ModifyUser
            });

            result = repositoryResult.Success && repositoryResult.Data
                ? new OperationResult<bool> { Data = true, Message = "Estudiante actualizado correctamente." }
                : Failure<bool>("No fue posible actualizar el estudiante.");

            this.logger.LogInformation("Operación de actualizar estudiante. Usuario: {UserId}. Fecha UTC: {TimestampUtc}. Éxito: {Success}. Correlación: {CorrelationId}.",
                student.ModifyUser, DateTime.UtcNow, result.Success,
                Activity.Current?.TraceId.ToString() ?? Guid.NewGuid().ToString("N"));
        }
        catch (Exception ex)
        {
            this.logger.LogError("Error al actualizar estudiante. Tipo: {ExceptionType}.", ex.GetType().Name);
            result = Failure<bool>("Ocurrió un error inesperado al actualizar el estudiante.");
        }

        return result;
    }

    public async Task<OperationResult<List<StudentModel>>> GetAllAsync()
    {
        OperationResult<List<StudentModel>> result = new OperationResult<List<StudentModel>>();

        try
        {
            this.logger.LogInformation("Iniciando consulta de estudiantes.");
            var repositoryResult = await this.repository.GetAllAsync();
            if (!repositoryResult.Success || repositoryResult.Data is null)
                return Failure<List<StudentModel>>("No fue posible consultar los estudiantes.");

            result.Data = repositoryResult.Data.Where(d => !d.Deleted)
                .Select(d => new StudentModel
                {
                    Id = d.Id,
                    Name = d.FirstName,
                    LastName = d.LastName
                }).ToList();
            this.logger.LogInformation("Consulta de estudiantes completada. Total: {Count}.", result.Data.Count);
        }
        catch (Exception ex)
        {
            this.logger.LogError("Error al consultar estudiantes. Tipo: {ExceptionType}.", ex.GetType().Name);
            result = Failure<List<StudentModel>>("Ocurrió un error inesperado al consultar los estudiantes.");
        }

        return result;
    }

    public async Task<OperationResult<StudentModel>> GetByIdAsync(int id)
    {
        OperationResult<StudentModel> result = new OperationResult<StudentModel>();

        try
        {
            this.logger.LogInformation("Iniciando consulta de estudiante. Identificador: {Id}.", id);
            if (id <= 0)
                return Failure<StudentModel>("El identificador del estudiante debe ser mayor que cero.");

            var repositoryResult = await this.repository.GetByIdAsync(id);
            if (repositoryResult is null || !repositoryResult.Success || repositoryResult.Data is null)
                return Failure<StudentModel>("No fue posible consultar el estudiante. Verifique que exista y no esté eliminado.");

            result.Data = repositoryResult.Data;
            this.logger.LogInformation("Consulta de estudiante completada. Identificador: {Id}.", id);
        }
        catch (Exception ex)
        {
            this.logger.LogError("Error al consultar estudiante. Tipo: {ExceptionType}.", ex.GetType().Name);
            result = Failure<StudentModel>("Ocurrió un error inesperado al consultar el estudiante.");
        }

        return result;
    }

    private static string? ValidateStudent(StudentBase student)
    {
        if (string.IsNullOrWhiteSpace(student.StudentNumber))
            return "La matrícula es requerida.";
        if (student.StudentNumber.Length > 50)
            return "La matrícula no puede ser mayor a 50 caracteres.";
        if (string.IsNullOrWhiteSpace(student.FirstName))
            return "El nombre es requerido.";
        if (string.IsNullOrWhiteSpace(student.LastName))
            return "El apellido es requerido.";
        return null;
    }

    private OperationResult<T> Failure<T>(string message)
    {
        this.logger.LogWarning("Validación u operación fallida: {ValidationMessage}. Éxito: False.", message);
        return new OperationResult<T> { Success = false, Message = message };
    }
}
