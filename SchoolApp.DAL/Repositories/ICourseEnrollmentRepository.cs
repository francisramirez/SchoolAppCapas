using SchoolApp.DAL.Models;
using SchoolApp.DAL.Models.Objects.CourseEnrollment;
using SchoolApp.DAL.Models.Result;

namespace SchoolApp.DAL.Repositories;

public interface ICourseEnrollmentRepository
{
    Task<OperationResult<List<CourseEnrollment>>> GetAllAsync();
    Task<OperationResult<List<CourseEnrollment>>> GetByStudentIdAsync(int studentId);
    Task<OperationResult<CourseEnrollmentModel>?> GetByIdAsync(int id);
    Task<OperationResult<bool>> AddAsync(CourseEnrollmentAdd item);
    Task<OperationResult<bool>> UpdateAsync(CourseEnrollmentUpdate item);
    Task<OperationResult<bool>> DeleteAsync(CourseEnrollmentRemove item);
}
