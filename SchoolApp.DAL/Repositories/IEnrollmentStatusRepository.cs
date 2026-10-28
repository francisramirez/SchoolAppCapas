using SchoolApp.DAL.Models;
using SchoolApp.DAL.Models.Objects.EnrollmentStatus;
using SchoolApp.DAL.Models.Result;

namespace SchoolApp.DAL.Repositories;

public interface IEnrollmentStatusRepository
{
    Task<OperationResult<List<EnrollmentStatus>>> GetAllAsync();
    Task<OperationResult<EnrollmentStatusModel>?> GetByIdAsync(int id);
    Task<OperationResult<bool>> AddAsync(EnrollmentStatusAdd item);
    Task<OperationResult<bool>> UpdateAsync(EnrollmentStatusUpdate item);
    Task<OperationResult<bool>> DeleteAsync(EnrollmentStatusRemove item);
}
