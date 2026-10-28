using SchoolApp.DAL.Models.Objects.Student;
using SchoolApp.DAL.Models.Result;

namespace SchoolApp.BL.Services;

public interface IStudentService
{
    Task<OperationResult<List<StudentModel>>> GetAllAsync();
    Task<OperationResult<StudentModel>> GetByIdAsync(int id);
    Task<OperationResult<bool>> AddAsync(StudentAdd? student);
    Task<OperationResult<bool>> UpdateAsync(StudentUpdate? student);
}
