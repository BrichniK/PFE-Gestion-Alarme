using CollectManagement.Domain.Shifts;
using CollectManagement.Domain.Shifts.ValueObjects;
using CollectManagement.Infrastructure.Persistence.Repositories.ShiftRepositories;
using FluentAssertions;

namespace CollectManagement.Tests.Integration;

public class ShiftRepositoryIntegrationTests
{
    [Fact]
    public async Task AddAsync_And_GetOneAsync_Should_Return_Shift()
    {
        await using var ctx  = InMemoryDbHelper.CreateContext();
        var repo = new ShiftRepository(ctx);

        var id    = new ShiftId(Ulid.NewUlid());
        var shift = Shift.Create(id, "Matin", new TimeOnly(8, 0), new TimeOnly(16, 0));

        await repo.AddAsync(shift, CancellationToken.None);
        await ctx.SaveChangesAsync();

        var result = await repo.GetOneAsync(id, CancellationToken.None);

        result.Should().NotBeNull();
        result.ShiftId.Should().Be(id);
        result.Label.Should().Be("Matin");
    }

    [Fact]
    public async Task GetPagedListAsync_Should_Return_All_Shifts()
    {
        await using var ctx  = InMemoryDbHelper.CreateContext();
        var repo = new ShiftRepository(ctx);

        await repo.AddAsync(Shift.Create(new ShiftId(Ulid.NewUlid()), "Matin",  new TimeOnly(6, 0), new TimeOnly(14, 0)), CancellationToken.None);
        await repo.AddAsync(Shift.Create(new ShiftId(Ulid.NewUlid()), "Soir",   new TimeOnly(14, 0), new TimeOnly(22, 0)), CancellationToken.None);
        await repo.AddAsync(Shift.Create(new ShiftId(Ulid.NewUlid()), "Nuit",   new TimeOnly(22, 0), new TimeOnly(6, 0)), CancellationToken.None);
        await ctx.SaveChangesAsync();

        var (list, count) = await repo.GetPagedListAsync(null, null, null, 1, 10, CancellationToken.None);

        list.Should().HaveCount(3);
        count.Should().Be(3);
    }

    [Fact]
    public async Task GetPagedListAsync_With_Search_Should_Filter()
    {
        await using var ctx  = InMemoryDbHelper.CreateContext();
        var repo = new ShiftRepository(ctx);

        await repo.AddAsync(Shift.Create(new ShiftId(Ulid.NewUlid()), "Matin", new TimeOnly(6, 0), new TimeOnly(14, 0)), CancellationToken.None);
        await repo.AddAsync(Shift.Create(new ShiftId(Ulid.NewUlid()), "Nuit",  new TimeOnly(22, 0), new TimeOnly(6, 0)), CancellationToken.None);
        await ctx.SaveChangesAsync();

        var (list, _) = await repo.GetPagedListAsync("Matin", null, null, 1, 10, CancellationToken.None);

        list.Should().HaveCount(1);
        list[0].Label.Should().Be("Matin");
    }

    [Fact]
    public async Task GetOneAsync_Should_Return_Null_When_Not_Found()
    {
        await using var ctx = InMemoryDbHelper.CreateContext();
        var repo = new ShiftRepository(ctx);

        var result = await repo.GetOneAsync(new ShiftId(Ulid.NewUlid()), CancellationToken.None);

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetPagedListAsync_With_Order_Should_Return_Results()
    {
        await using var ctx = InMemoryDbHelper.CreateContext();
        var repo = new ShiftRepository(ctx);

        await repo.AddAsync(Shift.Create(new ShiftId(Ulid.NewUlid()), "Matin", new TimeOnly(6, 0), new TimeOnly(14, 0)), CancellationToken.None);
        await repo.AddAsync(Shift.Create(new ShiftId(Ulid.NewUlid()), "Soir",  new TimeOnly(14, 0), new TimeOnly(22, 0)), CancellationToken.None);
        await ctx.SaveChangesAsync();

        // No sort param — default ordering
        var (list, _) = await repo.GetPagedListAsync(null, null, null, 1, 10, CancellationToken.None);

        list.Should().HaveCount(2);
    }
}
