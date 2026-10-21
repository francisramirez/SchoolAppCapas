using Microsoft.EntityFrameworkCore;
using SchoolApp.DAL.Data;
using SchoolApp.DAL.Models;
using SchoolApp.DAL.Models.Objects.Department;
using SchoolApp.DAL.Models.Result;

namespace SchoolApp.DAL.Repositories;

public class DepartmentRepository : IDepartmentRepository
{
    private readonly SchoolDbContext context;

    public DepartmentRepository(SchoolDbContext context)
    {
        this.context = context;
    }

    public async Task<OperationResult<bool>> AddAsync(DepartmentAdd department)
    {
        try
        {
            if (department is null)
                return Failure<bool>("El objeto DepartmentAdd es requerido.");

            var validation = ValidateDepartment(department.Name, department.Budget, department.StartDate, department.Administrator);
            if (validation is not null)
                return Failure<bool>(validation);

            await context.Departments.AddAsync(new Department
            {
                Name = department.Name!.Trim(),
                Budget = department.Budget,
                StartDate = department.StartDate,
                Administrator = department.Administrator,
                CreationDate = department.CreationDate,
                CreationUser = department.CreationUser
            });
            await context.SaveChangesAsync();
            return new OperationResult<bool> { Data = true, Message = "Departamento registrado correctamente." };
        }
        catch (Exception ex)
        {
            return Failure<bool>($"Error guardando los datos del departamento. {ex.Message}");
        }
    }

    public async Task<OperationResult<bool>> UpdateAsync(DepartmentUpdate department)
    {
        try
        {
            if (department is null)
                return Failure<bool>("El objeto DepartmentUpdate es requerido.");
            if (department.DepartmentId <= 0)
                return Failure<bool>("El identificador del departamento debe ser mayor que cero.");

            var validation = ValidateDepartment(department.Name, department.Budget, department.StartDate, department.Administrator);
            if (validation is not null)
                return Failure<bool>(validation);

            var entity = await context.Departments.FirstOrDefaultAsync(d => d.DepartmentId == department.DepartmentId && !d.Deleted);
            if (entity is null)
                return Failure<bool>("El departamento no se encuentra registrado o fue eliminado.");

            entity.Name = department.Name!.Trim();
            entity.Budget = department.Budget;
            entity.StartDate = department.StartDate;
            entity.Administrator = department.Administrator;
            entity.ModifyDate = department.ModifyDate;
            entity.UserMod = department.ModifyUser;
            await context.SaveChangesAsync();
            return new OperationResult<bool> { Data = true, Message = "Departamento actualizado correctamente." };
        }
        catch (Exception ex)
        {
            return Failure<bool>($"Error actualizando los datos del departamento. {ex.Message}");
        }
    }

    public async Task<OperationResult<bool>> DeleteAsync(DepartmentRemove department)
    {
        try
        {
            if (department is null)
                return Failure<bool>("El objeto DepartmentRemove es requerido.");
            if (department.DepartmentId <= 0)
                return Failure<bool>("El identificador del departamento debe ser mayor que cero.");

            var entity = await context.Departments.FirstOrDefaultAsync(d => d.DepartmentId == department.DepartmentId && !d.Deleted);
            if (entity is null)
                return Failure<bool>("El departamento no se encuentra registrado o fue eliminado.");

            entity.Deleted = true;
            entity.DeletedDate = department.DeletedDate;
            entity.UserDeleted = department.UserDeleted;
            await context.SaveChangesAsync();
            return new OperationResult<bool> { Data = true, Message = "Departamento eliminado correctamente." };
        }
        catch (Exception ex)
        {
            return Failure<bool>($"Error eliminando los datos del departamento. {ex.Message}");
        }
    }

    public async Task<OperationResult<List<Department>>> GetAllAsync()
    {
        try
        {
            var departments = await context.Departments.AsNoTracking()
                .Where(d => !d.Deleted)
                .ToListAsync();
            return new OperationResult<List<Department>> { Data = departments };
        }
        catch (Exception ex)
        {
            return Failure<List<Department>>($"Error consultando los departamentos. {ex.Message}");
        }
    }

    public async Task<OperationResult<DepartmentModel>?> GetByIdAsync(int id)
    {
        try
        {
            if (id <= 0)
                return Failure<DepartmentModel>("El identificador del departamento debe ser mayor que cero.");

            var department = await context.Departments.AsNoTracking()
                .Where(d => d.DepartmentId == id && !d.Deleted)
                .Select(d => new DepartmentModel
                {
                    DepartmentId = d.DepartmentId,
                    Name = d.Name,
                    Budget = d.Budget,
                    StartDate = d.StartDate,
                    Administrator = d.Administrator
                })
                .FirstOrDefaultAsync();
            if (department is null)
                return Failure<DepartmentModel>("El departamento no se encuentra registrado o fue eliminado.");

            return new OperationResult<DepartmentModel> { Data = department };
        }
        catch (Exception ex)
        {
            return Failure<DepartmentModel>($"Error consultando los datos del departamento. {ex.Message}");
        }
    }

    private static string? ValidateDepartment(string? name, decimal budget, DateTime startDate, int? administrator)
    {
        if (string.IsNullOrWhiteSpace(name))
            return "El nombre del departamento es requerido.";
        if (budget < 0)
            return "El presupuesto no puede ser negativo.";
        if (startDate == default)
            return "La fecha de inicio es requerida.";
        if (administrator.HasValue && administrator.Value <= 0)
            return "El identificador del administrador debe ser mayor que cero.";
        return null;
    }

    private static OperationResult<T> Failure<T>(string message) => new()
    {
        Success = false,
        Message = message
    };
}
