using Microsoft.EntityFrameworkCore;
using SchoolApp.DAL.Data;
using SchoolApp.DAL.Models;
using SchoolApp.DAL.Models.Objects.Student;
using SchoolApp.DAL.Models.Result;

namespace SchoolApp.DAL.Repositories;

public class StudentRepository : IStudentRepository
{
    private readonly SchoolDbContext context;

    public StudentRepository(SchoolDbContext context)
    {
        this.context = context;
    }

    public async Task<OperationResult<bool>> AddAsync(StudentAdd student)
    {
        try
        {
            if (student is null)
                return Failure<bool>("El objeto StudentAdd es requerido.");

            var validation = ValidateStudent(student.StudentNumber, student.FirstName, student.LastName);
            if (validation is not null)
                return Failure<bool>(validation);

            await context.Students.AddAsync(new Student
            {
                StudentNumber = student.StudentNumber!.Trim(),
                FirstName = student.FirstName!.Trim(),
                LastName = student.LastName!.Trim(),
                CreationDate = student.CreationDate,
                CreationUser = student.CreationUser,
                EnrollmentDate = student.EnrollmentDate
            });
            await context.SaveChangesAsync();
            return new OperationResult<bool> { Data = true, Message = "Estudiante registrado correctamente." };
        }
        catch (Exception ex)
        {
            return Failure<bool>($"Error guardando los datos del estudiante. {ex.Message}");
        }
    }

    public async Task<OperationResult<bool>> DeleteAsync(StudentRemove student)
    {
        try
        {
            if (student is null)
                return Failure<bool>("El objeto StudentRemove es requerido.");
            if (student.StudentId <= 0)
                return Failure<bool>("El identificador del estudiante debe ser mayor que cero.");

            var entity = await context.Students.FirstOrDefaultAsync(st => st.Id == student.StudentId && !st.Deleted);
            if (entity is null)
                return Failure<bool>("El estudiante no se encuentra registrado o fue eliminado.");

            entity.Deleted = true;
            entity.DeletedDate = student.DeletedDate;
            entity.UserDeleted = student.UserDeleted;
            await context.SaveChangesAsync();
            return new OperationResult<bool> { Data = true, Message = "Estudiante eliminado correctamente." };
        }
        catch (Exception ex)
        {
            return Failure<bool>($"Error eliminando los datos del estudiante. {ex.Message}");
        }
    }

    public Task<OperationResult<bool>> EmailExistsAsync(string email, int? exceptId = null)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(email))
                return Task.FromResult(Failure<bool>("El correo electrónico es requerido."));
            if (exceptId.HasValue && exceptId.Value <= 0)
                return Task.FromResult(Failure<bool>("El identificador a excluir debe ser mayor que cero."));

            // Student no contiene un campo de correo que permita realizar esta consulta.
            return Task.FromResult(Failure<bool>("No se puede verificar el correo electrónico porque el modelo Student no tiene un campo de correo."));
        }
        catch (Exception ex)
        {
            return Task.FromResult(Failure<bool>($"Error verificando el correo electrónico del estudiante. {ex.Message}"));
        }
    }

    public async Task<OperationResult<List<Student>>> GetAllAsync()
    {
        try
        {
            var students = await context.Students.AsNoTracking()
                .Where(st => !st.Deleted)
                .ToListAsync();
            return new OperationResult<List<Student>> { Data = students };
        }
        catch (Exception ex)
        {
            return Failure<List<Student>>($"Error consultando los estudiantes. {ex.Message}");
        }
    }

    public async Task<OperationResult<StudentModel>?> GetByIdAsync(int id)
    {
        try
        {
            if (id <= 0)
                return Failure<StudentModel>("El identificador del estudiante debe ser mayor que cero.");

            var student = await context.Students.AsNoTracking()
                .Where(st => st.Id == id && !st.Deleted)
                .Select(st => new StudentModel
                {
                    Id = st.Id,
                    LastName = st.LastName,
                    Name = st.FirstName
                })
                .FirstOrDefaultAsync();
            if (student is null)
                return Failure<StudentModel>("El estudiante no se encuentra registrado o fue eliminado.");

            return new OperationResult<StudentModel> { Data = student };
        }
        catch (Exception ex)
        {
            return Failure<StudentModel>($"Error consultando los datos del estudiante. {ex.Message}");
        }
    }

    public async Task<OperationResult<bool>> UpdateAsync(StudentUpdate student)
    {
        try
        {
            if (student is null)
                return Failure<bool>("El objeto StudentUpdate es requerido.");
            if (student.StudentId <= 0)
                return Failure<bool>("El identificador del estudiante debe ser mayor que cero.");

            var validation = ValidateStudent(student.StudentNumber, student.FirstName, student.LastName);
            if (validation is not null)
                return Failure<bool>(validation);

            var entity = await context.Students.FirstOrDefaultAsync(st => st.Id == student.StudentId && !st.Deleted);
            if (entity is null)
                return Failure<bool>("El estudiante no se encuentra registrado o fue eliminado.");

            entity.StudentNumber = student.StudentNumber!.Trim();
            entity.FirstName = student.FirstName!.Trim();
            entity.LastName = student.LastName!.Trim();
            entity.EnrollmentDate = student.EnrollmentDate;
            entity.ModifyDate = student.ModifyDate;
            entity.UserMod = student.ModifyUser;
            await context.SaveChangesAsync();
            return new OperationResult<bool> { Data = true, Message = "Estudiante actualizado correctamente." };
        }
        catch (Exception ex)
        {
            return Failure<bool>($"Error actualizando los datos del estudiante. {ex.Message}");
        }
    }

    private static string? ValidateStudent(string? studentNumber, string? firstName, string? lastName)
    {
        if (string.IsNullOrWhiteSpace(studentNumber))
            return "La matrícula es requerida.";
        if (studentNumber.Length > 50)
            return "La matrícula no puede ser mayor a 50 caracteres.";
        if (string.IsNullOrWhiteSpace(firstName))
            return "El nombre es requerido.";
        if (string.IsNullOrWhiteSpace(lastName))
            return "El apellido es requerido.";
        return null;
    }

    private static OperationResult<T> Failure<T>(string message) => new()
    {
        Success = false,
        Message = message
    };
}
