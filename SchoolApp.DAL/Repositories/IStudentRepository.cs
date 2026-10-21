using SchoolApp.DAL.Models;
using SchoolApp.DAL.Models.Objects.Student;
using SchoolApp.DAL.Models.Result;
namespace SchoolApp.DAL.Repositories;
public interface IStudentRepository
{
    Task<OperationResult<List<Student>>> GetAllAsync();
    Task<OperationResult<StudentModel>?> GetByIdAsync(int id);
    Task<OperationResult<bool>> AddAsync(StudentAdd student);
    Task<OperationResult<bool>> UpdateAsync(StudentUpdate student);
    Task<OperationResult<bool>> DeleteAsync(StudentRemove student);
    Task<OperationResult<bool>> EmailExistsAsync(string email, int? exceptId = null);
}
