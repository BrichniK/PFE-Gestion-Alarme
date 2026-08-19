using CollectManagement.Domain.Shifts;
using CollectManagement.Domain.Shifts.ValueObjects;
using FluentAssertions;

namespace CollectManagement.Tests.Features.Domain.Shifts;

public class ShiftTests
{

    [Fact]
    public void Create_Should_Create_Shift()
    {

        var id        = new ShiftId(Ulid.NewUlid());
        var startTime = new TimeOnly(8, 0);
        var endTime   = new TimeOnly(16, 0);


        var shift = Shift.Create(
            id,
            "Matin",
            startTime,
            endTime
        );


        shift.Should().NotBeNull();

        shift.ShiftId.Should().Be(id);

        shift.Label.Should().Be("Matin");

        shift.StartTime.Should().Be(startTime);

        shift.EndTime.Should().Be(endTime);
    }


    [Fact]
    public void Update_Should_Modify_Shift()
    {

        var shift = Shift.Create(
            new ShiftId(Ulid.NewUlid()),
            "Matin",
            new TimeOnly(8, 0),
            new TimeOnly(16, 0)
        );


        shift.Update(
            "Nuit",
            new TimeOnly(22, 0),
            new TimeOnly(6, 0)
        );


        shift.Label.Should().Be("Nuit");

        shift.StartTime.Should().Be(new TimeOnly(22, 0));

        shift.EndTime.Should().Be(new TimeOnly(6, 0));
    }
}
