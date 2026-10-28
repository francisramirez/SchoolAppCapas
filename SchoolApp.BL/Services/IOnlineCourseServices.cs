using SchoolApp.DAL.Models.Objects.OnlineCourse;
using SchoolApp.DAL.Models.Result;

namespace SchoolApp.BL.Services;

public interface IOnlineCourseServices
{
    Task<OperationResult<List<OnlineCourseModel>>> GetAllAsync();
    Task<OperationResult<OnlineCourseModel>> GetByIdAsync(int id);
    Task<OperationResult<bool>> AddAsync(OnlineCourseAdd? item);
    Task<OperationResult<bool>> UpdateAsync(OnlineCourseUpdate? item);
}
