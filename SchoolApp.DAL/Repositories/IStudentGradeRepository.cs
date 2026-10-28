using SchoolApp.DAL.Models;
using SchoolApp.DAL.Models.Objects.StudentGrade;
using SchoolApp.DAL.Models.Result;

namespace SchoolApp.DAL.Repositories;

public interface IStudentGradeRepository
{
    Task<OperationResult<List<StudentGrade>>> GetAllAsync();
    Task<OperationResult<List<StudentGrade>>> GetByStudentIdAsync(int studentId);
    Task<OperationResult<StudentGradeModel>?> GetByIdAsync(int id);
    Task<OperationResult<bool>> AddAsync(StudentGradeAdd item);
    Task<OperationResult<bool>> UpdateAsync(StudentGradeUpdate item);
    Task<OperationResult<bool>> DeleteAsync(StudentGradeRemove item);
}
