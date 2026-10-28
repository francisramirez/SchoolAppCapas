using SchoolApp.DAL.Models.Objects.Instructor;
using SchoolApp.DAL.Models.Result;

namespace SchoolApp.BL.Services;

public interface IInstructorServices
{
    Task<OperationResult<List<InstructorModel>>> GetAllAsync();
    Task<OperationResult<InstructorModel>> GetByIdAsync(int id);
    Task<OperationResult<bool>> AddAsync(InstructorAdd? instructor);
    Task<OperationResult<bool>> UpdateAsync(InstructorUpdate? instructor);
}
