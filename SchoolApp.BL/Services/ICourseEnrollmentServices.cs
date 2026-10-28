using SchoolApp.DAL.Models.Objects.CourseEnrollment;
using SchoolApp.DAL.Models.Result;

namespace SchoolApp.BL.Services;

public interface ICourseEnrollmentServices
{
    Task<OperationResult<List<CourseEnrollmentModel>>> GetAllAsync();
    Task<OperationResult<List<CourseEnrollmentModel>>> GetByStudentIdAsync(int studentId);
    Task<OperationResult<CourseEnrollmentModel>> GetByIdAsync(int id);
    Task<OperationResult<bool>> AddAsync(CourseEnrollmentAdd? item);
    Task<OperationResult<bool>> UpdateAsync(CourseEnrollmentUpdate? item);
}
