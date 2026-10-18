using SchoolApp.DAL.Models;
namespace SchoolApp.DAL.Repositories;
public interface IStudentRepository
{
    Task<List<Student>> GetAllAsync();
    Task<Student?> GetByIdAsync(int id);
    Task AddAsync(Student student);
    Task UpdateAsync(Student student);
    Task DeleteAsync(Student student);
    Task<bool> EmailExistsAsync(string email, int? exceptId = null);
}
