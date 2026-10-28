using System.Data;
using Microsoft.EntityFrameworkCore;
using SchoolApp.DAL.Data;
using SchoolApp.DAL.Models;
using SchoolApp.DAL.Models.Objects.StudentGrade;
using SchoolApp.DAL.Models.Result;

namespace SchoolApp.DAL.Repositories;

public class StudentGradeRepository : IStudentGradeRepository
{
    private readonly SchoolDbContext context;

    public StudentGradeRepository(SchoolDbContext context)
    {
        this.context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<OperationResult<bool>> AddAsync(StudentGradeAdd item)
    {
        try
        {
            if (item is null)
                return Failure<bool>("El objeto StudentGradeAdd es requerido.");

            var validation = ValidateStudentGrade(item);
            if (validation is not null)
                return Failure<bool>(validation);

            await using var transaction = await context.Database.BeginTransactionAsync(IsolationLevel.Serializable);
            if (await context.StudentGrades.AnyAsync(e => e.EnrollmentId == item.EnrollmentId))
                return Failure<bool>("Ya existe un registro para este identificador, incluso si fue eliminado.");

            var relationError = await ValidateRelationsAsync(item);
            if (relationError is not null)
                return Failure<bool>(relationError);

            await context.StudentGrades.AddAsync(new StudentGrade
            {
                EnrollmentId = item.EnrollmentId,
                CourseId = item.CourseId,
                StudentId = item.StudentId,
                Grade = item.Grade,
                CreationDate = item.CreationDate,
                CreationUser = item.CreationUser
            });
            await context.SaveChangesAsync();
            await transaction.CommitAsync();
            return new OperationResult<bool> { Data = true, Message = "Registro completado." };
        }
        catch (Exception)
        {
            return Failure<bool>("No fue posible registrar el registro de calificación.");
        }
    }

    public async Task<OperationResult<bool>> UpdateAsync(StudentGradeUpdate item)
    {
        try
        {
            if (item is null)
                return Failure<bool>("El objeto StudentGradeUpdate es requerido.");

            var validation = ValidateStudentGrade(item);
            if (validation is not null)
                return Failure<bool>(validation);

            await using var transaction = await context.Database.BeginTransactionAsync(IsolationLevel.Serializable);
            var entity = await context.StudentGrades.FirstOrDefaultAsync(e => e.EnrollmentId == item.EnrollmentId && !e.Deleted);
            if (entity is null)
                return Failure<bool>("El registro no existe o fue eliminado.");

            var relationError = await ValidateRelationsAsync(item, item.EnrollmentId);
            if (relationError is not null)
                return Failure<bool>(relationError);

            entity.CourseId = item.CourseId;
            entity.StudentId = item.StudentId;
            entity.Grade = item.Grade;
            entity.ModifyDate = item.ModifyDate;
            entity.UserMod = item.ModifyUser;
            await context.SaveChangesAsync();
            await transaction.CommitAsync();
            return new OperationResult<bool> { Data = true, Message = "Actualización completada." };
        }
        catch (Exception)
        {
            return Failure<bool>("No fue posible actualizar el registro de calificación.");
        }
    }

    public async Task<OperationResult<bool>> DeleteAsync(StudentGradeRemove item)
    {
        try
        {
            if (item is null)
                return Failure<bool>("El objeto StudentGradeRemove es requerido.");
            if (item.EnrollmentId <= 0)
                return Failure<bool>("El identificador debe ser mayor que cero.");

            var entity = await context.StudentGrades.FirstOrDefaultAsync(e => e.EnrollmentId == item.EnrollmentId && !e.Deleted);
            if (entity is null)
                return Failure<bool>("El registro no existe o fue eliminado.");

            entity.Deleted = true;
            entity.DeletedDate = item.DeletedDate;
            entity.UserDeleted = item.UserDeleted;
            await context.SaveChangesAsync();
            return new OperationResult<bool> { Data = true, Message = "Registro eliminado correctamente." };
        }
        catch (Exception)
        {
            return Failure<bool>("No fue posible eliminar el registro de calificación.");
        }
    }

    public async Task<OperationResult<List<StudentGrade>>> GetAllAsync()
    {
        try
        {
            var data = await context.StudentGrades.AsNoTracking().Where(e => !e.Deleted).ToListAsync();
            return new OperationResult<List<StudentGrade>> { Data = data };
        }
        catch (Exception)
        {
            return Failure<List<StudentGrade>>("No fue posible consultar los registros de calificaciones.");
        }
    }

    public async Task<OperationResult<StudentGradeModel>?> GetByIdAsync(int id)
    {
        try
        {
            if (id <= 0)
                return Failure<StudentGradeModel>("El identificador debe ser mayor que cero.");

            var data = await context.StudentGrades.AsNoTracking()
                .Where(e => e.EnrollmentId == id && !e.Deleted)
                .Select(e => new StudentGradeModel
                {
                    EnrollmentId = e.EnrollmentId,
                    CourseId = e.CourseId,
                    StudentId = e.StudentId,
                    Grade = e.Grade
                }).FirstOrDefaultAsync();
            if (data is null)
                return Failure<StudentGradeModel>("El registro no existe o fue eliminado.");

            return new OperationResult<StudentGradeModel> { Data = data };
        }
        catch (Exception)
        {
            return Failure<StudentGradeModel>("No fue posible consultar el registro de calificación.");
        }
    }

    public async Task<OperationResult<List<StudentGrade>>> GetByStudentIdAsync(int studentId)
    {
        try
        {
            if (studentId <= 0)
                return Failure<List<StudentGrade>>("El identificador del estudiante debe ser mayor que cero.");

            var data = await context.StudentGrades.AsNoTracking()
                .Where(e => e.StudentId == studentId && !e.Deleted).ToListAsync();
            return new OperationResult<List<StudentGrade>> { Data = data };
        }
        catch (Exception)
        {
            return Failure<List<StudentGrade>>("No fue posible consultar los registros del estudiante.");
        }
    }

    private static string? ValidateStudentGrade(StudentGradeBase item)
    {
        if (item.EnrollmentId <= 0 || item.CourseId <= 0 || item.StudentId <= 0)
            return "Los identificadores de inscripción, curso y estudiante deben ser mayores que cero.";
        if (!item.Grade.HasValue)
            return "La calificación es requerida.";
        return null;
    }

    private async Task<string?> ValidateRelationsAsync(StudentGradeBase item, int? exceptId = null)
    {
        if (!await context.CourseEnrollments.AnyAsync(e => e.EnrollmentId == item.EnrollmentId && !e.Deleted
            && e.CourseId == item.CourseId && e.StudentId == item.StudentId))
            return "La calificación debe corresponder al estudiante y curso de una inscripción válida.";
        if (!await context.Courses.AnyAsync(c => c.CourseId == item.CourseId && !c.Deleted)
            || !await context.Students.AnyAsync(s => s.Id == item.StudentId && !s.Deleted))
            return "El curso o estudiante de la inscripción fue eliminado.";
        return null;
    }

    private static OperationResult<T> Failure<T>(string message) => new()
    {
        Success = false,
        Message = message
    };
}
