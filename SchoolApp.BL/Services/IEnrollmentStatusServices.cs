using SchoolApp.DAL.Models.Objects.EnrollmentStatus;
using SchoolApp.DAL.Models.Result;

namespace SchoolApp.BL.Services;

public interface IEnrollmentStatusServices
{
    Task<OperationResult<List<EnrollmentStatusModel>>> GetAllAsync();
    Task<OperationResult<EnrollmentStatusModel>> GetByIdAsync(int id);
    Task<OperationResult<bool>> AddAsync(EnrollmentStatusAdd? item);
    Task<OperationResult<bool>> UpdateAsync(EnrollmentStatusUpdate? item);
}
