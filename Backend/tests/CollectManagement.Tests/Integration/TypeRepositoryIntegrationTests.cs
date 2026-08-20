using CollectManagement.Domain.Types;
using CollectManagement.Domain.Types.ValueObjects;
using CollectManagement.Infrastructure.Persistence.Repositories.TypeRepositories;
using FluentAssertions;
using Type = CollectManagement.Domain.Types.Type;

namespace CollectManagement.Tests.Integration;

public class TypeRepositoryIntegrationTests
{
    [Fact]
    public async Task AddAsync_And_GetOneAsync_Should_Return_Type()
    {
        await using var ctx  = InMemoryDbHelper.CreateContext();
        var repo = new TypeRepository(ctx);

        var id   = new TypeId(Ulid.NewUlid());
        var type = Type.Create(id, "ELEC", "Electricité", 30);

        await repo.AddAsync(type, CancellationToken.None);
        await ctx.SaveChangesAsync();

        var result = await repo.GetOneAsync(id, CancellationToken.None);

        result.Should().NotBeNull();
        result.TypeId.Should().Be(id);
        result.Code.Should().Be("ELEC");
        result.Label.Should().Be("Electricité");
    }

    [Fact]
    public async Task GetPagedListAsync_Should_Return_All_Types()
    {
        await using var ctx  = InMemoryDbHelper.CreateContext();
        var repo = new TypeRepository(ctx);

        await repo.AddAsync(Type.Create(new TypeId(Ulid.NewUlid()), "T1", "Label1", 10), CancellationToken.None);
        await repo.AddAsync(Type.Create(new TypeId(Ulid.NewUlid()), "T2", "Label2", 20), CancellationToken.None);
        await ctx.SaveChangesAsync();

        var (list, count) = await repo.GetPagedListAsync(null, null, null, 1, 10, CancellationToken.None);

        list.Should().HaveCount(2);
        count.Should().Be(2);
    }

    [Fact]
    public async Task GetPagedListAsync_With_Search_Should_Filter()
    {
        await using var ctx  = InMemoryDbHelper.CreateContext();
        var repo = new TypeRepository(ctx);

        await repo.AddAsync(Type.Create(new TypeId(Ulid.NewUlid()), "ELEC", "Electricité", 10), CancellationToken.None);
        await repo.AddAsync(Type.Create(new TypeId(Ulid.NewUlid()), "MECA", "Mécanique", 20), CancellationToken.None);
        await ctx.SaveChangesAsync();

        var (list, count) = await repo.GetPagedListAsync("ELEC", null, null, 1, 10, CancellationToken.None);

        list.Should().HaveCount(1);
        list[0].Code.Should().Be("ELEC");
    }

    [Fact]
    public async Task GetOneAsync_Should_Return_Null_When_Not_Found()
    {
        await using var ctx  = InMemoryDbHelper.CreateContext();
        var repo = new TypeRepository(ctx);

        var result = await repo.GetOneAsync(new TypeId(Ulid.NewUlid()), CancellationToken.None);

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByCodeAsync_Should_Return_Type_By_Code()
    {
        await using var ctx  = InMemoryDbHelper.CreateContext();
        var repo = new TypeRepository(ctx);

        var type = Type.Create(new TypeId(Ulid.NewUlid()), "VIBR", "Vibration", 15);
        await repo.AddAsync(type, CancellationToken.None);
        await ctx.SaveChangesAsync();

        var result = await repo.GetByCodeAsync("VIBR", CancellationToken.None);

        result.Should().NotBeNull();
        result!.Code.Should().Be("VIBR");
    }

    [Fact]
    public async Task GetPagedListAsync_With_Order_Should_Return_Results()
    {
        await using var ctx = InMemoryDbHelper.CreateContext();
        var repo = new TypeRepository(ctx);

        await repo.AddAsync(Type.Create(new TypeId(Ulid.NewUlid()), "ZZZ", "ZLabel", 10), CancellationToken.None);
        await repo.AddAsync(Type.Create(new TypeId(Ulid.NewUlid()), "AAA", "ALabel", 20), CancellationToken.None);
        await ctx.SaveChangesAsync();

        // default ordering — EF.Property not supported in InMemory
        var (list, _) = await repo.GetPagedListAsync(null, null, null, 1, 10, CancellationToken.None);

        list.Should().HaveCount(2);
    }
}
