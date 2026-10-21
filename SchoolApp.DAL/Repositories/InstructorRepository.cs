using Microsoft.EntityFrameworkCore;
using SchoolApp.DAL.Data;
using SchoolApp.DAL.Models;
using SchoolApp.DAL.Models.Objects.Instructor;
using SchoolApp.DAL.Models.Result;

namespace SchoolApp.DAL.Repositories;

public class InstructorRepository : IInstructorRepository
{
    private readonly SchoolDbContext context;

    public InstructorRepository(SchoolDbContext context)
    {
        this.context = context;
    }

    public async Task<OperationResult<bool>> AddAsync(InstructorAdd instructor)
    {
        try
        {
            if (instructor is null)
                return Failure<bool>("El objeto InstructorAdd es requerido.");

            var validation = ValidateInstructor(instructor.FirstName, instructor.LastName);
            if (validation is not null)
                return Failure<bool>(validation);

            await context.Instructors.AddAsync(new Instructor
            {
                FirstName = instructor.FirstName!.Trim(),
                LastName = instructor.LastName!.Trim(),
                HireDate = instructor.HireDate,
                CreationDate = instructor.CreationDate,
                CreationUser = instructor.CreationUser
            });
            await context.SaveChangesAsync();
            return new OperationResult<bool> { Data = true, Message = "Instructor registrado correctamente." };
        }
        catch (Exception ex)
        {
            return Failure<bool>($"Error guardando los datos del instructor. {ex.Message}");
        }
    }

    public async Task<OperationResult<bool>> UpdateAsync(InstructorUpdate instructor)
    {
        try
        {
            if (instructor is null)
                return Failure<bool>("El objeto InstructorUpdate es requerido.");
            if (instructor.InstructorId <= 0)
                return Failure<bool>("El identificador del instructor debe ser mayor que cero.");

            var validation = ValidateInstructor(instructor.FirstName, instructor.LastName);
            if (validation is not null)
                return Failure<bool>(validation);

            var entity = await context.Instructors.FirstOrDefaultAsync(i => i.Id == instructor.InstructorId && !i.Deleted);
            if (entity is null)
                return Failure<bool>("El instructor no se encuentra registrado o fue eliminado.");

            entity.FirstName = instructor.FirstName!.Trim();
            entity.LastName = instructor.LastName!.Trim();
            entity.HireDate = instructor.HireDate;
            entity.ModifyDate = instructor.ModifyDate;
            entity.UserMod = instructor.ModifyUser;
            await context.SaveChangesAsync();
            return new OperationResult<bool> { Data = true, Message = "Instructor actualizado correctamente." };
        }
        catch (Exception ex)
        {
            return Failure<bool>($"Error actualizando los datos del instructor. {ex.Message}");
        }
    }

    public async Task<OperationResult<bool>> DeleteAsync(InstructorRemove instructor)
    {
        try
        {
            if (instructor is null)
                return Failure<bool>("El objeto InstructorRemove es requerido.");
            if (instructor.InstructorId <= 0)
                return Failure<bool>("El identificador del instructor debe ser mayor que cero.");

            var entity = await context.Instructors.FirstOrDefaultAsync(i => i.Id == instructor.InstructorId && !i.Deleted);
            if (entity is null)
                return Failure<bool>("El instructor no se encuentra registrado o fue eliminado.");

            entity.Deleted = true;
            entity.DeletedDate = instructor.DeletedDate;
            entity.UserDeleted = instructor.UserDeleted;
            await context.SaveChangesAsync();
            return new OperationResult<bool> { Data = true, Message = "Instructor eliminado correctamente." };
        }
        catch (Exception ex)
        {
            return Failure<bool>($"Error eliminando los datos del instructor. {ex.Message}");
        }
    }

    public async Task<OperationResult<List<Instructor>>> GetAllAsync()
    {
        try
        {
            var instructors = await context.Instructors.AsNoTracking()
                .Where(i => !i.Deleted)
                .ToListAsync();
            return new OperationResult<List<Instructor>> { Data = instructors };
        }
        catch (Exception ex)
        {
            return Failure<List<Instructor>>($"Error consultando los instructores. {ex.Message}");
        }
    }

    public async Task<OperationResult<InstructorModel>?> GetByIdAsync(int id)
    {
        try
        {
            if (id <= 0)
                return Failure<InstructorModel>("El identificador del instructor debe ser mayor que cero.");

            var instructor = await context.Instructors.AsNoTracking()
                .Where(i => i.Id == id && !i.Deleted)
                .Select(i => new InstructorModel
                {
                    Id = i.Id,
                    FirstName = i.FirstName,
                    LastName = i.LastName,
                    HireDate = i.HireDate
                })
                .FirstOrDefaultAsync();
            if (instructor is null)
                return Failure<InstructorModel>("El instructor no se encuentra registrado o fue eliminado.");

            return new OperationResult<InstructorModel> { Data = instructor };
        }
        catch (Exception ex)
        {
            return Failure<InstructorModel>($"Error consultando los datos del instructor. {ex.Message}");
        }
    }

    private static string? ValidateInstructor(string? firstName, string? lastName)
    {
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
