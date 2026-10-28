using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using SchoolApp.BL.Services;
using SchoolApp.DAL.Models;
using SchoolApp.DAL.Models.Objects.Course;
using SchoolApp.DAL.Models.Result;
using SchoolApp.DAL.Repositories;
using Xunit;

namespace SchoolApp.BL.Tests;

public class CourseServicesTests
{
    private readonly RepositoryStub repository = new();
    private CourseServices Service => new(repository, NullLogger<CourseServices>.Instance);

    [Fact]
    public async Task InvalidRequestsDoNotReachPersistence()
    {
        Assert.False((await Service.AddAsync(null)).Success);
        Assert.False((await Service.UpdateAsync(null)).Success);
        Assert.False((await Service.AddAsync(new CourseAdd { Title = "Curso", Credits = -1, DepartmentId = 2, CreationUser = 1 })).Success);
        Assert.False((await Service.AddAsync(new CourseAdd { Title = "  Matemáticas  ", Credits = 3, DepartmentId = 2, CreationUser = 0 })).Success);
        Assert.False((await Service.UpdateAsync(new CourseUpdate { Title = "  Matemáticas  ", Credits = 3, DepartmentId = 2, CourseId = 0, ModifyUser = 1 })).Success);
        Assert.False((await Service.UpdateAsync(new CourseUpdate { Title = "  Matemáticas  ", Credits = 3, DepartmentId = 2, CourseId = 1 })).Success);
        Assert.False((await Service.GetByIdAsync(0)).Success);
        Assert.Equal(0, repository.Calls);
    }

    [Fact]
    public async Task WritesNormalizeFieldsAndUseServerAuditWithoutMutatingInputs()
    {
        var input = new CourseAdd { Title = "  Matemáticas  ", Credits = 3, DepartmentId = 2, CreationUser = 7, CreationDate = new DateTime(2000, 1, 1) };
        var before = DateTime.UtcNow;
        Assert.True((await Service.AddAsync(input)).Data);
        var saved = Assert.IsType<CourseAdd>(repository.Added);
        Assert.Equal(input.Title.Trim(), saved.Title);
        Assert.Equal(input.Credits, saved.Credits);
        Assert.Equal(input.DepartmentId, saved.DepartmentId);
        Assert.Equal(7, saved.CreationUser);
        Assert.InRange(saved.CreationDate, before, DateTime.UtcNow);
        Assert.Equal(DateTimeKind.Utc, saved.CreationDate.Kind);
        Assert.Equal(new DateTime(2000, 1, 1), input.CreationDate);

        var update = new CourseUpdate { CourseId = 1, Title = input.Title, Credits = input.Credits, DepartmentId = input.DepartmentId, ModifyUser = 8 };
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
        var add = await Service.AddAsync(new CourseAdd { Title = "  Matemáticas  ", Credits = 3, DepartmentId = 2, CreationUser = 1 });
        var update = await Service.UpdateAsync(new CourseUpdate { Title = "  Matemáticas  ", Credits = 3, DepartmentId = 2, CourseId = 1, ModifyUser = 1 });
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
        services.AddSingleton<ICourseRepository>(repository);
        services.AddBusinessLogic();
        using var provider = services.BuildServiceProvider();
        using var scope = provider.CreateScope();
        Assert.IsType<CourseServices>(scope.ServiceProvider.GetRequiredService<ICourseServices>());
    }

    private sealed class RepositoryStub : ICourseRepository
    {
        public int Calls { get; private set; }
        public bool Fail { get; set; }
        public bool Throw { get; set; }
        public CourseAdd? Added { get; private set; }
        public CourseUpdate? Updated { get; private set; }
        private void Check()
        {
            Calls++;
            if (Throw) throw new InvalidOperationException("secret database details");
        }
        public Task<OperationResult<bool>> AddAsync(CourseAdd value)
        {
            Check(); Added = value;
            return Task.FromResult(new OperationResult<bool> { Success = !Fail, Data = !Fail, Message = "secret" });
        }
        public Task<OperationResult<bool>> UpdateAsync(CourseUpdate value)
        {
            Check(); Updated = value;
            return Task.FromResult(new OperationResult<bool> { Success = !Fail, Data = !Fail, Message = "secret" });
        }
        public Task<OperationResult<List<Course>>> GetAllAsync()
        {
            Check();
            return Task.FromResult(new OperationResult<List<Course>> { Success = !Fail, Data = new(), Message = "secret" });
        }
        public Task<OperationResult<CourseModel>?> GetByIdAsync(int id)
        {
            Check();
            return Task.FromResult<OperationResult<CourseModel>?>(Fail ? new() { Success = false, Message = "secret" } : null);
        }
        public Task<OperationResult<bool>> DeleteAsync(CourseRemove value) => throw new NotSupportedException();
    }
}
