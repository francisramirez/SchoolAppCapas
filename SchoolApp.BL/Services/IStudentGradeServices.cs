using SchoolApp.DAL.Models.Objects.StudentGrade;
using SchoolApp.DAL.Models.Result;

namespace SchoolApp.BL.Services;

public interface IStudentGradeServices
{
    Task<OperationResult<List<StudentGradeModel>>> GetAllAsync();
    Task<OperationResult<List<StudentGradeModel>>> GetByStudentIdAsync(int studentId);
    Task<OperationResult<StudentGradeModel>> GetByIdAsync(int id);
    Task<OperationResult<bool>> AddAsync(StudentGradeAdd? item);
    Task<OperationResult<bool>> UpdateAsync(StudentGradeUpdate? item);
}
