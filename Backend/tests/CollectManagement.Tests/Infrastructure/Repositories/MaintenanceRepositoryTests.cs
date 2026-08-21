using CollectManagement.Domain.Devices;
using CollectManagement.Domain.Devices.ValueObjects;
using CollectManagement.Domain.Employess;
using CollectManagement.Domain.Employess.ObjectValues;
using CollectManagement.Domain.Maintenances;
using CollectManagement.Domain.Maintenances.ObjectValues;
using CollectManagement.Infrastructure.Persistence.Context;
using CollectManagement.Infrastructure.Persistence.Repositories.MaintenanceRepositories;
using FluentAssertions;

using Microsoft.EntityFrameworkCore;

namespace CollectManagement.Tests.Infrastructure.Repositories;

public class MaintenanceRepositoryTests : IDisposable
{

    private readonly ApplicationDbContext _context;
    private readonly MaintenanceRepository _repository;

    private readonly DeviceId _deviceId;
    private readonly EmployeeId _employeeId;

    public MaintenanceRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlServer(
                "Server=(localdb)\\MSSQLLocalDB;" +
                "Database=CollectManagementTests;" +
                "Trusted_Connection=True;" +
                "TrustServerCertificate=True;")
            .Options;

        _context = new ApplicationDbContext(options);

        _context.Database.EnsureDeleted();
        _context.Database.EnsureCreated();

        _repository = new MaintenanceRepository(_context);

