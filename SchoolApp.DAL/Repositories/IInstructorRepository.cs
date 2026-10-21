using SchoolApp.DAL.Models;
using SchoolApp.DAL.Models.Objects.Instructor;
using SchoolApp.DAL.Models.Result;

namespace SchoolApp.DAL.Repositories;

public interface IInstructorRepository
{
    Task<OperationResult<List<Instructor>>> GetAllAsync();
    Task<OperationResult<InstructorModel>?> GetByIdAsync(int id);
    Task<OperationResult<bool>> AddAsync(InstructorAdd instructor);
    Task<OperationResult<bool>> UpdateAsync(InstructorUpdate instructor);
    Task<OperationResult<bool>> DeleteAsync(InstructorRemove instructor);
}
