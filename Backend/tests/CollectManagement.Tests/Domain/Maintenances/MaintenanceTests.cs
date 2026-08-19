using CollectManagement.Domain.Devices.ValueObjects;
using CollectManagement.Domain.Employess.ObjectValues;
using CollectManagement.Domain.Maintenances;
using CollectManagement.Domain.Maintenances.ObjectValues;
using FluentAssertions;

namespace CollectManagement.Tests.Features.Domain.Maintenances;

public class MaintenanceTests
{

    private static Maintenance CreateMaintenance(
        MaintenanceId? id        = null,
        DeviceId?      deviceId  = null,
        EmployeeId?    empId     = null,
        string         desc      = "Test")
    {
        return Maintenance.Create(
            id       ?? new MaintenanceId(Ulid.NewUlid()),
            deviceId ?? new DeviceId(Ulid.NewUlid()),
            empId    ?? new EmployeeId(Ulid.NewUlid()),
            null, null, null, null, null, null,
            desc);
    }


    [Fact]
    public void Create_Should_Create_Maintenance()
    {

        var id       = new MaintenanceId(Ulid.NewUlid());
        var deviceId = new DeviceId(Ulid.NewUlid());
        var empId    = new EmployeeId(Ulid.NewUlid());


        var m = Maintenance.Create(
            id, deviceId, empId,
            null, null, null, null, null, null,
            "Inspection"
        );


        m.Should().NotBeNull();

        m.MaintenanceId.Should().Be(id);

        m.DeviceId.Should().Be(deviceId);

        m.EmployeeId.Should().Be(empId);

        m.Description.Should().Be("Inspection");

        m.IsCompleted.Should().BeFalse();
    }


    [Fact]
    public void ProcessRfidScan_Should_Advance_Steps_In_Order()
    {

        var m = CreateMaintenance();


        m.ProcessRfidScan().Should().Be("T1");

        m.ProcessRfidScan().Should().Be("T2");

        m.ProcessRfidScan().Should().Be("T3");

        m.ProcessRfidScan().Should().Be("T4");

        m.ProcessRfidScan().Should().Be("T5");

        m.ProcessRfidScan().Should().BeNull();
    }


    [Fact]
    public void IsCompleted_Should_Be_True_When_All_T_Steps_Set()
    {

        var m = CreateMaintenance();

        m.ProcessRfidScan(); // T1
        m.ProcessRfidScan(); // T2
        m.ProcessRfidScan(); // T3
        m.ProcessRfidScan(); // T4
        m.ProcessRfidScan(); // T5


        m.IsCompleted.Should().BeTrue();
    }


    [Fact]
    public void ReassignEmployee_Should_Change_Employee_And_Reset_T3_T4_T5()
    {

        var m = CreateMaintenance();

        m.ProcessRfidScan(); // T1
        m.ProcessRfidScan(); // T2
        m.ProcessRfidScan(); // T3
        m.ProcessRfidScan(); // T4

        var newEmpId = new EmployeeId(Ulid.NewUlid());


        m.ReassignEmployee(newEmpId);


        m.EmployeeId.Should().Be(newEmpId);

        m.T3Arrival.Should().BeNull();

        m.T4Completion.Should().BeNull();

        m.T5Confirmation.Should().BeNull();
    }


    [Fact]
    public void AutoComplete_Should_Fill_Missing_T_Steps()
    {

        var m         = CreateMaintenance();
        var completed = DateTime.UtcNow;

        m.ProcessRfidScan(); // T1
        m.ProcessRfidScan(); // T2


        m.AutoComplete(completed);


        m.T3Arrival.Should().Be(completed);

        m.T4Completion.Should().Be(completed);

        m.T5Confirmation.Should().Be(completed);

        m.IsCompleted.Should().BeTrue();
    }


    [Fact]
    public void SetT6NextAlert_Should_Set_T6()
    {

        var m    = CreateMaintenance();
        var t6   = DateTime.UtcNow.AddDays(7);


        m.SetT6NextAlert(t6);


        m.T6NextAlert.Should().Be(t6);
    }
}