        _deviceId = new DeviceId(Ulid.NewUlid());
        _employeeId = new EmployeeId(Ulid.NewUlid());
    }

    // ============================================================
    // HELPERS
    // ============================================================

    private Device CreateDevice(
        DeviceId? deviceId = null,
        string deviceName = "Machine Test",
        string matricule = "MACHINE001")
    {
        return Device.Create(
            deviceId ?? new DeviceId(Ulid.NewUlid()),
            deviceName,
            matricule,
            4);
    }

    private Employee CreateEmployee(
        EmployeeId? employeeId = null,
        string nom = "Dupont",
        string prenom = "Jean",
        string rfid = "RFID001")
    {
        return Employee.Create(
            employeeId ?? new EmployeeId(Ulid.NewUlid()),
            nom,
            prenom,
            22123456,
            rfid,
            "jean@test.com",
            null);
    }

    private Maintenance CreateMaintenance(
        DeviceId? deviceId = null,
        EmployeeId? employeeId = null,
        DateTime? t1 = null,
        DateTime? t2 = null,
        DateTime? t3 = null,
        DateTime? t4 = null,
        DateTime? t5 = null,
        DateTime? t6 = null,
        string description = "Maintenance test",
        DateTime? dateInsertion = null)
    {
        var maintenance = Maintenance.Create(
            new MaintenanceId(Ulid.NewUlid()),
            deviceId ?? _deviceId,
            employeeId ?? _employeeId,
            t1,
            t2,
            t3,
            t4,
            t5,
            t6,
            description);

        // Important pour les tests du repository :
        // MaintenanceRepository utilise DateInsertion
        // pour plusieurs recherches par date.
        maintenance.DateInsertion = dateInsertion ?? DateTime.UtcNow;

        return maintenance;
    }

    private async Task SeedBasicDataAsync()
    {
        var device = CreateDevice(_deviceId);
        var employee = CreateEmployee(_employeeId);

        _context.Devices.Add(device);
        _context.Set<Employee>().Add(employee);

        await _context.SaveChangesAsync();
    }

    private async Task AddMaintenanceAsync(Maintenance maintenance)
    {
        _context.Set<Maintenance>().Add(maintenance);
        await _context.SaveChangesAsync();

        // Nettoie le ChangeTracker afin que les requêtes du repository
        // utilisent réellement la base SQLite.
        _context.ChangeTracker.Clear();
    }

    // ============================================================
    // GetOneAsync
    // ============================================================

    [Fact]
    public async Task GetOneAsync_Should_Return_Maintenance()
    {
        await SeedBasicDataAsync();

        var maintenance = CreateMaintenance(
            description: "Inspection moteur");

        await AddMaintenanceAsync(maintenance);

        var result = await _repository.GetOneAsync(
            maintenance.MaintenanceId,
            CancellationToken.None);

        result.Should().NotBeNull();
        result.MaintenanceId.Should().Be(maintenance.MaintenanceId);
        result.Description.Should().Be("Inspection moteur");
        result.Device.Should().NotBeNull();
        result.Employee.Should().NotBeNull();
    }

    [Fact]
    public async Task GetOneAsync_Should_Return_Null_When_NotFound()
    {
        await SeedBasicDataAsync();

        var result = await _repository.GetOneAsync(
            new MaintenanceId(Ulid.NewUlid()),
            CancellationToken.None);

        result.Should().BeNull();
    }

    // ============================================================
    // GetLatestByDeviceIdAsync
    // ============================================================

    [Fact]
    public async Task GetLatestByDeviceIdAsync_Should_Return_Latest()
    {
        await SeedBasicDataAsync();

        var older = CreateMaintenance(
            t1: DateTime.UtcNow.AddHours(-2),
            description: "Ancienne maintenance");

        var latest = CreateMaintenance(
            t1: DateTime.UtcNow,
            description: "Dernière maintenance");

        await AddMaintenanceAsync(older);
        await AddMaintenanceAsync(latest);

        var result = await _repository.GetLatestByDeviceIdAsync(
            _deviceId,
            CancellationToken.None);

        result.Should().NotBeNull();
        result!.Description.Should().Be("Dernière maintenance");
    }

    [Fact]
    public async Task GetLatestByDeviceIdAsync_Should_Return_Null_When_NoMaintenance()
    {
        await SeedBasicDataAsync();

        var result = await _repository.GetLatestByDeviceIdAsync(
            new DeviceId(Ulid.NewUlid()),
            CancellationToken.None);

        result.Should().BeNull();
    }

    // ============================================================
    // GetActiveByEmployeeRfidAsync
    // ============================================================

    [Fact]
    public async Task GetActiveByEmployeeRfidAsync_Should_Return_Open_Maintenance()
    {
        await SeedBasicDataAsync();

        var maintenance = CreateMaintenance(
            t1: DateTime.UtcNow,
            t2: DateTime.UtcNow,
            t3: null,
            t4: null,
            t5: null,
            description: "Maintenance active");

        await AddMaintenanceAsync(maintenance);

        var result = await _repository.GetActiveByEmployeeRfidAsync(
            "RFID001",
            CancellationToken.None);

        result.Should().NotBeNull();
        result!.Description.Should().Be("Maintenance active");
        result.Employee.Rfid.Should().Be("RFID001");
        result.T5Confirmation.Should().BeNull();
    }

    [Fact]
    public async Task GetActiveByEmployeeRfidAsync_Should_Not_Return_Completed_Maintenance()
    {
        await SeedBasicDataAsync();

        var maintenance = CreateMaintenance(
            t1: DateTime.UtcNow,
            t2: DateTime.UtcNow,
            t3: DateTime.UtcNow,
            t4: DateTime.UtcNow,
            t5: DateTime.UtcNow,
            description: "Maintenance terminée");

        await AddMaintenanceAsync(maintenance);

        var result = await _repository.GetActiveByEmployeeRfidAsync(
            "RFID001",
            CancellationToken.None);

        result.Should().BeNull();
    }

    // ============================================================
    // GetLastByEmployeeRfidAndDeviceMatriculeAsync
    // ============================================================

    [Fact]
    public async Task GetLastByEmployeeRfidAndDeviceMatriculeAsync_Should_Return_Last()
    {
        await SeedBasicDataAsync();

        var older = CreateMaintenance(
            t1: DateTime.UtcNow.AddHours(-2),
            description: "Ancienne");

        var latest = CreateMaintenance(
            t1: DateTime.UtcNow,
            description: "Dernière");

        await AddMaintenanceAsync(older);
        await AddMaintenanceAsync(latest);

        var result =
            await _repository.GetLastByEmployeeRfidAndDeviceMatriculeAsync(
                "RFID001",
                "MACHINE001",
                CancellationToken.None);

        result.Should().NotBeNull();
        result!.Description.Should().Be("Dernière");
    }

    [Fact]
    public async Task GetLastByEmployeeRfidAndDeviceMatriculeAsync_Should_Return_Null_When_NotFound()
    {
        await SeedBasicDataAsync();

        var result =
            await _repository.GetLastByEmployeeRfidAndDeviceMatriculeAsync(
                "UNKNOWN",
                "UNKNOWN",
                CancellationToken.None);

        result.Should().BeNull();
    }

    // ============================================================
    // HasOpenMaintenanceForDeviceOnDateAsync
    // ============================================================

    [Fact]
    public async Task HasOpenMaintenanceForDeviceOnDateAsync_Should_Return_True()
    {
        await SeedBasicDataAsync();

        var maintenance = CreateMaintenance(
            t1: DateTime.UtcNow,
            description: "Maintenance ouverte");

        await AddMaintenanceAsync(maintenance);

        var result =
            await _repository.HasOpenMaintenanceForDeviceOnDateAsync(
                _deviceId,
                DateTime.UtcNow,
                CancellationToken.None);

        result.Should().BeTrue();
    }

    [Fact]
    public async Task HasOpenMaintenanceForDeviceOnDateAsync_Should_Return_False_When_Completed()
    {
        await SeedBasicDataAsync();

        var maintenance = CreateMaintenance(
            t1: DateTime.UtcNow,
            t2: DateTime.UtcNow,
            t3: DateTime.UtcNow,
            t4: DateTime.UtcNow,
            t5: DateTime.UtcNow,
            description: "Terminée");

        await AddMaintenanceAsync(maintenance);

        var result =
            await _repository.HasOpenMaintenanceForDeviceOnDateAsync(
                _deviceId,
                DateTime.UtcNow,
                CancellationToken.None);

        result.Should().BeFalse();
    }

    // ============================================================
    // GetOpenMaintenanceForDeviceOnDateAsync
    // ============================================================

    [Fact]
    public async Task GetOpenMaintenanceForDeviceOnDateAsync_Should_Return_Open()
    {
        await SeedBasicDataAsync();

        var maintenance = CreateMaintenance(
            t1: DateTime.UtcNow,
            description: "Ouverte aujourd'hui");

        await AddMaintenanceAsync(maintenance);

        var result =
            await _repository.GetOpenMaintenanceForDeviceOnDateAsync(
                _deviceId,
                DateTime.UtcNow,
                CancellationToken.None);

        result.Should().NotBeNull();
        result!.Description.Should().Be("Ouverte aujourd'hui");
        result.Device.Should().NotBeNull();
        result.Employee.Should().NotBeNull();
    }

    [Fact]
    public async Task GetOpenMaintenanceForDeviceOnDateAsync_Should_Return_Null_When_None()
    {
        await SeedBasicDataAsync();

        var result =
            await _repository.GetOpenMaintenanceForDeviceOnDateAsync(
                _deviceId,
                DateTime.UtcNow,
                CancellationToken.None);

        result.Should().BeNull();
    }

    // ============================================================
    // GetOpenMaintenancesByDeviceAndCaptureCodeAsync
    // ============================================================

    [Fact]
    public async Task GetOpenMaintenancesByDeviceAndCaptureCodeAsync_Should_Return_Matching()
    {
        await SeedBasicDataAsync();

        var maintenance = CreateMaintenance(
            t1: DateTime.UtcNow,
            description: "CAPTURE_CODE:A1");

        await AddMaintenanceAsync(maintenance);

        var result =
            await _repository.GetOpenMaintenancesByDeviceAndCaptureCodeAsync(
                _deviceId,
                "A1",
                CancellationToken.None);

        result.Should().HaveCount(1);
        result[0].Description.Should().Be("CAPTURE_CODE:A1");
    }

    [Fact]
    public async Task GetOpenMaintenancesByDeviceAndCaptureCodeAsync_Should_Not_Return_Completed()
    {
        await SeedBasicDataAsync();

        var maintenance = CreateMaintenance(
            t1: DateTime.UtcNow,
            t2: DateTime.UtcNow,
            t3: DateTime.UtcNow,
            t4: DateTime.UtcNow,
            t5: DateTime.UtcNow,
            description: "CAPTURE_CODE:A1");

        await AddMaintenanceAsync(maintenance);

        var result =
            await _repository.GetOpenMaintenancesByDeviceAndCaptureCodeAsync(
                _deviceId,
                "A1",
                CancellationToken.None);

        result.Should().BeEmpty();
    }

    // ============================================================
    // GetByDateRangeAsync
    // ============================================================

    [Fact]
    public async Task GetByDateRangeAsync_Should_Return_Maintenances_For_Device()
    {
        await SeedBasicDataAsync();

        var maintenance = CreateMaintenance(
            description: "Dans la période");

        await AddMaintenanceAsync(maintenance);

        var start = DateTime.UtcNow.AddDays(-1);
        var end = DateTime.UtcNow.AddDays(1);

        var result = await _repository.GetByDateRangeAsync(
            start,
            end,
            _deviceId.Value,
            CancellationToken.None);

        result.Should().NotBeEmpty();
        result.Should().Contain(x =>
            x.MaintenanceId == maintenance.MaintenanceId);
    }

    [Fact]
    public async Task GetByDateRangeAsync_Should_Return_All_When_Device_Is_Null()
    {
        await SeedBasicDataAsync();

        var maintenance = CreateMaintenance(
            description: "Dans période");

        await AddMaintenanceAsync(maintenance);

        var result = await _repository.GetByDateRangeAsync(
            DateTime.UtcNow.AddDays(-1),
            DateTime.UtcNow.AddDays(1),
            null,
            CancellationToken.None);

        result.Should().Contain(x =>
            x.MaintenanceId == maintenance.MaintenanceId);
    }

    // ============================================================
    // GetByDateRangeAsync1
    // ============================================================

    [Fact]
    public async Task GetByDateRangeAsync1_Should_Return_Maintenance()
    {
        await SeedBasicDataAsync();

        var now = DateTime.UtcNow;

        var maintenance = CreateMaintenance(
            t1: now,
            description: "Maintenance période");

        await AddMaintenanceAsync(maintenance);

        var result = await _repository.GetByDateRangeAsync1(
            now.AddDays(-1),
            now.AddDays(1),
            CancellationToken.None);

        result.Should().Contain(x =>
            x.MaintenanceId == maintenance.MaintenanceId);
    }

    // ============================================================
    // GetCompletedPagedListAsync
    // ============================================================

    [Fact]
    public async Task GetCompletedPagedListAsync_Should_Return_Completed()
    {
        await SeedBasicDataAsync();

        var now = DateTime.UtcNow;

        var maintenance = CreateMaintenance(
            t1: now.AddMinutes(-5),
            t2: now.AddMinutes(-4),
            t3: now.AddMinutes(-3),
            t4: now.AddMinutes(-2),
            t5: now.AddMinutes(-1),
            description: "Maintenance terminée");

        await AddMaintenanceAsync(maintenance);

        var result = await _repository.GetCompletedPagedListAsync(
            null,
            1,
            10,
            null,
            null,
            CancellationToken.None);

        result.Item2.Should().BeGreaterThanOrEqualTo(1);

        result.Item1.Should().Contain(x =>
            x.MaintenanceId == maintenance.MaintenanceId);
    }

    [Fact]
    public async Task GetCompletedPagedListAsync_Should_Filter_By_Search()
    {
        await SeedBasicDataAsync();

        var now = DateTime.UtcNow;

        var maintenance = CreateMaintenance(
            t1: now,
            t2: now,
            t3: now,
            t4: now,
            t5: now,
            description: "Réparation moteur");

        await AddMaintenanceAsync(maintenance);

        var result = await _repository.GetCompletedPagedListAsync(
            "Machine Test",
            1,
            10,
            null,
            null,
            CancellationToken.None);

        result.Item1.Should().Contain(x =>
            x.MaintenanceId == maintenance.MaintenanceId);
    }

    // ============================================================
    // GetPagedListAsync
    // ============================================================

    [Fact]
    public async Task GetPagedListAsync_Should_Return_Data()
    {
        await SeedBasicDataAsync();

        var now = DateTime.UtcNow;

        var maintenance = CreateMaintenance(
            t1: now,
            description: "Inspection moteur");

        await AddMaintenanceAsync(maintenance);

        var result = await _repository.GetPagedListAsync(
            null,
            null,
            null,
            1,
            10,
            "all",
            null,
            null,
            CancellationToken.None);

        result.Item2.Should().BeGreaterThanOrEqualTo(1);

        result.Item1.Should().Contain(x =>
            x.MaintenanceId == maintenance.MaintenanceId);
    }

    [Fact]
    public async Task GetPagedListAsync_Should_Filter_Affecte()
    {
        await SeedBasicDataAsync();

        var maintenance = CreateMaintenance(
            t1: null,
            t2: null,
            t3: null,
            t4: null,
            t5: null,
            description: "Affectée");

        await AddMaintenanceAsync(maintenance);

        var result = await _repository.GetPagedListAsync(
            null,
            null,
            null,
            1,
            10,
            "affecte",
            null,
            null,
            CancellationToken.None);

        result.Item1.Should().Contain(x =>
            x.MaintenanceId == maintenance.MaintenanceId);
    }

    [Fact]
    public async Task GetPagedListAsync_Should_Filter_Diagnostique()
    {
        await SeedBasicDataAsync();

        var maintenance = CreateMaintenance(
            t1: DateTime.UtcNow,
            t2: DateTime.UtcNow,
            t3: DateTime.UtcNow,
            t4: null,
            t5: null,
            description: "Diagnostic");

        await AddMaintenanceAsync(maintenance);

        var result = await _repository.GetPagedListAsync(
            null,
            null,
            null,
            1,
            10,
            "diagnostique",
            null,
            null,
            CancellationToken.None);

        result.Item1.Should().Contain(x =>
            x.MaintenanceId == maintenance.MaintenanceId);
    }

    [Fact]
    public async Task GetPagedListAsync_Should_Filter_Reparation()
    {
        await SeedBasicDataAsync();

        var maintenance = CreateMaintenance(
            t1: DateTime.UtcNow,
            t2: DateTime.UtcNow,
            t3: DateTime.UtcNow,
            t4: DateTime.UtcNow,
            t5: null,
            description: "Réparation");

        await AddMaintenanceAsync(maintenance);

        var result = await _repository.GetPagedListAsync(
            null,
            null,
            null,
            1,
            10,
            "reparation",
            null,
            null,
            CancellationToken.None);

        result.Item1.Should().Contain(x =>
            x.MaintenanceId == maintenance.MaintenanceId);
    }

    [Fact]
    public async Task GetPagedListAsync_Should_Filter_Done()
    {
        await SeedBasicDataAsync();

        var maintenance = CreateMaintenance(
            t1: DateTime.UtcNow,
            t2: DateTime.UtcNow,
            t3: DateTime.UtcNow,
            t4: DateTime.UtcNow,
            t5: DateTime.UtcNow,
            description: "Terminée");

        await AddMaintenanceAsync(maintenance);

        var result = await _repository.GetPagedListAsync(
            null,
            null,
            null,
            1,
            10,
            "done",
            null,
            null,
            CancellationToken.None);

        result.Item1.Should().Contain(x =>
            x.MaintenanceId == maintenance.MaintenanceId);
    }

    // ============================================================
    // UpdateBulkAsync
    // ============================================================

    [Fact]
    public async Task UpdateBulkAsync_Should_Update_Maintenance()
    {
        await SeedBasicDataAsync();

        var maintenance = CreateMaintenance(
            description: "Avant modification");

        await AddMaintenanceAsync(maintenance);

        maintenance.Update(
            _deviceId,
            _employeeId,
            DateTime.UtcNow,
            DateTime.UtcNow,
            DateTime.UtcNow,
            null,
            null,
            null,
            "Après modification");

        await _repository.UpdateBulkAsync(
            maintenance,
            CancellationToken.None);

        _context.ChangeTracker.Clear();

        var result = await _context.Set<Maintenance>()
            .FirstAsync(x =>
                x.MaintenanceId == maintenance.MaintenanceId);

        result.Description.Should().Be("Après modification");
        result.T1Alerte.Should().NotBeNull();
        result.T2Assignment.Should().NotBeNull();
        result.T3Arrival.Should().NotBeNull();
    }

    // ============================================================
    // IDisposable
    // ============================================================

    public void Dispose()
    {
        _context.Dispose();
    }
}