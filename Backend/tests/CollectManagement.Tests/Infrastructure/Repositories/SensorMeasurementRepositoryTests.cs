using CollectManagement.Domain.Devices.ValueObjects;
using CollectManagement.Domain.SensorMeasurements;
using CollectManagement.Domain.SensorMeasurements.ValueObjects;
using CollectManagement.Infrastructure.Persistence.Context;
using CollectManagement.Infrastructure.Persistence.Repositories.SensorMeasurementRepositories;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace CollectManagement.Tests.Infrastructure.Persistence.Repositories;

public class SensorMeasurementRepositoryTests
{
    private static ApplicationDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new ApplicationDbContext(options);
    }

    private static SensorMeasurement CreateMeasurement(
        Ulid deviceId,
        string sensorCode,
        DateTime measuredAt,
        double? temperature = 25,
        double? vibration = 0.3,
        double? pressure = 1013,
        double? humidity = 60,
        bool isFailure = false)
    {
        return SensorMeasurement.Create(
            new SensorMeasurementId(Ulid.NewUlid()),
            new DeviceId(deviceId),
            sensorCode,
            measuredAt,
            temperature,
            vibration,
            pressure,
            humidity,
            isFailure);
    }

    // ============================================================
    // GetOneAsync
    // ============================================================

    [Fact]
    public async Task GetOneAsync_Should_Return_Measurement_When_It_Exists()
    {
        // Arrange
        await using var context = CreateContext();

        var repository =
            new SensorMeasurementRepository(context);

        var deviceId = Ulid.NewUlid();

        var measurement = CreateMeasurement(
            deviceId,
            "A1",
            DateTime.UtcNow);

        context.SensorMeasurements.Add(measurement);
        await context.SaveChangesAsync();

        // Act
        var result = await repository.GetOneAsync(
            measurement.SensorMeasurementId,
            CancellationToken.None);

        // Assert
        result.Should().NotBeNull();

        result!.SensorMeasurementId
            .Should()
            .Be(measurement.SensorMeasurementId);

        result.DeviceId.Value
            .Should()
            .Be(deviceId);

        result.SensorCode
            .Should()
            .Be("A1");
    }

    [Fact]
    public async Task GetOneAsync_Should_Return_Null_When_Measurement_Does_Not_Exist()
    {
        // Arrange
        await using var context = CreateContext();

        var repository =
            new SensorMeasurementRepository(context);

        var unknownId =
            new SensorMeasurementId(Ulid.NewUlid());

        // Act
        var result = await repository.GetOneAsync(
            unknownId,
            CancellationToken.None);

        // Assert
        result.Should().BeNull();
    }

    // ============================================================
    // GetForAnalysisAsync
    // ============================================================

    [Fact]
    public async Task GetForAnalysisAsync_Should_Return_Measurements_For_Device()
    {
        // Arrange
        await using var context = CreateContext();

        var repository =
            new SensorMeasurementRepository(context);

        var deviceId = Ulid.NewUlid();
        var otherDeviceId = Ulid.NewUlid();

        var first = CreateMeasurement(
            deviceId,
            "A1",
            new DateTime(
                2026,
                8,
                1,
                10,
                0,
                0,
                DateTimeKind.Utc));

        var second = CreateMeasurement(
            deviceId,
            "A1",
            new DateTime(
                2026,
                8,
                1,
                11,
                0,
                0,
                DateTimeKind.Utc));

        var otherDeviceMeasurement = CreateMeasurement(
            otherDeviceId,
            "A1",
            new DateTime(
                2026,
                8,
                1,
                12,
                0,
                0,
                DateTimeKind.Utc));

        context.SensorMeasurements.AddRange(
            first,
            second,
            otherDeviceMeasurement);

        await context.SaveChangesAsync();

        // Act
        var result = await repository.GetForAnalysisAsync(
            deviceId,
            null,
            CancellationToken.None);

        // Assert
        result.Should().HaveCount(2);

        result.Should().OnlyContain(
            x => x.DeviceId.Value == deviceId);

        result[0].SensorMeasurementId
            .Should()
            .Be(first.SensorMeasurementId);

        result[1].SensorMeasurementId
            .Should()
            .Be(second.SensorMeasurementId);
    }

    [Fact]
    public async Task GetForAnalysisAsync_Should_Filter_By_SensorCode()
    {
        // Arrange
        await using var context = CreateContext();

        var repository =
            new SensorMeasurementRepository(context);

        var deviceId = Ulid.NewUlid();

        var a1 = CreateMeasurement(
            deviceId,
            "A1",
            new DateTime(
                2026,
                8,
                1,
                10,
                0,
                0,
                DateTimeKind.Utc));

        var a2 = CreateMeasurement(
            deviceId,
            "A2",
            new DateTime(
                2026,
                8,
                1,
                11,
                0,
                0,
                DateTimeKind.Utc));

        var a1Second = CreateMeasurement(
            deviceId,
            "A1",
            new DateTime(
                2026,
                8,
                1,
                12,
                0,
                0,
                DateTimeKind.Utc));

        context.SensorMeasurements.AddRange(
            a1,
            a2,
            a1Second);

        await context.SaveChangesAsync();

        // Act
        var result = await repository.GetForAnalysisAsync(
            deviceId,
            "A1",
            CancellationToken.None);

        // Assert
        result.Should().HaveCount(2);

        result.Should()
            .OnlyContain(x => x.SensorCode == "A1");

        result[0].SensorMeasurementId
            .Should()
            .Be(a1.SensorMeasurementId);

        result[1].SensorMeasurementId
            .Should()
            .Be(a1Second.SensorMeasurementId);
    }

    [Fact]
    public async Task GetForAnalysisAsync_Should_Order_By_MeasuredAt_Ascending()
    {
        // Arrange
        await using var context = CreateContext();

        var repository =
            new SensorMeasurementRepository(context);

        var deviceId = Ulid.NewUlid();

        var late = CreateMeasurement(
            deviceId,
            "A1",
            new DateTime(
                2026,
                8,
                1,
                14,
                0,
                0,
                DateTimeKind.Utc));

        var early = CreateMeasurement(
            deviceId,
            "A1",
            new DateTime(
                2026,
                8,
                1,
                10,
                0,
                0,
                DateTimeKind.Utc));

        var middle = CreateMeasurement(
            deviceId,
            "A1",
            new DateTime(
                2026,
                8,
                1,
                12,
                0,
                0,
                DateTimeKind.Utc));

        context.SensorMeasurements.AddRange(
            late,
            early,
            middle);

        await context.SaveChangesAsync();

        // Act
        var result = await repository.GetForAnalysisAsync(
            deviceId,
            null,
            CancellationToken.None);

        // Assert
        result.Should().HaveCount(3);

        result[0].MeasuredAt
            .Should()
            .Be(early.MeasuredAt);

        result[1].MeasuredAt
            .Should()
            .Be(middle.MeasuredAt);

        result[2].MeasuredAt
            .Should()
            .Be(late.MeasuredAt);
    }

    [Fact]
    public async Task GetForAnalysisAsync_Should_Return_Empty_When_Device_Has_No_Measurements()
    {
        // Arrange
        await using var context = CreateContext();

        var repository =
            new SensorMeasurementRepository(context);

        var deviceId = Ulid.NewUlid();

        // Act
        var result = await repository.GetForAnalysisAsync(
            deviceId,
            null,
            CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    // ============================================================
    // GetPagedListAsync
    // ============================================================

    [Fact]
    public async Task GetPagedListAsync_Should_Return_All_Matching_Measurements()
    {
        // Arrange
        await using var context = CreateContext();

        var repository =
            new SensorMeasurementRepository(context);

        var deviceId = Ulid.NewUlid();

        for (var i = 0; i < 5; i++)
        {
            context.SensorMeasurements.Add(
                CreateMeasurement(
                    deviceId,
                    "A1",
                    new DateTime(
                        2026,
                        8,
                        1,
                        10,
                        i,
                        0,
                        DateTimeKind.Utc)));
        }

        await context.SaveChangesAsync();

        // Act
        var (measurements, count) =
            await repository.GetPagedListAsync(
                deviceId,
                null,
                null,
                null,
                1,
                10,
                CancellationToken.None);

        // Assert
        measurements.Should().HaveCount(5);

        count.Should().Be(5);

        measurements.Should()
            .OnlyContain(x => x.DeviceId.Value == deviceId);
    }

    [Fact]
    public async Task GetPagedListAsync_Should_Filter_By_DeviceId()
    {
        // Arrange
        await using var context = CreateContext();

        var repository =
            new SensorMeasurementRepository(context);

        var deviceId = Ulid.NewUlid();
        var otherDeviceId = Ulid.NewUlid();

        context.SensorMeasurements.AddRange(
            CreateMeasurement(
                deviceId,
                "A1",
                DateTime.UtcNow),

            CreateMeasurement(
                deviceId,
                "A1",
                DateTime.UtcNow.AddMinutes(-1)),

            CreateMeasurement(
                otherDeviceId,
                "A1",
                DateTime.UtcNow.AddMinutes(-2)));

        await context.SaveChangesAsync();

        // Act
        var (measurements, count) =
            await repository.GetPagedListAsync(
                deviceId,
                null,
                null,
                null,
                1,
                10,
                CancellationToken.None);

        // Assert
        measurements.Should().HaveCount(2);

        count.Should().Be(2);

        measurements.Should()
            .OnlyContain(x => x.DeviceId.Value == deviceId);
    }

    [Fact]
    public async Task GetPagedListAsync_Should_Filter_By_SensorCode_Using_Contains()
    {
        // Arrange
        await using var context = CreateContext();

        var repository =
            new SensorMeasurementRepository(context);

        var deviceId = Ulid.NewUlid();

        context.SensorMeasurements.AddRange(
            CreateMeasurement(
                deviceId,
                "A1",
                DateTime.UtcNow),

            CreateMeasurement(
                deviceId,
                "A10",
                DateTime.UtcNow.AddMinutes(-1)),

            CreateMeasurement(
                deviceId,
                "B1",
                DateTime.UtcNow.AddMinutes(-2)));

        await context.SaveChangesAsync();

        // Act
        var (measurements, count) =
            await repository.GetPagedListAsync(
                deviceId,
                "A1",
                null,
                null,
                1,
                10,
                CancellationToken.None);

        // Assert
        measurements.Should().HaveCount(2);

        count.Should().Be(2);

        measurements.Should()
            .OnlyContain(x => x.SensorCode.Contains("A1"));
    }

    [Fact]
    public async Task GetPagedListAsync_Should_Filter_By_Date_Range()
    {
        // Arrange
        await using var context = CreateContext();

        var repository =
            new SensorMeasurementRepository(context);

        var deviceId = Ulid.NewUlid();

        var before = new DateTime(
            2026,
            7,
            31,
            23,
            0,
            0,
            DateTimeKind.Utc);

        var start = new DateTime(
            2026,
            8,
            1,
            10,
            0,
            0,
            DateTimeKind.Utc);

        var middle = new DateTime(
            2026,
            8,
            5,
            10,
            0,
            0,
            DateTimeKind.Utc);

        var end = new DateTime(
            2026,
            8,
            10,
            10,
            0,
            0,
            DateTimeKind.Utc);

        var after = new DateTime(
            2026,
            8,
            11,
            10,
            0,
            0,
            DateTimeKind.Utc);

        context.SensorMeasurements.AddRange(
            CreateMeasurement(deviceId, "A1", before),
            CreateMeasurement(deviceId, "A1", start),
            CreateMeasurement(deviceId, "A1", middle),
            CreateMeasurement(deviceId, "A1", end),
            CreateMeasurement(deviceId, "A1", after));

        await context.SaveChangesAsync();

        // Act
        var (measurements, count) =
            await repository.GetPagedListAsync(
                deviceId,
                null,
                start,
                end,
                1,
                10,
                CancellationToken.None);

        // Assert
        measurements.Should().HaveCount(3);

        count.Should().Be(3);

        measurements.Should()
            .OnlyContain(
                x => x.MeasuredAt >= start &&
                     x.MeasuredAt <= end);
    }

    [Fact]
    public async Task GetPagedListAsync_Should_Order_By_MeasuredAt_Descending()
    {
        // Arrange
        await using var context = CreateContext();

        var repository =
            new SensorMeasurementRepository(context);

        var deviceId = Ulid.NewUlid();

        var first = CreateMeasurement(
            deviceId,
            "A1",
            new DateTime(
                2026,
                8,
                1,
                10,
                0,
                0,
                DateTimeKind.Utc));

        var second = CreateMeasurement(
            deviceId,
            "A1",
            new DateTime(
                2026,
                8,
                2,
                10,
                0,
                0,
                DateTimeKind.Utc));

        var third = CreateMeasurement(
            deviceId,
            "A1",
            new DateTime(
                2026,
                8,
                3,
                10,
                0,
                0,
                DateTimeKind.Utc));

        context.SensorMeasurements.AddRange(
            first,
            second,
            third);

        await context.SaveChangesAsync();

        // Act
        var (measurements, count) =
            await repository.GetPagedListAsync(
                deviceId,
                null,
                null,
                null,
                1,
                10,
                CancellationToken.None);

        // Assert
        count.Should().Be(3);

        measurements[0].MeasuredAt
            .Should()
            .Be(third.MeasuredAt);

        measurements[1].MeasuredAt
            .Should()
            .Be(second.MeasuredAt);

        measurements[2].MeasuredAt
            .Should()
            .Be(first.MeasuredAt);
    }

    [Fact]
    public async Task GetPagedListAsync_Should_Return_Correct_Page()
    {
        // Arrange
        await using var context = CreateContext();

        var repository =
            new SensorMeasurementRepository(context);

        var deviceId = Ulid.NewUlid();

        for (var i = 0; i < 25; i++)
        {
            context.SensorMeasurements.Add(
                CreateMeasurement(
                    deviceId,
                    "A1",
                    new DateTime(
                        2026,
                        8,
                        1,
                        0,
                        i,
                        0,
                        DateTimeKind.Utc)));
        }

        await context.SaveChangesAsync();

        // Act
        var (measurements, count) =
            await repository.GetPagedListAsync(
                deviceId,
                null,
                null,
                null,
                2,
                10,
                CancellationToken.None);

        // Assert
        count.Should().Be(25);

        measurements.Should().HaveCount(10);
    }

    [Fact]
    public async Task GetPagedListAsync_Should_Return_Last_Partial_Page()
    {
        // Arrange
        await using var context = CreateContext();

        var repository =
            new SensorMeasurementRepository(context);

        var deviceId = Ulid.NewUlid();

        for (var i = 0; i < 25; i++)
        {
            context.SensorMeasurements.Add(
                CreateMeasurement(
                    deviceId,
                    "A1",
                    new DateTime(
                        2026,
                        8,
                        1,
                        0,
                        i,
                        0,
                        DateTimeKind.Utc)));
        }

        await context.SaveChangesAsync();

        // Act
        var (measurements, count) =
            await repository.GetPagedListAsync(
                deviceId,
                null,
                null,
                null,
                3,
                10,
                CancellationToken.None);

        // Assert
        count.Should().Be(25);

        measurements.Should().HaveCount(5);
    }

    [Fact]
    public async Task GetPagedListAsync_Should_Work_Without_Optional_Filters()
    {
        // Arrange
        await using var context = CreateContext();

        var repository =
            new SensorMeasurementRepository(context);

        var device1 = Ulid.NewUlid();
        var device2 = Ulid.NewUlid();

        context.SensorMeasurements.AddRange(
            CreateMeasurement(
                device1,
                "A1",
                DateTime.UtcNow),

            CreateMeasurement(
                device2,
                "B1",
                DateTime.UtcNow.AddMinutes(-1)),

            CreateMeasurement(
                device1,
                "A2",
                DateTime.UtcNow.AddMinutes(-2)));

        await context.SaveChangesAsync();

        // Act
        var (measurements, count) =
            await repository.GetPagedListAsync(
                null,
                null,
                null,
                null,
                1,
                10,
                CancellationToken.None);

        // Assert
        measurements.Should().HaveCount(3);

        count.Should().Be(3);
    }
}