using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SchoolApp.BL.Services;
using SchoolApp.DAL.Models;
using SchoolApp.DAL.Models.Objects.Department;
using SchoolApp.DAL.Models.Result;
using SchoolApp.DAL.Repositories;
using Xunit;

namespace SchoolApp.BL.Tests;

public class DepartmentServicesTests
{
    private readonly RepositoryStub repository = new();
    private readonly TestLogger logger = new();
    private DepartmentServices Service => new(repository, logger);

    [Theory]
    [InlineData(null, 0, false, null, 1)]
    [InlineData("   ", 0, false, null, 1)]
    [InlineData("Ciencias", -1, false, null, 1)]
    [InlineData("Ciencias", 0, true, null, 1)]
    [InlineData("Ciencias", 0, false, 0, 1)]
    [InlineData("Ciencias", 0, false, -1, 1)]
    [InlineData("Ciencias", 0, false, null, 0)]
    public async Task InvalidInputDoesNotReachPersistence(string? name, int budget,
        bool missingStartDate, int? administrator, int userId)
    {
        var add = ValidAdd();
        add.Name = name;
        add.Budget = budget;
        add.StartDate = missingStartDate ? default : add.StartDate;
        add.Administrator = administrator;
        add.CreationUser = userId;
        var update = new DepartmentUpdate
        {
            DepartmentId = 1, Name = add.Name, Budget = add.Budget,
            StartDate = add.StartDate, Administrator = add.Administrator, ModifyUser = userId
        };

        Assert.False((await Service.AddAsync(add)).Success);
        Assert.False((await Service.UpdateAsync(update)).Success);
        Assert.Equal(0, repository.Calls);
    }

    [Fact]
    public async Task NullInputsAndInvalidIdentifiersAreRejected()
    {
        Assert.False((await Service.AddAsync(null)).Success);
        Assert.False((await Service.UpdateAsync(null)).Success);
        Assert.False((await Service.UpdateAsync(new DepartmentUpdate())).Success);
        Assert.False((await Service.GetByIdAsync(0)).Success);
        Assert.False((await Service.GetByIdAsync(-1)).Success);
        var update = ValidUpdate();
        update.ModifyUser = null;
        Assert.False((await Service.UpdateAsync(update)).Success);
        Assert.Equal(0, repository.Calls);
    }

    [Fact]
    public async Task AddNormalizesNameAndAssignsServerAuditWithoutMutatingInput()
    {
        var input = ValidAdd();
        input.CreationDate = new DateTime(2000, 1, 1);
        var before = DateTime.UtcNow;
        var result = await Service.AddAsync(input);

        Assert.True(result.Success);
        Assert.True(result.Data);
        var saved = Assert.IsType<DepartmentAdd>(repository.Added);
        Assert.Equal("Ciencias", saved.Name);
        Assert.Equal(input.Budget, saved.Budget);
        Assert.Equal(input.StartDate, saved.StartDate);
        Assert.Equal(input.Administrator, saved.Administrator);
        Assert.Equal(input.CreationUser, saved.CreationUser);
        Assert.InRange(saved.CreationDate, before, DateTime.UtcNow);
        Assert.Equal(DateTimeKind.Utc, saved.CreationDate.Kind);
        Assert.Equal(new DateTime(2000, 1, 1), input.CreationDate);
        Assert.Equal("  Ciencias  ", input.Name);
        Assert.Contains(logger.Entries, e => e.Contains("Usuario: 7") && e.Contains("Éxito: True") && e.Contains("Correlación:"));
    }

    [Fact]
    public async Task UpdateAssignsServerAuditAndPreservesFunctionalFields()
    {
        var input = ValidUpdate();
        var before = DateTime.UtcNow;
        var result = await Service.UpdateAsync(input);

        Assert.True(result.Success);
        Assert.True(result.Data);
        var saved = Assert.IsType<DepartmentUpdate>(repository.Updated);
        Assert.Equal(input.DepartmentId, saved.DepartmentId);
        Assert.Equal("Ciencias", saved.Name);
        Assert.Equal(input.Budget, saved.Budget);
        Assert.Equal(input.StartDate, saved.StartDate);
        Assert.Equal(input.Administrator, saved.Administrator);
        Assert.Equal(input.ModifyUser, saved.ModifyUser);
        Assert.InRange(saved.ModifyDate, before, DateTime.UtcNow);
        Assert.Equal(default, input.ModifyDate);
    }

    [Theory]
    [InlineData("Add")]
    [InlineData("Update")]
    [InlineData("GetAll")]
    [InlineData("GetById")]
    public async Task RepositoryFailuresAreControlledAndDoNotLeakInternalDetails(string operation)
    {
        repository.Fail = true;
        var (success, message) = await Invoke(operation);
        Assert.False(success);
        Assert.DoesNotContain("secret", message ?? "");
        Assert.Contains(logger.Entries, e => e.Contains("Éxito: False"));
    }

