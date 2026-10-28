using SchoolApp.DAL.Models;
using SchoolApp.DAL.Models.Objects.OnsiteCourse;
using SchoolApp.DAL.Models.Result;

namespace SchoolApp.DAL.Repositories;

public interface IOnsiteCourseRepository
{
    Task<OperationResult<List<OnsiteCourse>>> GetAllAsync();
    Task<OperationResult<OnsiteCourseModel>?> GetByIdAsync(int id);
    Task<OperationResult<bool>> AddAsync(OnsiteCourseAdd item);
    Task<OperationResult<bool>> UpdateAsync(OnsiteCourseUpdate item);
    Task<OperationResult<bool>> DeleteAsync(OnsiteCourseRemove item);
}
