using SchoolApp.DAL.Models;
using SchoolApp.DAL.Models.Objects.Course;
using SchoolApp.DAL.Models.Result;

namespace SchoolApp.DAL.Repositories;

public interface ICourseRepository
{
    Task<OperationResult<List<Course>>> GetAllAsync();
    Task<OperationResult<CourseModel>?> GetByIdAsync(int id);
    Task<OperationResult<bool>> AddAsync(CourseAdd course);
    Task<OperationResult<bool>> UpdateAsync(CourseUpdate course);
    Task<OperationResult<bool>> DeleteAsync(CourseRemove course);
}
