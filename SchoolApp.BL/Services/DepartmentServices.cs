using System.Diagnostics;
using Microsoft.Extensions.Logging;
using SchoolApp.DAL.Models.Objects.Department;
using SchoolApp.DAL.Models.Result;
using SchoolApp.DAL.Repositories;

namespace SchoolApp.BL.Services;

public class DepartmentServices : IDepartmentServices
{
    private readonly IDepartmentRepository repository;
    private readonly ILogger<DepartmentServices> logger;

    public DepartmentServices(IDepartmentRepository repository, ILogger<DepartmentServices> logger)
    {
        this.repository = repository ?? throw new ArgumentNullException(nameof(repository));
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<OperationResult<bool>> AddAsync(DepartmentAdd? department)
    {
        OperationResult<bool> result = new OperationResult<bool>();

        try
        {
            this.logger.LogInformation("Iniciando operación de registrar departamento.");

            if (department is null)
                return Failure<bool>("El objeto DepartmentAdd es requerido.");

            var validation = ValidateDepartment(department);
            if (validation is not null)
                return Failure<bool>(validation);

            if (department.CreationUser <= 0)
                return Failure<bool>("El usuario de creación debe ser mayor que cero.");

            var repositoryResult = await this.repository.AddAsync(new DepartmentAdd
            {
                Name = department.Name!.Trim(),
                Budget = department.Budget,
                StartDate = department.StartDate,
                Administrator = department.Administrator,
                CreationDate = DateTime.UtcNow,
                CreationUser = department.CreationUser
            });

            result = repositoryResult.Success && repositoryResult.Data
                ? new OperationResult<bool> { Data = true, Message = "Departamento registrado correctamente." }
                : Failure<bool>("No fue posible registrar el departamento.");

            this.logger.LogInformation("Operación de registrar departamento. Usuario: {UserId}. Fecha UTC: {TimestampUtc}. Éxito: {Success}. Correlación: {CorrelationId}.",
                department.CreationUser, DateTime.UtcNow, result.Success,
                Activity.Current?.TraceId.ToString() ?? Guid.NewGuid().ToString("N"));
        }
        catch (Exception ex)
        {
            this.logger.LogError("Error al registrar departamento. Tipo: {ExceptionType}.", ex.GetType().Name);
            result = Failure<bool>("Ocurrió un error inesperado al registrar el departamento.");
        }

        return result;
    }

    public async Task<OperationResult<bool>> UpdateAsync(DepartmentUpdate? department)
    {
        OperationResult<bool> result = new OperationResult<bool>();

        try
        {
            this.logger.LogInformation("Iniciando operación de actualizar departamento.");

            if (department is null)
                return Failure<bool>("El objeto DepartmentUpdate es requerido.");

            if (department.DepartmentId <= 0)
                return Failure<bool>("El identificador del departamento debe ser mayor que cero.");

            var validation = ValidateDepartment(department);
            if (validation is not null)
                return Failure<bool>(validation);

            if (!department.ModifyUser.HasValue || department.ModifyUser.Value <= 0)
                return Failure<bool>("El usuario de modificación debe ser mayor que cero.");

            var repositoryResult = await this.repository.UpdateAsync(new DepartmentUpdate
            {
                DepartmentId = department.DepartmentId,
                Name = department.Name!.Trim(),
                Budget = department.Budget,
                StartDate = department.StartDate,
                Administrator = department.Administrator,
                ModifyDate = DateTime.UtcNow,
                ModifyUser = department.ModifyUser
            });

            result = repositoryResult.Success && repositoryResult.Data
                ? new OperationResult<bool> { Data = true, Message = "Departamento actualizado correctamente." }
                : Failure<bool>("No fue posible actualizar el departamento.");

            this.logger.LogInformation("Operación de actualizar departamento. Usuario: {UserId}. Fecha UTC: {TimestampUtc}. Éxito: {Success}. Correlación: {CorrelationId}.",
                department.ModifyUser, DateTime.UtcNow, result.Success,
                Activity.Current?.TraceId.ToString() ?? Guid.NewGuid().ToString("N"));
        }
        catch (Exception ex)
        {
            this.logger.LogError("Error al actualizar departamento. Tipo: {ExceptionType}.", ex.GetType().Name);
            result = Failure<bool>("Ocurrió un error inesperado al actualizar el departamento.");
        }

        return result;
    }

    public async Task<OperationResult<List<DepartmentModel>>> GetAllAsync()
    {
        OperationResult<List<DepartmentModel>> result = new OperationResult<List<DepartmentModel>>();

        try
        {
            this.logger.LogInformation("Iniciando consulta de departamentos.");
            var repositoryResult = await this.repository.GetAllAsync();
            if (!repositoryResult.Success || repositoryResult.Data is null)
                return Failure<List<DepartmentModel>>("No fue posible consultar los departamentos.");

            result.Data = repositoryResult.Data.Where(d => !d.Deleted)
                .Select(d => new DepartmentModel
                {
                    DepartmentId = d.DepartmentId,
                    Name = d.Name,
                    Budget = d.Budget,
                    StartDate = d.StartDate,
                    Administrator = d.Administrator
                }).ToList();
            this.logger.LogInformation("Consulta de departamentos completada. Total: {Count}.", result.Data.Count);
        }
        catch (Exception ex)
        {
            this.logger.LogError("Error al consultar departamentos. Tipo: {ExceptionType}.", ex.GetType().Name);
            result = Failure<List<DepartmentModel>>("Ocurrió un error inesperado al consultar los departamentos.");
        }

        return result;
    }

    public async Task<OperationResult<DepartmentModel>> GetByIdAsync(int id)
    {
        OperationResult<DepartmentModel> result = new OperationResult<DepartmentModel>();

        try
        {
            this.logger.LogInformation("Iniciando consulta de departamento. Identificador: {Id}.", id);
            if (id <= 0)
                return Failure<DepartmentModel>("El identificador del departamento debe ser mayor que cero.");

            var repositoryResult = await this.repository.GetByIdAsync(id);
            if (repositoryResult is null || !repositoryResult.Success || repositoryResult.Data is null)
                return Failure<DepartmentModel>("No fue posible consultar el departamento. Verifique que exista y no esté eliminado.");

            result.Data = repositoryResult.Data;
            this.logger.LogInformation("Consulta de departamento completada. Identificador: {Id}.", id);
        }
        catch (Exception ex)
        {
            this.logger.LogError("Error al consultar departamento. Tipo: {ExceptionType}.", ex.GetType().Name);
            result = Failure<DepartmentModel>("Ocurrió un error inesperado al consultar el departamento.");
        }

        return result;
    }

    private static string? ValidateDepartment(DepartmentBase department)
    {
        if (string.IsNullOrWhiteSpace(department.Name))
            return "El nombre del departamento es requerido.";
        if (department.Budget < 0)
            return "El presupuesto no puede ser negativo.";
        if (department.StartDate == default)
            return "La fecha de inicio es requerida.";
        if (department.Administrator.HasValue && department.Administrator.Value <= 0)
            return "El identificador del administrador debe ser mayor que cero.";
        return null;
    }

    private OperationResult<T> Failure<T>(string message)
    {
        this.logger.LogWarning("Validación u operación fallida: {ValidationMessage}. Éxito: False.", message);
        return new OperationResult<T> { Success = false, Message = message };
    }
}
