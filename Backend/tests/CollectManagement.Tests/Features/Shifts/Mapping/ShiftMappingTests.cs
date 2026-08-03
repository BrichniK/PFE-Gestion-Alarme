using CollectManagement.Application.Features.Shifts.Commands.CreateShift;
using CollectManagement.Application.Features.Shifts.Mapping;
using CollectManagement.Application.Features.Shifts.Queries.GetOneShift;
using CollectManagement.Domain.Shifts;
using CollectManagement.Domain.Shifts.ValueObjects;
using FluentAssertions;
using Mapster;

namespace CollectManagement.Tests.Features.Shifts.Mapping;

public class ShiftMappingTests
{
    private readonly TypeAdapterConfig _config;

    public ShiftMappingTests()
    {
        _config = new TypeAdapterConfig();
        _config.Scan(typeof(ShiftMapping).Assembly);
    }

    [Fact]
    public void ShouldMapShiftToCreateShiftResponse()
    {
        // Arrange
        var shiftId = new ShiftId(Ulid.NewUlid());
        var shift = Shift.Create(shiftId, "Matin", new TimeOnly(8, 0), new TimeOnly(16, 0));

        // Act
        var result = shift.Adapt<CreateShiftResponse>(_config);

        // Assert
        result.Should().NotBeNull();
        result.ShiftId.Should().Be(shiftId.Value);
    }

    [Fact]
    public void ShouldMapShiftToGetOneShiftResponse()
    {
        // Arrange
        var shiftId = new ShiftId(Ulid.NewUlid());
        var shift = Shift.Create(shiftId, "Matin", new TimeOnly(8, 0), new TimeOnly(16, 0));

        // Act
        var result = shift.Adapt<GetOneShiftResponse>(_config);

        // Assert
        result.Should().NotBeNull();
        result.ShiftId.Should().Be(shiftId.Value);
        result.Label.Should().Be("Matin");
        result.StartTime.Should().Be(new TimeOnly(8, 0));
        result.EndTime.Should().Be(new TimeOnly(16, 0));
    }
}
