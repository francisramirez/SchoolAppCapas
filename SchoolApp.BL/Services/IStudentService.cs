using SchoolApp.DAL.Models;
namespace SchoolApp.BL.Services;
public interface IStudentService
{
    Task<List<Student>> GetAllAsync();
    Task<Student?> GetByIdAsync(int id);
    Task CreateAsync(Student student);
    Task UpdateAsync(Student student);
    Task DeleteAsync(int id);
}
