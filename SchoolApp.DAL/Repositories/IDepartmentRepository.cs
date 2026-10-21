using SchoolApp.DAL.Models;
using SchoolApp.DAL.Models.Objects.Department;
using SchoolApp.DAL.Models.Result;

namespace SchoolApp.DAL.Repositories;

public interface IDepartmentRepository
{
    Task<OperationResult<List<Department>>> GetAllAsync();
    Task<OperationResult<DepartmentModel>?> GetByIdAsync(int id);
    Task<OperationResult<bool>> AddAsync(DepartmentAdd department);
    Task<OperationResult<bool>> UpdateAsync(DepartmentUpdate department);
    Task<OperationResult<bool>> DeleteAsync(DepartmentRemove department);
}
