using SchoolApp.DAL.Models.Objects.OnsiteCourse;
using SchoolApp.DAL.Models.Result;

namespace SchoolApp.BL.Services;

public interface IOnsiteCourseServices
{
    Task<OperationResult<List<OnsiteCourseModel>>> GetAllAsync();
    Task<OperationResult<OnsiteCourseModel>> GetByIdAsync(int id);
    Task<OperationResult<bool>> AddAsync(OnsiteCourseAdd? item);
    Task<OperationResult<bool>> UpdateAsync(OnsiteCourseUpdate? item);
}