    [Theory]
    [InlineData("Add")]
    [InlineData("Update")]
    [InlineData("GetAll")]
    [InlineData("GetById")]
    public async Task UnexpectedExceptionsAreControlledAndLoggedWithoutSensitiveData(string operation)
    {
        repository.Throw = true;
        var (success, message) = await Invoke(operation);
        Assert.False(success);
        Assert.DoesNotContain("secret", message ?? "");
        Assert.Contains(logger.Entries, e => e.Contains("InvalidOperationException"));
        Assert.DoesNotContain(logger.Entries, e => e.Contains("secret"));
    }

    [Fact]
    public async Task ListReturnsModelsAndExcludesDeletedDepartments()
    {
        repository.Departments.Add(new Department { DepartmentId = 1, Name = "Ciencias", Budget = 100 });
        repository.Departments.Add(new Department { DepartmentId = 2, Deleted = true });
        var result = await Service.GetAllAsync();
        Assert.True(result.Success);
        var item = Assert.Single(result.Data!);
        Assert.Equal(1, item.DepartmentId);
        Assert.Equal("Ciencias", item.Name);
        Assert.Equal(100, item.Budget);
    }

    [Fact]
    public async Task EmptyListIsSuccessfulAndMissingDepartmentIsControlled()
    {
        var list = await Service.GetAllAsync();
        Assert.True(list.Success);
        Assert.Empty(list.Data!);
        Assert.False((await Service.GetByIdAsync(1)).Success);
        repository.Found = new OperationResult<DepartmentModel>();
        Assert.False((await Service.GetByIdAsync(1)).Success);
    }

    [Fact]
    public async Task GetByIdReturnsRequestedDepartment()
    {
        repository.Found = new OperationResult<DepartmentModel>
        {
            Data = new DepartmentModel { DepartmentId = 1, Name = "Ciencias" }
        };
        var result = await Service.GetByIdAsync(1);
        Assert.True(result.Success);
        Assert.Equal(1, repository.RequestedId);
        Assert.Equal("Ciencias", result.Data!.Name);
    }

    [Fact]
    public void BusinessLogicRegistrationResolvesDepartmentServices()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton<IDepartmentRepository>(repository);
        services.AddBusinessLogic();
        using var provider = services.BuildServiceProvider();
        using var scope = provider.CreateScope();
        Assert.IsType<DepartmentServices>(scope.ServiceProvider.GetRequiredService<IDepartmentServices>());
    }

    private async Task<(bool, string?)> Invoke(string operation)
    {
        if (operation == "Add") { var r = await Service.AddAsync(ValidAdd()); return (r.Success, r.Message); }
        if (operation == "Update") { var r = await Service.UpdateAsync(ValidUpdate()); return (r.Success, r.Message); }
        if (operation == "GetAll") { var r = await Service.GetAllAsync(); return (r.Success, r.Message); }
        var found = await Service.GetByIdAsync(1);
        return (found.Success, found.Message);
    }

    private static DepartmentAdd ValidAdd() => new()
    {
        Name = "  Ciencias  ", Budget = 0, StartDate = new DateTime(2026, 1, 1), CreationUser = 7
    };

    private static DepartmentUpdate ValidUpdate() => new()
    {
        DepartmentId = 1, Name = "  Ciencias  ", Budget = 50,
        StartDate = new DateTime(2026, 1, 1), Administrator = 2, ModifyUser = 7
    };

    private sealed class RepositoryStub : IDepartmentRepository
    {
        public int Calls { get; private set; }
        public bool Fail { get; set; }
        public bool Throw { get; set; }
        public DepartmentAdd? Added { get; private set; }
        public DepartmentUpdate? Updated { get; private set; }
        public int RequestedId { get; private set; }
        public List<Department> Departments { get; } = new();
        public OperationResult<DepartmentModel>? Found { get; set; }

        private void Check()
        {
            Calls++;
            if (Throw) throw new InvalidOperationException("secret database details");
        }

        public Task<OperationResult<bool>> AddAsync(DepartmentAdd department)
        {
            Check(); Added = department;
            return Task.FromResult(new OperationResult<bool> { Success = !Fail, Data = !Fail, Message = "secret database details" });
        }

        public Task<OperationResult<bool>> UpdateAsync(DepartmentUpdate department)
        {
            Check(); Updated = department;
            return Task.FromResult(new OperationResult<bool> { Success = !Fail, Data = !Fail, Message = "secret database details" });
        }

        public Task<OperationResult<List<Department>>> GetAllAsync()
        {
            Check();
            return Task.FromResult(new OperationResult<List<Department>> { Success = !Fail, Data = Departments, Message = "secret database details" });
        }

        public Task<OperationResult<DepartmentModel>?> GetByIdAsync(int id)
        {
            Check(); RequestedId = id;
            return Task.FromResult(Fail ? new OperationResult<DepartmentModel> { Success = false, Message = "secret database details" } : Found);
        }

        public Task<OperationResult<bool>> DeleteAsync(DepartmentRemove department) => throw new NotSupportedException();
    }

    private sealed class TestLogger : ILogger<DepartmentServices>
    {
        public List<string> Entries { get; } = new();
        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;
        public bool IsEnabled(LogLevel logLevel) => true;
        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state,
            Exception? exception, Func<TState, Exception?, string> formatter) => Entries.Add(formatter(state, exception));
    }
}
