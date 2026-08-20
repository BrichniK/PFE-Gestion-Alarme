using CollectManagement.Domain.Devices;
using CollectManagement.Domain.Devices.ValueObjects;
using CollectManagement.Infrastructure.Persistence.Repositories.DeviceRepositories;
using FluentAssertions;

namespace CollectManagement.Tests.Integration;

public class DeviceRepositoryIntegrationTests
{
    [Fact]
    public async Task AddAsync_And_GetOneAsync_Should_Return_Device()
    {
        await using var ctx  = InMemoryDbHelper.CreateContext();
        var repo = new DeviceRepository(ctx);

        var id     = new DeviceId(Ulid.NewUlid());
        var device = Device.Create(id, "Machine-01", "MAT-001", 4);

        await repo.AddAsync(device, CancellationToken.None);
        await ctx.SaveChangesAsync();

        var result = await repo.GetOneAsync(id, CancellationToken.None);

        result.Should().NotBeNull();
        result.DeviceId.Should().Be(id);
        result.DeviceName.Should().Be("Machine-01");
    }

    [Fact]
    public async Task GetPagedListAsync_Should_Return_All_Devices()
    {
        await using var ctx  = InMemoryDbHelper.CreateContext();
        var repo = new DeviceRepository(ctx);

        await repo.AddAsync(Device.Create(new DeviceId(Ulid.NewUlid()), "Dev1", "MAT-001", 2), CancellationToken.None);
        await repo.AddAsync(Device.Create(new DeviceId(Ulid.NewUlid()), "Dev2", "MAT-002", 4), CancellationToken.None);
        await ctx.SaveChangesAsync();

        var (list, count) = await repo.GetPagedListAsync(null, null, null, 1, 10, CancellationToken.None);

        list.Should().HaveCount(2);
        count.Should().Be(2);
    }

    [Fact]
    public async Task GetPagedListAsync_With_Search_Should_Filter()
    {
        await using var ctx  = InMemoryDbHelper.CreateContext();
        var repo = new DeviceRepository(ctx);

        await repo.AddAsync(Device.Create(new DeviceId(Ulid.NewUlid()), "Pompe", "MAT-001", 2), CancellationToken.None);
        await repo.AddAsync(Device.Create(new DeviceId(Ulid.NewUlid()), "Moteur", "MAT-002", 4), CancellationToken.None);
        await ctx.SaveChangesAsync();

        var (list, count) = await repo.GetPagedListAsync("Pompe", null, null, 1, 10, CancellationToken.None);

        list.Should().HaveCount(1);
        list[0].DeviceName.Should().Be("Pompe");
    }

    [Fact]
    public async Task GetByMatriculeAsync_Should_Return_Device()
    {
        await using var ctx  = InMemoryDbHelper.CreateContext();
        var repo = new DeviceRepository(ctx);

        var device = Device.Create(new DeviceId(Ulid.NewUlid()), "Machine", "UNIQUE-MAT", 1);
        await repo.AddAsync(device, CancellationToken.None);
        await ctx.SaveChangesAsync();

        var result = await repo.GetByMatriculeAsync("UNIQUE-MAT", CancellationToken.None);

        result.Should().NotBeNull();
        result!.Matricule.Should().Be("UNIQUE-MAT");
    }

    [Fact]
    public async Task GetPagedListAsync_With_Pagination_Should_Return_Subset()
    {
        await using var ctx  = InMemoryDbHelper.CreateContext();
        var repo = new DeviceRepository(ctx);

        for (int i = 1; i <= 5; i++)
            await repo.AddAsync(Device.Create(new DeviceId(Ulid.NewUlid()), $"Device{i}", $"MAT-{i:D3}", 1), CancellationToken.None);
        await ctx.SaveChangesAsync();

        var (list, count) = await repo.GetPagedListAsync(null, null, null, 1, 2, CancellationToken.None);

        list.Should().HaveCount(2);
        count.Should().Be(5);
    }
}
