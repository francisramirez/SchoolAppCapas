using System.Diagnostics;
using Microsoft.Extensions.Logging;
using SchoolApp.DAL.Models.Objects.Instructor;
using SchoolApp.DAL.Models.Result;
using SchoolApp.DAL.Repositories;

namespace SchoolApp.BL.Services;

public class InstructorServices : IInstructorServices
{
    private readonly IInstructorRepository repository;
    private readonly ILogger<InstructorServices> logger;

    public InstructorServices(IInstructorRepository repository, ILogger<InstructorServices> logger)
    {
        this.repository = repository ?? throw new ArgumentNullException(nameof(repository));
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<OperationResult<bool>> AddAsync(InstructorAdd? instructor)
    {
        OperationResult<bool> result = new OperationResult<bool>();

        try
        {
            this.logger.LogInformation("Iniciando operación de registrar instructor.");

            if (instructor is null)
                return Failure<bool>("El objeto InstructorAdd es requerido.");

            var validation = ValidateInstructor(instructor);
            if (validation is not null)
                return Failure<bool>(validation);

            if (instructor.CreationUser <= 0)
                return Failure<bool>("El usuario de creación debe ser mayor que cero.");

            var repositoryResult = await this.repository.AddAsync(new InstructorAdd
            {
                FirstName = instructor.FirstName!.Trim(),
                LastName = instructor.LastName!.Trim(),
                HireDate = instructor.HireDate,
                CreationDate = DateTime.UtcNow,
                CreationUser = instructor.CreationUser
            });

            result = repositoryResult.Success && repositoryResult.Data
                ? new OperationResult<bool> { Data = true, Message = "Instructor registrado correctamente." }
                : Failure<bool>("No fue posible registrar el instructor.");

            this.logger.LogInformation("Operación de registrar instructor. Usuario: {UserId}. Fecha UTC: {TimestampUtc}. Éxito: {Success}. Correlación: {CorrelationId}.",
                instructor.CreationUser, DateTime.UtcNow, result.Success,
                Activity.Current?.TraceId.ToString() ?? Guid.NewGuid().ToString("N"));
        }
        catch (Exception ex)
        {
            this.logger.LogError("Error al registrar instructor. Tipo: {ExceptionType}.", ex.GetType().Name);
            result = Failure<bool>("Ocurrió un error inesperado al registrar el instructor.");
        }

        return result;
    }

    public async Task<OperationResult<bool>> UpdateAsync(InstructorUpdate? instructor)
    {
        OperationResult<bool> result = new OperationResult<bool>();

        try
        {
            this.logger.LogInformation("Iniciando operación de actualizar instructor.");

            if (instructor is null)
                return Failure<bool>("El objeto InstructorUpdate es requerido.");

            if (instructor.InstructorId <= 0)
                return Failure<bool>("El identificador del instructor debe ser mayor que cero.");

            var validation = ValidateInstructor(instructor);
            if (validation is not null)
                return Failure<bool>(validation);

            if (!instructor.ModifyUser.HasValue || instructor.ModifyUser.Value <= 0)
                return Failure<bool>("El usuario de modificación debe ser mayor que cero.");

            var repositoryResult = await this.repository.UpdateAsync(new InstructorUpdate
            {
                InstructorId = instructor.InstructorId,
                FirstName = instructor.FirstName!.Trim(),
                LastName = instructor.LastName!.Trim(),
                HireDate = instructor.HireDate,
                ModifyDate = DateTime.UtcNow,
                ModifyUser = instructor.ModifyUser
            });

            result = repositoryResult.Success && repositoryResult.Data
                ? new OperationResult<bool> { Data = true, Message = "Instructor actualizado correctamente." }
                : Failure<bool>("No fue posible actualizar el instructor.");

            this.logger.LogInformation("Operación de actualizar instructor. Usuario: {UserId}. Fecha UTC: {TimestampUtc}. Éxito: {Success}. Correlación: {CorrelationId}.",
                instructor.ModifyUser, DateTime.UtcNow, result.Success,
                Activity.Current?.TraceId.ToString() ?? Guid.NewGuid().ToString("N"));
        }
        catch (Exception ex)
        {
            this.logger.LogError("Error al actualizar instructor. Tipo: {ExceptionType}.", ex.GetType().Name);
            result = Failure<bool>("Ocurrió un error inesperado al actualizar el instructor.");
        }

        return result;
    }

    public async Task<OperationResult<List<InstructorModel>>> GetAllAsync()
    {
        OperationResult<List<InstructorModel>> result = new OperationResult<List<InstructorModel>>();

        try
        {
            this.logger.LogInformation("Iniciando consulta de instructores.");
            var repositoryResult = await this.repository.GetAllAsync();
            if (!repositoryResult.Success || repositoryResult.Data is null)
                return Failure<List<InstructorModel>>("No fue posible consultar los instructores.");

            result.Data = repositoryResult.Data.Where(d => !d.Deleted)
                .Select(d => new InstructorModel
                {
                    Id = d.Id,
                    FirstName = d.FirstName,
                    LastName = d.LastName,
                    HireDate = d.HireDate
                }).ToList();
            this.logger.LogInformation("Consulta de instructores completada. Total: {Count}.", result.Data.Count);
        }
        catch (Exception ex)
        {
            this.logger.LogError("Error al consultar instructores. Tipo: {ExceptionType}.", ex.GetType().Name);
            result = Failure<List<InstructorModel>>("Ocurrió un error inesperado al consultar los instructores.");
        }

        return result;
    }

    public async Task<OperationResult<InstructorModel>> GetByIdAsync(int id)
    {
        OperationResult<InstructorModel> result = new OperationResult<InstructorModel>();

        try
        {
            this.logger.LogInformation("Iniciando consulta de instructor. Identificador: {Id}.", id);
            if (id <= 0)
                return Failure<InstructorModel>("El identificador del instructor debe ser mayor que cero.");

            var repositoryResult = await this.repository.GetByIdAsync(id);
            if (repositoryResult is null || !repositoryResult.Success || repositoryResult.Data is null)
                return Failure<InstructorModel>("No fue posible consultar el instructor. Verifique que exista y no esté eliminado.");

            result.Data = repositoryResult.Data;
            this.logger.LogInformation("Consulta de instructor completada. Identificador: {Id}.", id);
        }
        catch (Exception ex)
        {
            this.logger.LogError("Error al consultar instructor. Tipo: {ExceptionType}.", ex.GetType().Name);
            result = Failure<InstructorModel>("Ocurrió un error inesperado al consultar el instructor.");
        }

        return result;
    }

    private static string? ValidateInstructor(InstructorBase instructor)
    {
        if (string.IsNullOrWhiteSpace(instructor.FirstName))
            return "El nombre es requerido.";
        if (string.IsNullOrWhiteSpace(instructor.LastName))
            return "El apellido es requerido.";
        return null;
    }

    private OperationResult<T> Failure<T>(string message)
    {
        this.logger.LogWarning("Validación u operación fallida: {ValidationMessage}. Éxito: False.", message);
        return new OperationResult<T> { Success = false, Message = message };
    }
}
