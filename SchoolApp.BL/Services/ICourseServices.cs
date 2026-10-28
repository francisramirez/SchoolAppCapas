using SchoolApp.DAL.Models.Objects.Course;
using SchoolApp.DAL.Models.Result;

namespace SchoolApp.BL.Services;

public interface ICourseServices
{
    Task<OperationResult<List<CourseModel>>> GetAllAsync();
    Task<OperationResult<CourseModel>> GetByIdAsync(int id);
    Task<OperationResult<bool>> AddAsync(CourseAdd? course);
    Task<OperationResult<bool>> UpdateAsync(CourseUpdate? course);
}
