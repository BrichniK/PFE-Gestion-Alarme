using CollectManagement.Domain.JoursFeries;
using CollectManagement.Domain.JoursFeries.ValueObjects;
using CollectManagement.Infrastructure.Persistence.Repositories.JourFerieRepositories;
using FluentAssertions;

namespace CollectManagement.Tests.Integration;

public class JourFerieRepositoryIntegrationTests
{
    [Fact]
    public async Task AddAsync_And_GetOneAsync_Should_Return_JourFerie()
    {
        await using var ctx = InMemoryDbHelper.CreateContext();
        var repo = new JourFerieRepository(ctx);

        var id  = new JourFerieId(Ulid.NewUlid());
        var jf  = JourFerie.Create(id, new DateTime(2026, 1, 14), "Révolution");

        await repo.AddAsync(jf, CancellationToken.None);
        await ctx.SaveChangesAsync();

        var result = await repo.GetOneAsync(id, CancellationToken.None);

        result.Should().NotBeNull();
        result.JourFerieId.Should().Be(id);
        result.Label.Should().Be("Révolution");
    }

    [Fact]
    public async Task GetPagedListAsync_Should_Return_All()
    {
        await using var ctx = InMemoryDbHelper.CreateContext();
        var repo = new JourFerieRepository(ctx);

        await repo.AddAsync(JourFerie.Create(new JourFerieId(Ulid.NewUlid()), new DateTime(2026, 1, 14), "Révolution"), CancellationToken.None);
        await repo.AddAsync(JourFerie.Create(new JourFerieId(Ulid.NewUlid()), new DateTime(2026, 3, 20), "Indépendance"), CancellationToken.None);
        await ctx.SaveChangesAsync();

        var (list, count) = await repo.GetPagedListAsync(null, null, null, 1, 10, CancellationToken.None);

        list.Should().HaveCount(2);
        count.Should().Be(2);
    }

    [Fact]
    public async Task GetPagedListAsync_With_Search_Should_Filter()
    {
        await using var ctx = InMemoryDbHelper.CreateContext();
        var repo = new JourFerieRepository(ctx);

        await repo.AddAsync(JourFerie.Create(new JourFerieId(Ulid.NewUlid()), new DateTime(2026, 1, 14), "Révolution"), CancellationToken.None);
        await repo.AddAsync(JourFerie.Create(new JourFerieId(Ulid.NewUlid()), new DateTime(2026, 3, 20), "Indépendance"), CancellationToken.None);
        await ctx.SaveChangesAsync();

        var (list, _) = await repo.GetPagedListAsync("Révolution", null, null, 1, 10, CancellationToken.None);

        list.Should().HaveCount(1);
        list[0].Label.Should().Be("Révolution");
    }

    [Fact]
    public async Task GetOneAsync_Should_Return_Null_When_Not_Found()
    {
        await using var ctx = InMemoryDbHelper.CreateContext();
        var repo = new JourFerieRepository(ctx);

        var result = await repo.GetOneAsync(new JourFerieId(Ulid.NewUlid()), CancellationToken.None);

        result.Should().BeNull();
    }
}
