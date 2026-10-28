using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using SchoolApp.BL.Services;
using SchoolApp.DAL.Models;
using SchoolApp.DAL.Models.Objects.Student;
using SchoolApp.DAL.Models.Result;
using SchoolApp.DAL.Repositories;
using Xunit;

namespace SchoolApp.BL.Tests;

public class StudentServiceTests
{
    private readonly RepositoryStub repository = new();
    private StudentService Service => new(repository, NullLogger<StudentService>.Instance);

    [Fact]
    public async Task InvalidRequestsDoNotReachPersistence()
    {
        Assert.False((await Service.AddAsync(null)).Success);
        Assert.False((await Service.UpdateAsync(null)).Success);
        Assert.False((await Service.AddAsync(new StudentAdd { StudentNumber = " ", FirstName = "Ana", LastName = "Pérez", CreationUser = 1 })).Success);
        Assert.False((await Service.AddAsync(new StudentAdd { StudentNumber = "  A001  ", FirstName = "  Ana  ", LastName = "  Pérez  ", CreationUser = 0 })).Success);
        Assert.False((await Service.UpdateAsync(new StudentUpdate { StudentNumber = "  A001  ", FirstName = "  Ana  ", LastName = "  Pérez  ", StudentId = 0, ModifyUser = 1 })).Success);
        Assert.False((await Service.UpdateAsync(new StudentUpdate { StudentNumber = "  A001  ", FirstName = "  Ana  ", LastName = "  Pérez  ", StudentId = 1 })).Success);
        Assert.False((await Service.GetByIdAsync(0)).Success);
        Assert.Equal(0, repository.Calls);
    }

    [Fact]
    public async Task WritesNormalizeFieldsAndUseServerAuditWithoutMutatingInputs()
    {
        var input = new StudentAdd { StudentNumber = "  A001  ", FirstName = "  Ana  ", LastName = "  Pérez  ", CreationUser = 7, CreationDate = new DateTime(2000, 1, 1) };
        var before = DateTime.UtcNow;
        Assert.True((await Service.AddAsync(input)).Data);
        var saved = Assert.IsType<StudentAdd>(repository.Added);
        Assert.Equal(input.StudentNumber.Trim(), saved.StudentNumber);
        Assert.Equal(input.FirstName.Trim(), saved.FirstName);
        Assert.Equal(input.LastName.Trim(), saved.LastName);
        Assert.Equal(input.EnrollmentDate, saved.EnrollmentDate);
        Assert.Equal(7, saved.CreationUser);
        Assert.InRange(saved.CreationDate, before, DateTime.UtcNow);
        Assert.Equal(DateTimeKind.Utc, saved.CreationDate.Kind);
        Assert.Equal(new DateTime(2000, 1, 1), input.CreationDate);

        var update = new StudentUpdate { StudentId = 1, StudentNumber = input.StudentNumber, FirstName = input.FirstName, LastName = input.LastName, EnrollmentDate = input.EnrollmentDate, ModifyUser = 8 };
        Assert.True((await Service.UpdateAsync(update)).Data);
        Assert.Equal(8, repository.Updated!.ModifyUser);
        Assert.InRange(repository.Updated.ModifyDate, before, DateTime.UtcNow);
        Assert.Equal(default, update.ModifyDate);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task RepositoryFailuresAndExceptionsReturnControlledErrors(bool throws)
    {
        repository.Fail = true;
        repository.Throw = throws;
        var add = await Service.AddAsync(new StudentAdd { StudentNumber = "  A001  ", FirstName = "  Ana  ", LastName = "  Pérez  ", CreationUser = 1 });
        var update = await Service.UpdateAsync(new StudentUpdate { StudentNumber = "  A001  ", FirstName = "  Ana  ", LastName = "  Pérez  ", StudentId = 1, ModifyUser = 1 });
        var all = await Service.GetAllAsync();
        var one = await Service.GetByIdAsync(1);
        Assert.False(add.Success);
        Assert.False(update.Success);
        Assert.False(all.Success);
        Assert.False(one.Success);
        foreach (var message in new[] { add.Message, update.Message, all.Message, one.Message })
            Assert.DoesNotContain("secret", message ?? "");
    }

    [Fact]
    public async Task EmptyListAndMissingRecordAreHandled()
    {
        var all = await Service.GetAllAsync();
        Assert.True(all.Success);
        Assert.Empty(all.Data!);
        Assert.False((await Service.GetByIdAsync(1)).Success);
    }

    [Fact]
    public void DependencyInjectionResolvesService()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton<IStudentRepository>(repository);
        services.AddBusinessLogic();
        using var provider = services.BuildServiceProvider();
        using var scope = provider.CreateScope();
        Assert.IsType<StudentService>(scope.ServiceProvider.GetRequiredService<IStudentService>());
    }

    private sealed class RepositoryStub : IStudentRepository
    {
        public int Calls { get; private set; }
        public bool Fail { get; set; }
        public bool Throw { get; set; }
        public StudentAdd? Added { get; private set; }
        public StudentUpdate? Updated { get; private set; }
        private void Check()
        {
            Calls++;
            if (Throw) throw new InvalidOperationException("secret database details");
        }
        public Task<OperationResult<bool>> AddAsync(StudentAdd value)
        {
            Check(); Added = value;
            return Task.FromResult(new OperationResult<bool> { Success = !Fail, Data = !Fail, Message = "secret" });
        }
        public Task<OperationResult<bool>> UpdateAsync(StudentUpdate value)
        {
            Check(); Updated = value;
            return Task.FromResult(new OperationResult<bool> { Success = !Fail, Data = !Fail, Message = "secret" });
        }
        public Task<OperationResult<List<Student>>> GetAllAsync()
        {
            Check();
            return Task.FromResult(new OperationResult<List<Student>> { Success = !Fail, Data = new(), Message = "secret" });
        }
        public Task<OperationResult<StudentModel>?> GetByIdAsync(int id)
        {
            Check();
            return Task.FromResult<OperationResult<StudentModel>?>(Fail ? new() { Success = false, Message = "secret" } : null);
        }
        public Task<OperationResult<bool>> DeleteAsync(StudentRemove value) => throw new NotSupportedException();
        public Task<OperationResult<bool>> EmailExistsAsync(string email, int? exceptId = null) => throw new NotSupportedException();
    }
}
