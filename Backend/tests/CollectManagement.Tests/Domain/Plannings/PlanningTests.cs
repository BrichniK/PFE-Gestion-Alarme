using CollectManagement.Domain.Devices.ValueObjects;
using CollectManagement.Domain.Employess.ObjectValues;
using CollectManagement.Domain.Groupes.ValueObjects;
using CollectManagement.Domain.Plannings;
using CollectManagement.Domain.Plannings.ValueObjects;
using CollectManagement.Domain.Shifts.ValueObjects;
using FluentAssertions;

namespace CollectManagement.Tests.Features.Domain.Plannings;

public class PlanningTests
{

    [Fact]
    public void Create_Should_Create_Planning()
    {

        var id         = new PlanningId(Ulid.NewUlid());
        var date       = new DateTime(2026, 9, 1);
        var groupeIds  = new[] { new GroupeId(Ulid.NewUlid()) };
        var deviceIds  = new[] { new DeviceId(Ulid.NewUlid()) };
        var shiftIds   = new[] { new ShiftId(Ulid.NewUlid()) };
        var empIds     = new[] { new EmployeeId(Ulid.NewUlid()) };


        var planning = Planning.Create(
            id, date,
            groupeIds, deviceIds, shiftIds, empIds
        );


        planning.Should().NotBeNull();

        planning.PlanningId.Should().Be(id);

        planning.Date.Should().Be(date);

        planning.PlanningGroupes.Should().HaveCount(1);

        planning.PlanningDevices.Should().HaveCount(1);

        planning.PlanningShifts.Should().HaveCount(1);

        planning.PlanningEmployees.Should().HaveCount(1);
    }


    [Fact]
    public void Update_Should_Replace_Relations()
    {

        var planning = Planning.Create(
            new PlanningId(Ulid.NewUlid()),
            new DateTime(2026, 9, 1),
            new[] { new GroupeId(Ulid.NewUlid()) },
            new[] { new DeviceId(Ulid.NewUlid()) },
            new[] { new ShiftId(Ulid.NewUlid()) },
            Array.Empty<EmployeeId>()
        );

        var newDate      = new DateTime(2026, 10, 1);
        var newGroupeIds = new[] { new GroupeId(Ulid.NewUlid()), new GroupeId(Ulid.NewUlid()) };
        var newDeviceIds = Array.Empty<DeviceId>();
        var newShiftIds  = new[] { new ShiftId(Ulid.NewUlid()) };
        var newEmpIds    = Array.Empty<EmployeeId>();


        planning.Update(newDate, newGroupeIds, newDeviceIds, newShiftIds, newEmpIds);


        planning.Date.Should().Be(newDate);

        planning.PlanningGroupes.Should().HaveCount(2);

        planning.PlanningDevices.Should().BeEmpty();
    }


    [Fact]
    public void Create_Should_Deduplicate_Ids()
    {

        var sharedId  = Ulid.NewUlid();
        var deviceIds = new[] { new DeviceId(sharedId), new DeviceId(sharedId) };


        var planning = Planning.Create(
            new PlanningId(Ulid.NewUlid()),
            DateTime.UtcNow,
            Array.Empty<GroupeId>(),
            deviceIds,
            Array.Empty<ShiftId>(),
            Array.Empty<EmployeeId>()
        );


        planning.PlanningDevices.Should().HaveCount(1);
    }
}
