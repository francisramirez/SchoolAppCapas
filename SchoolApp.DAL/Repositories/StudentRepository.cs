using Microsoft.EntityFrameworkCore;
using SchoolApp.DAL.Data;
using SchoolApp.DAL.Models;
namespace SchoolApp.DAL.Repositories;
public class StudentRepository(SchoolDbContext context) : IStudentRepository
{
    public Task<List<Student>> GetAllAsync() => context.Students.AsNoTracking().OrderBy(x => x.FirstName).ToListAsync();
    public Task<Student?> GetByIdAsync(int id) => context.Students.FirstOrDefaultAsync(x => x.Id == id);
    public async Task AddAsync(Student student) { context.Students.Add(student); await context.SaveChangesAsync(); }
    public async Task UpdateAsync(Student student) { await context.SaveChangesAsync(); }
    public async Task DeleteAsync(Student student) { context.Students.Remove(student); await context.SaveChangesAsync(); }
    public Task<bool> EmailExistsAsync(string email, int? exceptId = null) => context.Students.AnyAsync(x => x.FirstName == email && (!exceptId.HasValue || x.Id != exceptId.Value));
}
