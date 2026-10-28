using SchoolApp.DAL.Models.Objects.Department;
using SchoolApp.DAL.Models.Result;

namespace SchoolApp.BL.Services;

public interface IDepartmentServices
{
    Task<OperationResult<List<DepartmentModel>>> GetAllAsync();
    Task<OperationResult<DepartmentModel>> GetByIdAsync(int id);
    Task<OperationResult<bool>> AddAsync(DepartmentAdd? department);
    Task<OperationResult<bool>> UpdateAsync(DepartmentUpdate? department);
}
