using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using SchoolApp.BL.Services;
using SchoolApp.DAL.Models;
using SchoolApp.DAL.Models.Objects.Instructor;
using SchoolApp.DAL.Models.Result;
using SchoolApp.DAL.Repositories;
using Xunit;

namespace SchoolApp.BL.Tests;

public class InstructorServicesTests
{
    private readonly RepositoryStub repository = new();
    private InstructorServices Service => new(repository, NullLogger<InstructorServices>.Instance);

    [Fact]
    public async Task InvalidRequestsDoNotReachPersistence()
    {
        Assert.False((await Service.AddAsync(null)).Success);
        Assert.False((await Service.UpdateAsync(null)).Success);
        Assert.False((await Service.AddAsync(new InstructorAdd { FirstName = " ", LastName = "Pérez", CreationUser = 1 })).Success);
        Assert.False((await Service.AddAsync(new InstructorAdd { FirstName = "  Ana  ", LastName = "  Pérez  ", CreationUser = 0 })).Success);
        Assert.False((await Service.UpdateAsync(new InstructorUpdate { FirstName = "  Ana  ", LastName = "  Pérez  ", InstructorId = 0, ModifyUser = 1 })).Success);
        Assert.False((await Service.UpdateAsync(new InstructorUpdate { FirstName = "  Ana  ", LastName = "  Pérez  ", InstructorId = 1 })).Success);
        Assert.False((await Service.GetByIdAsync(0)).Success);
        Assert.Equal(0, repository.Calls);
    }

    [Fact]
    public async Task WritesNormalizeFieldsAndUseServerAuditWithoutMutatingInputs()
    {
        var input = new InstructorAdd { FirstName = "  Ana  ", LastName = "  Pérez  ", CreationUser = 7, CreationDate = new DateTime(2000, 1, 1) };
        var before = DateTime.UtcNow;
        Assert.True((await Service.AddAsync(input)).Data);
        var saved = Assert.IsType<InstructorAdd>(repository.Added);
        Assert.Equal(input.FirstName.Trim(), saved.FirstName);
        Assert.Equal(input.LastName.Trim(), saved.LastName);
        Assert.Equal(input.HireDate, saved.HireDate);
        Assert.Equal(7, saved.CreationUser);
        Assert.InRange(saved.CreationDate, before, DateTime.UtcNow);
        Assert.Equal(DateTimeKind.Utc, saved.CreationDate.Kind);
        Assert.Equal(new DateTime(2000, 1, 1), input.CreationDate);

        var update = new InstructorUpdate { InstructorId = 1, FirstName = input.FirstName, LastName = input.LastName, HireDate = input.HireDate, ModifyUser = 8 };
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
        var add = await Service.AddAsync(new InstructorAdd { FirstName = "  Ana  ", LastName = "  Pérez  ", CreationUser = 1 });
        var update = await Service.UpdateAsync(new InstructorUpdate { FirstName = "  Ana  ", LastName = "  Pérez  ", InstructorId = 1, ModifyUser = 1 });
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
        services.AddSingleton<IInstructorRepository>(repository);
        services.AddBusinessLogic();
        using var provider = services.BuildServiceProvider();
        using var scope = provider.CreateScope();
        Assert.IsType<InstructorServices>(scope.ServiceProvider.GetRequiredService<IInstructorServices>());
    }

    private sealed class RepositoryStub : IInstructorRepository
    {
        public int Calls { get; private set; }
        public bool Fail { get; set; }
        public bool Throw { get; set; }
        public InstructorAdd? Added { get; private set; }
        public InstructorUpdate? Updated { get; private set; }
        private void Check()
        {
            Calls++;
            if (Throw) throw new InvalidOperationException("secret database details");
        }
        public Task<OperationResult<bool>> AddAsync(InstructorAdd value)
        {
            Check(); Added = value;
            return Task.FromResult(new OperationResult<bool> { Success = !Fail, Data = !Fail, Message = "secret" });
        }
        public Task<OperationResult<bool>> UpdateAsync(InstructorUpdate value)
        {
            Check(); Updated = value;
            return Task.FromResult(new OperationResult<bool> { Success = !Fail, Data = !Fail, Message = "secret" });
        }
        public Task<OperationResult<List<Instructor>>> GetAllAsync()
        {
            Check();
            return Task.FromResult(new OperationResult<List<Instructor>> { Success = !Fail, Data = new(), Message = "secret" });
        }
        public Task<OperationResult<InstructorModel>?> GetByIdAsync(int id)
        {
            Check();
            return Task.FromResult<OperationResult<InstructorModel>?>(Fail ? new() { Success = false, Message = "secret" } : null);
        }
        public Task<OperationResult<bool>> DeleteAsync(InstructorRemove value) => throw new NotSupportedException();
    }
}
