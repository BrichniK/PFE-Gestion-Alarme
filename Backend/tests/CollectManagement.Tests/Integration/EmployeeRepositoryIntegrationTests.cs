using CollectManagement.Domain.Employess;
using CollectManagement.Domain.Employess.ObjectValues;
using CollectManagement.Infrastructure.Persistence.Repositories.EmployeeRepositories;
using FluentAssertions;

namespace CollectManagement.Tests.Integration;

public class EmployeeRepositoryIntegrationTests
{
    private static Employee BuildEmployee(string nom = "Ben Ali", string rfid = "RFID-001")
        => Employee.Create(
            new EmployeeId(Ulid.NewUlid()),
            nom, "Mohamed", 12345678, rfid, null, null);

    [Fact]
    public async Task AddAsync_And_GetOneAsync_Should_Return_Employee()
    {
        await using var ctx = InMemoryDbHelper.CreateContext();
        var repo = new EmployeeRepository(ctx);

        var id  = new EmployeeId(Ulid.NewUlid());
        var emp = Employee.Create(id, "Trabelsi", "Ali", 98765432, "RFID-X", "ali@test.com", null);

        await repo.AddAsync(emp, CancellationToken.None);
        await ctx.SaveChangesAsync();

        var result = await repo.GetOneAsync(id, CancellationToken.None);

        result.Should().NotBeNull();
        result.EmployeeId.Should().Be(id);
        result.Nom.Should().Be("Trabelsi");
    }

    [Fact]
    public async Task GetPagedListAsync_Should_Return_All()
    {
        await using var ctx = InMemoryDbHelper.CreateContext();
        var repo = new EmployeeRepository(ctx);

        await repo.AddAsync(BuildEmployee("Ben Ali", "R1"), CancellationToken.None);
        await repo.AddAsync(BuildEmployee("Chabbi",  "R2"), CancellationToken.None);
        await ctx.SaveChangesAsync();

        var (list, count) = await repo.GetPagedListAsync(null, null, null, 1, 10, CancellationToken.None);

        list.Should().HaveCount(2);
        count.Should().Be(2);
    }

    [Fact]
    public async Task GetPagedListAsync_With_Search_Should_Filter_By_Nom()
    {
        await using var ctx = InMemoryDbHelper.CreateContext();
        var repo = new EmployeeRepository(ctx);

        await repo.AddAsync(BuildEmployee("Ben Ali", "R1"), CancellationToken.None);
        await repo.AddAsync(BuildEmployee("Chabbi",  "R2"), CancellationToken.None);
        await ctx.SaveChangesAsync();

        var (list, _) = await repo.GetPagedListAsync("Ben Ali", null, null, 1, 10, CancellationToken.None);

        list.Should().HaveCount(1);
        list[0].Nom.Should().Be("Ben Ali");
    }

    [Fact]
    public async Task GetByRfidAsync_Should_Return_Employee()
    {
        await using var ctx = InMemoryDbHelper.CreateContext();
        var repo = new EmployeeRepository(ctx);

        await repo.AddAsync(BuildEmployee("Kacem", "RFID-UNIQUE"), CancellationToken.None);
        await ctx.SaveChangesAsync();

        var result = await repo.GetByRfidAsync("RFID-UNIQUE", CancellationToken.None);

        result.Should().NotBeNull();
        result!.Rfid.Should().Be("RFID-UNIQUE");
        result.Nom.Should().Be("Kacem");
    }

    [Fact]
    public async Task GetByRfidAsync_Should_Return_Null_When_Not_Found()
    {
        await using var ctx = InMemoryDbHelper.CreateContext();
        var repo = new EmployeeRepository(ctx);

        var result = await repo.GetByRfidAsync("RFID-INEXISTANT", CancellationToken.None);

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetPagedListAsync_With_Pagination_Should_Return_Correct_Page()
    {
        await using var ctx = InMemoryDbHelper.CreateContext();
        var repo = new EmployeeRepository(ctx);

        for (int i = 1; i <= 6; i++)
            await repo.AddAsync(BuildEmployee($"Emp{i}", $"R{i}"), CancellationToken.None);
        await ctx.SaveChangesAsync();

        var (list, count) = await repo.GetPagedListAsync(null, null, null, 2, 3, CancellationToken.None);

        list.Should().HaveCount(3);
        count.Should().Be(6);
    }
}
