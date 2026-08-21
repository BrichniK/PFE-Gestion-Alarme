using CollectManagement.Domain.Devices;
using CollectManagement.Domain.Devices.ValueObjects;
using CollectManagement.Domain.Employess;
using CollectManagement.Domain.Employess.ObjectValues;
using CollectManagement.Domain.Groupes;
using CollectManagement.Domain.Groupes.ValueObjects;
using CollectManagement.Domain.Plannings;
using CollectManagement.Domain.Plannings.ValueObjects;
using CollectManagement.Domain.Shifts;
using CollectManagement.Domain.Shifts.ValueObjects;
using CollectManagement.Infrastructure.Persistence.Context;
using CollectManagement.Infrastructure.Persistence.Repositories.PlanningRepositories;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace CollectManagement.Tests.Infrastructure.Repositories;

public class PlanningRepositoryTests
{
    private static ApplicationDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new ApplicationDbContext(options);
    }

    private static PlanningRepository CreateRepository(ApplicationDbContext context)
    {
        return new PlanningRepository(context);
    }

    // ============================================================
    // GetOneAsync
    // ============================================================

    [Fact]
    public async Task GetOneAsync_Should_Return_Planning_When_Exists()
    {
        // Arrange
        await using var context = CreateContext();
        var repository = CreateRepository(context);

        var planningId = new PlanningId(Ulid.NewUlid());

        var groupeId = new GroupeId(Ulid.NewUlid());
        var deviceId = new DeviceId(Ulid.NewUlid());
        var shiftId = new ShiftId(Ulid.NewUlid());
        var employeeId = new EmployeeId(Ulid.NewUlid());

        var planning = Planning.Create(
            planningId,
            DateTime.UtcNow.Date,
            new[] { groupeId },
            new[] { deviceId },
            new[] { shiftId },
            new[] { employeeId });

        context.Set<Planning>().Add(planning);

        await context.SaveChangesAsync();

        // Act
        var result = await repository.GetOneAsync(
            planningId,
            CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.PlanningId.Should().Be(planningId);

        result.PlanningGroupes.Should().NotBeNull();
        result.PlanningDevices.Should().NotBeNull();
        result.PlanningShifts.Should().NotBeNull();
        result.PlanningEmployees.Should().NotBeNull();
    }

    [Fact]
    public async Task GetOneAsync_Should_Return_Null_When_Planning_Does_Not_Exist()
    {
        // Arrange
        await using var context = CreateContext();
        var repository = CreateRepository(context);

        var planningId = new PlanningId(Ulid.NewUlid());

        // Act
        var result = await repository.GetOneAsync(
            planningId,
            CancellationToken.None);

        // Assert
        result.Should().BeNull();
    }


    // ============================================================
    // GetPagedListAsync
    // ============================================================

    [Fact]
    public async Task GetPagedListAsync_Should_Return_All_Plannings_When_Search_Is_Empty()
    {
        // Arrange
        await using var context = CreateContext();
        var repository = CreateRepository(context);

        var planning1 = Planning.Create(
            new PlanningId(Ulid.NewUlid()),
            new DateTime(2026, 8, 1),
            Array.Empty<GroupeId>(),
            Array.Empty<DeviceId>(),
            Array.Empty<ShiftId>(),
            Array.Empty<EmployeeId>());

        var planning2 = Planning.Create(
            new PlanningId(Ulid.NewUlid()),
            new DateTime(2026, 8, 2),
            Array.Empty<GroupeId>(),
            Array.Empty<DeviceId>(),
            Array.Empty<ShiftId>(),
            Array.Empty<EmployeeId>());

        context.Set<Planning>().AddRange(planning1, planning2);

        await context.SaveChangesAsync();

        // Act
        var (items, count) = await repository.GetPagedListAsync(
            null,
            null,
            null,
            1,
            10,
            CancellationToken.None);

        // Assert
        count.Should().Be(2);
        items.Should().HaveCount(2);
    }


    [Fact]
    public async Task GetPagedListAsync_Should_Return_Correct_Page()
    {
        // Arrange
        await using var context = CreateContext();
        var repository = CreateRepository(context);

        for (var i = 1; i <= 5; i++)
        {
            var planning = Planning.Create(
                new PlanningId(Ulid.NewUlid()),
                new DateTime(2026, 8, i),
                Array.Empty<GroupeId>(),
                Array.Empty<DeviceId>(),
                Array.Empty<ShiftId>(),
                Array.Empty<EmployeeId>());

            context.Set<Planning>().Add(planning);
        }

        await context.SaveChangesAsync();

        // Act
        var (items, count) = await repository.GetPagedListAsync(
            null,
            null,
            null,
            2,
            2,
            CancellationToken.None);

        // Assert
        count.Should().Be(5);
        items.Should().HaveCount(2);
    }


    [Fact]
    public async Task GetPagedListAsync_Should_Order_By_Date_Ascending()
    {
        // Arrange
        await using var context = CreateContext();
        var repository = CreateRepository(context);

        var planning1 = Planning.Create(
            new PlanningId(Ulid.NewUlid()),
            new DateTime(2026, 8, 3),
            Array.Empty<GroupeId>(),
            Array.Empty<DeviceId>(),
            Array.Empty<ShiftId>(),
            Array.Empty<EmployeeId>());

        var planning2 = Planning.Create(
            new PlanningId(Ulid.NewUlid()),
            new DateTime(2026, 8, 1),
            Array.Empty<GroupeId>(),
            Array.Empty<DeviceId>(),
            Array.Empty<ShiftId>(),
            Array.Empty<EmployeeId>());

        var planning3 = Planning.Create(
            new PlanningId(Ulid.NewUlid()),
            new DateTime(2026, 8, 2),
            Array.Empty<GroupeId>(),
            Array.Empty<DeviceId>(),
            Array.Empty<ShiftId>(),
            Array.Empty<EmployeeId>());

        context.Set<Planning>().AddRange(
            planning1,
            planning2,
            planning3);

        await context.SaveChangesAsync();

        // Act
        var (items, count) = await repository.GetPagedListAsync(
            null,
            "Date",
            "asc",
            1,
            10,
            CancellationToken.None);

        // Assert
        count.Should().Be(3);

        items[0].Date.Should().Be(new DateTime(2026, 8, 1));
        items[1].Date.Should().Be(new DateTime(2026, 8, 2));
        items[2].Date.Should().Be(new DateTime(2026, 8, 3));
    }


    [Fact]
    public async Task GetPagedListAsync_Should_Order_By_Date_Descending()
    {
        // Arrange
        await using var context = CreateContext();
        var repository = CreateRepository(context);

        var planning1 = Planning.Create(
            new PlanningId(Ulid.NewUlid()),
            new DateTime(2026, 8, 1),
            Array.Empty<GroupeId>(),
            Array.Empty<DeviceId>(),
            Array.Empty<ShiftId>(),
            Array.Empty<EmployeeId>());

        var planning2 = Planning.Create(
            new PlanningId(Ulid.NewUlid()),
            new DateTime(2026, 8, 3),
            Array.Empty<GroupeId>(),
            Array.Empty<DeviceId>(),
            Array.Empty<ShiftId>(),
            Array.Empty<EmployeeId>());

        var planning3 = Planning.Create(
            new PlanningId(Ulid.NewUlid()),
            new DateTime(2026, 8, 2),
            Array.Empty<GroupeId>(),
            Array.Empty<DeviceId>(),
            Array.Empty<ShiftId>(),
            Array.Empty<EmployeeId>());

        context.Set<Planning>().AddRange(
            planning1,
            planning2,
            planning3);

        await context.SaveChangesAsync();

        // Act
        var (items, count) = await repository.GetPagedListAsync(
            null,
            "Date",
            "desc",
            1,
            10,
            CancellationToken.None);

        // Assert
        count.Should().Be(3);

        items[0].Date.Should().Be(new DateTime(2026, 8, 3));
        items[1].Date.Should().Be(new DateTime(2026, 8, 2));
        items[2].Date.Should().Be(new DateTime(2026, 8, 1));
    }


    [Fact]
    public async Task GetPagedListAsync_Should_Search_By_Device_Name()
    {
        // Arrange
        await using var context = CreateContext();
        var repository = CreateRepository(context);

        var deviceId = new DeviceId(Ulid.NewUlid());

        var device = Device.Create(
            deviceId,
            "Machine-Test",
            "MAT-001",
            4);

        context.Set<Device>().Add(device);

        var planning = Planning.Create(
            new PlanningId(Ulid.NewUlid()),
            DateTime.UtcNow.Date,
            Array.Empty<GroupeId>(),
            new[] { deviceId },
            Array.Empty<ShiftId>(),
            Array.Empty<EmployeeId>());

        context.Set<Planning>().Add(planning);

        await context.SaveChangesAsync();

        // Act
        var (items, count) = await repository.GetPagedListAsync(
            "Machine-Test",
            null,
            null,
            1,
            10,
            CancellationToken.None);

        // Assert
        count.Should().Be(1);
        items.Should().ContainSingle();
    }


    [Fact]
    public async Task GetPagedListAsync_Should_Search_By_Group_Name()
    {
        // Arrange
        await using var context = CreateContext();
        var repository = CreateRepository(context);

        var groupeId = new GroupeId(Ulid.NewUlid());

        var groupe = Groupe.Create(
            groupeId,
            "Equipe Maintenance",
            "#FF0000",
            new List<Ulid>());

        context.Set<Groupe>().Add(groupe);

        var planning = Planning.Create(
            new PlanningId(Ulid.NewUlid()),
            DateTime.UtcNow.Date,
            new[] { groupeId },
            Array.Empty<DeviceId>(),
            Array.Empty<ShiftId>(),
            Array.Empty<EmployeeId>());

        context.Set<Planning>().Add(planning);

        await context.SaveChangesAsync();

        // Act
        var (items, count) = await repository.GetPagedListAsync(
            "Equipe Maintenance",
            null,
            null,
            1,
            10,
            CancellationToken.None);

        // Assert
        count.Should().Be(1);
        items.Should().ContainSingle();
    }


    [Fact]
    public async Task GetPagedListAsync_Should_Search_By_Shift_Label()
    {
        // Arrange
        await using var context = CreateContext();
        var repository = CreateRepository(context);

        var shiftId = new ShiftId(Ulid.NewUlid());

        var shift = Shift.Create(
            shiftId,
            "Shift Matin",
            new TimeOnly(8, 0),
            new TimeOnly(16, 0));

        context.Set<Shift>().Add(shift);

        var planning = Planning.Create(
            new PlanningId(Ulid.NewUlid()),
            DateTime.UtcNow.Date,
            Array.Empty<GroupeId>(),
            Array.Empty<DeviceId>(),
            new[] { shiftId },
            Array.Empty<EmployeeId>());

        context.Set<Planning>().Add(planning);

        await context.SaveChangesAsync();

        // Act
        var (items, count) = await repository.GetPagedListAsync(
            "Shift Matin",
            null,
            null,
            1,
            10,
            CancellationToken.None);

        // Assert
        count.Should().Be(1);
        items.Should().ContainSingle();
    }


    [Fact]
    public async Task GetPagedListAsync_Should_Return_Empty_When_Search_Does_Not_Match()
    {
        // Arrange
        await using var context = CreateContext();
        var repository = CreateRepository(context);

        var planning = Planning.Create(
            new PlanningId(Ulid.NewUlid()),
            DateTime.UtcNow.Date,
            Array.Empty<GroupeId>(),
            Array.Empty<DeviceId>(),
            Array.Empty<ShiftId>(),
            Array.Empty<EmployeeId>());

        context.Set<Planning>().Add(planning);

        await context.SaveChangesAsync();

        // Act
        var (items, count) = await repository.GetPagedListAsync(
            "UNKNOWN",
            null,
            null,
            1,
            10,
            CancellationToken.None);

        // Assert
        count.Should().Be(0);
        items.Should().BeEmpty();
    }


    // ============================================================
    // GetEmployeesByDateAndDeviceAsync
    // ============================================================

    [Fact]
    public async Task GetEmployeesByDateAndDeviceAsync_Should_Return_Employees()
    {
        // Arrange
        await using var context = CreateContext();
        var repository = CreateRepository(context);

        var planningId = new PlanningId(Ulid.NewUlid());
        var groupeId = new GroupeId(Ulid.NewUlid());
        var deviceId = new DeviceId(Ulid.NewUlid());

        var employeeId1 = new EmployeeId(Ulid.NewUlid());
        var employeeId2 = new EmployeeId(Ulid.NewUlid());

        var employee1 = Employee.Create(
            employeeId1,
            "Dupont",
            "Jean",
            22111111,
            "RFID001",
            "jean@test.com",
            null);

        var employee2 = Employee.Create(
            employeeId2,
            "Martin",
            "Paul",
            22222222,
            "RFID002",
            "paul@test.com",
            null);

        context.Set<Employee>().AddRange(employee1, employee2);

        var groupe = Groupe.Create(
            groupeId,
            "Equipe A",
            "#000000",
            new List<Ulid>
            {
                employeeId1.Value,
                employeeId2.Value
            });

        context.Set<Groupe>().Add(groupe);

        var planning = Planning.Create(
            planningId,
            new DateTime(2026, 8, 21),
            new[] { groupeId },
            new[] { deviceId },
            Array.Empty<ShiftId>(),
            Array.Empty<EmployeeId>());

        context.Set<Planning>().Add(planning);

        await context.SaveChangesAsync();

        // Act
        var result =
            await repository.GetEmployeesByDateAndDeviceAsync(
                new DateTime(2026, 8, 21),
                deviceId,
                CancellationToken.None);

        // Assert
        result.Should().HaveCount(2);

        result.Should().Contain(x =>
            x.EmployeeId == employeeId1.Value);

        result.Should().Contain(x =>
            x.EmployeeId == employeeId2.Value);
    }


    [Fact]
    public async Task GetEmployeesByDateAndDeviceAsync_Should_Return_Empty_When_No_Employees()
    {
        // Arrange
        await using var context = CreateContext();
        var repository = CreateRepository(context);

        var groupeId = new GroupeId(Ulid.NewUlid());
        var deviceId = new DeviceId(Ulid.NewUlid());

        var groupe = Groupe.Create(
            groupeId,
            "Equipe Vide",
            "#000000",
            new List<Ulid>());

        context.Set<Groupe>().Add(groupe);

        var planning = Planning.Create(
            new PlanningId(Ulid.NewUlid()),
            new DateTime(2026, 8, 21),
            new[] { groupeId },
            new[] { deviceId },
            Array.Empty<ShiftId>(),
            Array.Empty<EmployeeId>());

        context.Set<Planning>().Add(planning);

        await context.SaveChangesAsync();

        // Act
        var result =
            await repository.GetEmployeesByDateAndDeviceAsync(
                new DateTime(2026, 8, 21),
                deviceId,
                CancellationToken.None);

        // Assert
        result.Should().BeEmpty();
    }


    [Fact]
    public async Task GetEmployeesByDateAndDeviceAsync_Should_Return_Empty_When_Date_Does_Not_Match()
    {
        // Arrange
        await using var context = CreateContext();
        var repository = CreateRepository(context);

        var groupeId = new GroupeId(Ulid.NewUlid());
        var deviceId = new DeviceId(Ulid.NewUlid());
        var employeeId = new EmployeeId(Ulid.NewUlid());

        var employee = Employee.Create(
            employeeId,
            "Test",
            "Employee",
            22111111,
            "RFID001",
            "test@test.com",
            null);

        context.Set<Employee>().Add(employee);

        var groupe = Groupe.Create(
            groupeId,
            "Equipe A",
            "#000000",
            new List<Ulid> { employeeId.Value });

        context.Set<Groupe>().Add(groupe);

        var planning = Planning.Create(
            new PlanningId(Ulid.NewUlid()),
            new DateTime(2026, 8, 20),
            new[] { groupeId },
            new[] { deviceId },
            Array.Empty<ShiftId>(),
            Array.Empty<EmployeeId>());

        context.Set<Planning>().Add(planning);

        await context.SaveChangesAsync();

        // Act
        var result =
            await repository.GetEmployeesByDateAndDeviceAsync(
                new DateTime(2026, 8, 21),
                deviceId,
                CancellationToken.None);

        // Assert
        result.Should().BeEmpty();
    }


    // ============================================================
    // GetGroupesWithEmployeesByDateAndDeviceAsync
    // ============================================================

    [Fact]
    public async Task GetGroupesWithEmployeesByDateAndDeviceAsync_Should_Return_Groups_With_Employees()
    {
        // Arrange
        await using var context = CreateContext();
        var repository = CreateRepository(context);

        var groupeId = new GroupeId(Ulid.NewUlid());
        var deviceId = new DeviceId(Ulid.NewUlid());
        var shiftId = new ShiftId(Ulid.NewUlid());

        var employeeId1 = new EmployeeId(Ulid.NewUlid());
        var employeeId2 = new EmployeeId(Ulid.NewUlid());

        var employee1 = Employee.Create(
            employeeId1,
            "Dupont",
            "Jean",
            22111111,
            "RFID001",
            "jean@test.com",
            null);

        var employee2 = Employee.Create(
            employeeId2,
            "Martin",
            "Paul",
            22222222,
            "RFID002",
            "paul@test.com",
            null);

        context.Set<Employee>().AddRange(employee1, employee2);

        var groupe = Groupe.Create(
            groupeId,
            "Maintenance",
            "#000000",
            new List<Ulid>
            {
                employeeId1.Value,
                employeeId2.Value
            });

        context.Set<Groupe>().Add(groupe);

        var shift = Shift.Create(
            shiftId,
            "Matin",
            new TimeOnly(8, 0),
            new TimeOnly(16, 0));

        context.Set<Shift>().Add(shift);

        var planning = Planning.Create(
            new PlanningId(Ulid.NewUlid()),
            new DateTime(2026, 8, 21),
            new[] { groupeId },
            new[] { deviceId },
            new[] { shiftId },
            Array.Empty<EmployeeId>());

        context.Set<Planning>().Add(planning);

        await context.SaveChangesAsync();

        // Act
        var result =
            await repository.GetGroupesWithEmployeesByDateAndDeviceAsync(
                new DateTime(2026, 8, 21),
                deviceId,
                CancellationToken.None);

        // Assert
        result.Should().HaveCount(1);

        var groupResult = result.First();

        groupResult.GroupeNom.Should().Be("Maintenance");
        groupResult.ShiftLabel.Should().Be("Matin");
        groupResult.ShiftStartTime.Should().Be("08:00");
        groupResult.ShiftEndTime.Should().Be("16:00");

        groupResult.Employees.Should().HaveCount(2);
    }


    [Fact]
    public async Task GetGroupesWithEmployeesByDateAndDeviceAsync_Should_Return_Empty_When_No_Planning()
    {
        // Arrange
        await using var context = CreateContext();
        var repository = CreateRepository(context);

        var deviceId = new DeviceId(Ulid.NewUlid());

        // Act
        var result =
            await repository.GetGroupesWithEmployeesByDateAndDeviceAsync(
                new DateTime(2026, 8, 21),
                deviceId,
                CancellationToken.None);

        // Assert
        result.Should().BeEmpty();
    }


    [Fact]
    public async Task GetGroupesWithEmployeesByDateAndDeviceAsync_Should_Handle_Group_With_No_Employees()
    {
        // Arrange
        await using var context = CreateContext();
        var repository = CreateRepository(context);

        var groupeId = new GroupeId(Ulid.NewUlid());
        var deviceId = new DeviceId(Ulid.NewUlid());
        var shiftId = new ShiftId(Ulid.NewUlid());

        var groupe = Groupe.Create(
            groupeId,
            "Groupe Vide",
            "#000000",
            new List<Ulid>());

        context.Set<Groupe>().Add(groupe);

        var shift = Shift.Create(
            shiftId,
            "Nuit",
            new TimeOnly(22, 0),
            new TimeOnly(6, 0));

        context.Set<Shift>().Add(shift);

        var planning = Planning.Create(
            new PlanningId(Ulid.NewUlid()),
            new DateTime(2026, 8, 21),
            new[] { groupeId },
            new[] { deviceId },
            new[] { shiftId },
            Array.Empty<EmployeeId>());

        context.Set<Planning>().Add(planning);

        await context.SaveChangesAsync();

        // Act
        var result =
            await repository.GetGroupesWithEmployeesByDateAndDeviceAsync(
                new DateTime(2026, 8, 21),
                deviceId,
                CancellationToken.None);

        // Assert
        result.Should().HaveCount(1);

        result[0].GroupeNom.Should().Be("Groupe Vide");
        result[0].Employees.Should().BeEmpty();
    }
}