using SchoolApp.DAL.Models;
using SchoolApp.DAL.Models.Objects.OnlineCourse;
using SchoolApp.DAL.Models.Result;

namespace SchoolApp.DAL.Repositories;

public interface IOnlineCourseRepository
{
    Task<OperationResult<List<OnlineCourse>>> GetAllAsync();
    Task<OperationResult<OnlineCourseModel>?> GetByIdAsync(int id);
    Task<OperationResult<bool>> AddAsync(OnlineCourseAdd item);
    Task<OperationResult<bool>> UpdateAsync(OnlineCourseUpdate item);
    Task<OperationResult<bool>> DeleteAsync(OnlineCourseRemove item);
}
