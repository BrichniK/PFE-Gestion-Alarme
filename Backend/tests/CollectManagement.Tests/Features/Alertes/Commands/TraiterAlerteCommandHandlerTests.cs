using CollectManagement.Application.Features.Alertes.Commands.TraiterAlerte;
using CollectManagement.Application.Interfaces.Employees;
using CollectManagement.Application.Interfaces.Repositories.Alertes;
using CollectManagement.Application.Interfaces.Repositories.Maintenances;
using CollectManagement.Application.Interfaces.Repositories.Plannings;
using CollectManagement.Application.Interfaces.Repositories.SMS;
using CollectManagement.Application.Interfaces.Repositories.SMSConfigurations;
using CollectManagement.Application.Interfaces.Services;
using CollectManagement.Domain.Alertes;
using CollectManagement.Domain.Alertes.ValueObjects;
using CollectManagement.Domain.Devices;
using CollectManagement.Domain.Devices.ValueObjects;
using CollectManagement.Domain.Employess;
using CollectManagement.Domain.Employess.ObjectValues;
using CollectManagement.Domain.Maintenances;
using CollectManagement.Domain.SMSConfigurations;
using CollectManagement.Domain.SMSConfigurations.ValueObjects;
using Moq;
using Xunit;

namespace CollectManagement.Tests.Features.Alertes.Commands;

public class TraiterAlerteCommandHandlerTests
{
    private readonly Mock<IAlerteRepository> _alerteRepository = new();
    private readonly Mock<IMaintenanceRepository> _maintenanceRepository = new();
    private readonly Mock<IEmployeeRepository> _employeeRepository = new();
    private readonly Mock<IMqttPublisher> _mqttPublisher = new();
    private readonly Mock<ISMSRepository> _smsRepository = new();
    private readonly Mock<ISMSService> _smsService = new();
    private readonly Mock<IPlanningRepository> _planningRepository = new();
    private readonly Mock<ISMSConfigurationRepository> _smsConfigurationRepository = new();
    private readonly Mock<IEmailService> _emailService = new();
    private readonly Mock<ISignalService> _signalService = new();

    // =========================================================
    // CREATE HANDLER
    // =========================================================

    private TraiterAlerteCommandHandler CreateHandler()
    {
        return new TraiterAlerteCommandHandler(
            _alerteRepository.Object,
            _maintenanceRepository.Object,
            _employeeRepository.Object,
            _mqttPublisher.Object,
            _smsRepository.Object,
            _smsService.Object,
            _planningRepository.Object,
            _smsConfigurationRepository.Object,
            _emailService.Object,
            _signalService.Object);
    }

    // =========================================================
    // TEST DATA
    // =========================================================

    private static Device CreateDevice(
        string deviceName = "Machine 001",
        string matricule = "MACHINE001")
    {
        return Device.Create(
            new DeviceId(Ulid.NewUlid()),
            deviceName,
            matricule,
            1);
    }

    private static Employee CreateEmployee(
        string? email = "employee@test.com",
        int phone = 22123456)
    {
        return Employee.Create(
            new EmployeeId(Ulid.NewUlid()),
            "Dupont",
            "Jean",
            phone,
            "RFID001",
            email,
            null);
    }

    private static Alerte CreateAlerte(
        Device device,
        DateTime? date = null)
    {
        return Alerte.Create(
            new AlerteId(Ulid.NewUlid()),
            date ?? DateTime.UtcNow,
            device.DeviceId,
            new CollectManagement.Domain.Types.ValueObjects.TypeId(
                Ulid.NewUlid()));
    }

    private static void SetAlerteDevice(
        Alerte alerte,
        Device device)
    {
        var property = typeof(Alerte)
            .GetProperty(nameof(Alerte.Dispositif));

        property!.SetValue(alerte, device);
    }

    // =========================================================
    // TEST 1
    // ALERTE NOT FOUND
    // =========================================================

    [Fact]
    public async Task Handle_ShouldReturnFalse_WhenAlerteDoesNotExist()
    {
        // Arrange
        var command = new TraiterAlerteCommand(
            Ulid.NewUlid(),
            Ulid.NewUlid());

        _alerteRepository
            .Setup(x => x.GetOneAsync(
                It.IsAny<AlerteId>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((Alerte)null!);

        var handler = CreateHandler();

        // Act
        var result = await handler.Handle(
            command,
            CancellationToken.None);

        // Assert
        Assert.False(result);

        _employeeRepository.Verify(
            x => x.GetOneAsync(
                It.IsAny<EmployeeId>(),
                It.IsAny<CancellationToken>()),
            Times.Never);

        _alerteRepository.Verify(
            x => x.UpdateBulkAsync(
                It.IsAny<Alerte>(),
                It.IsAny<CancellationToken>()),
            Times.Never);

        _maintenanceRepository.Verify(
            x => x.AddAsync(
                It.IsAny<Maintenance>(),
                It.IsAny<CancellationToken>()),
            Times.Never);

        _mqttPublisher.Verify(
            x => x.PublishAsync(
                It.IsAny<string>(),
                It.IsAny<object>()),
            Times.Never);

        _signalService.Verify(
            x => x.NotifyMaintenanceUpdated(),
            Times.Never);
    }

    // =========================================================
    // TEST 2
    // EMPLOYEE NOT FOUND
    // =========================================================

    [Fact]
    public async Task Handle_ShouldReturnFalse_WhenEmployeeDoesNotExist()
    {
        // Arrange
        var device = CreateDevice();
        var alerte = CreateAlerte(device);

        SetAlerteDevice(alerte, device);

        _alerteRepository
            .Setup(x => x.GetOneAsync(
                It.IsAny<AlerteId>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(alerte);

        _employeeRepository
            .Setup(x => x.GetOneAsync(
                It.IsAny<EmployeeId>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((Employee)null!);

        var command = new TraiterAlerteCommand(
            alerte.AlerteId.Value,
            Ulid.NewUlid());

        var handler = CreateHandler();

        // Act
        var result = await handler.Handle(
            command,
            CancellationToken.None);

        // Assert
        Assert.False(result);

        Assert.False(alerte.Traiter);

        _alerteRepository.Verify(
            x => x.UpdateBulkAsync(
                It.IsAny<Alerte>(),
                It.IsAny<CancellationToken>()),
            Times.Never);

        _maintenanceRepository.Verify(
            x => x.AddAsync(
                It.IsAny<Maintenance>(),
                It.IsAny<CancellationToken>()),
            Times.Never);

        _mqttPublisher.Verify(
            x => x.PublishAsync(
                It.IsAny<string>(),
                It.IsAny<object>()),
            Times.Never);

        _signalService.Verify(
            x => x.NotifyMaintenanceUpdated(),
            Times.Never);
    }

    // =========================================================
    // TEST 3
    // NORMAL PROCESSING + DATE NULL
    // =========================================================

    [Fact]
    public async Task Handle_ShouldProcessAlerte_WhenDateIsNull()
    {
        // Arrange
        var device = CreateDevice();

        var employee = CreateEmployee();

        var alerte = Alerte.Create(
            new AlerteId(Ulid.NewUlid()),
            null,
            device.DeviceId,
            new CollectManagement.Domain.Types.ValueObjects.TypeId(
                Ulid.NewUlid()));

        SetAlerteDevice(alerte, device);

        _alerteRepository
            .Setup(x => x.GetOneAsync(
                It.IsAny<AlerteId>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(alerte);

        _employeeRepository
            .Setup(x => x.GetOneAsync(
                It.IsAny<EmployeeId>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(employee);

        _smsConfigurationRepository
            .Setup(x => x.GetConfigurationAsync(
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((SMSConfiguration)null!);

        var command = new TraiterAlerteCommand(
            alerte.AlerteId.Value,
            employee.EmployeeId.Value);

        var handler = CreateHandler();

        // Act
        var result = await handler.Handle(
            command,
            CancellationToken.None);

        // Assert
        Assert.True(result);

        Assert.True(alerte.Traiter);

        // Alerte updated
        _alerteRepository.Verify(
            x => x.UpdateBulkAsync(
                alerte,
                It.IsAny<CancellationToken>()),
            Times.Once);

        // Maintenance created
        _maintenanceRepository.Verify(
            x => x.AddAsync(
                It.IsAny<Maintenance>(),
                It.IsAny<CancellationToken>()),
            Times.Once);

        // Date null => no previous maintenance lookup
        _maintenanceRepository.Verify(
            x => x.GetLatestByDeviceIdAsync(
                It.IsAny<DeviceId>(),
                It.IsAny<CancellationToken>()),
            Times.Never);

        // MQTT MUST be called because MACHINE001 exists
        _mqttPublisher.Verify(
            x => x.PublishAsync(
                "ALARME/PC/MACHINE001",
                It.IsAny<object>()),
            Times.Once);

        // SignalR notification
        _signalService.Verify(
            x => x.NotifyMaintenanceUpdated(),
            Times.Once);
    }

    // =========================================================
    // TEST 4
    // SMS DISABLED
    // =========================================================

    [Fact]
    public async Task Handle_ShouldNotSendSms_WhenSmsIsDisabled()
    {
        // Arrange
        var device = CreateDevice();

        var employee = CreateEmployee();

        var alerte = CreateAlerte(
            device,
            DateTime.UtcNow);

        SetAlerteDevice(alerte, device);

        var smsConfig = SMSConfiguration.Create(
            new SMSConfigurationId(Ulid.NewUlid()),
            "https://sms.test",
            false,
            5,
            10,
            true,
            true,
            true,
            true,
            true);

        _alerteRepository
            .Setup(x => x.GetOneAsync(
                It.IsAny<AlerteId>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(alerte);

        _employeeRepository
            .Setup(x => x.GetOneAsync(
                It.IsAny<EmployeeId>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(employee);

        _smsConfigurationRepository
            .Setup(x => x.GetConfigurationAsync(
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(smsConfig);

        _maintenanceRepository
            .Setup(x => x.GetLatestByDeviceIdAsync(
                It.IsAny<DeviceId>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((Maintenance)null!);

        var command = new TraiterAlerteCommand(
            alerte.AlerteId.Value,
            employee.EmployeeId.Value);

        var handler = CreateHandler();

        // Act
        var result = await handler.Handle(
            command,
            CancellationToken.None);

        // Assert
        Assert.True(result);

        _smsRepository.Verify(
            x => x.GetByDeviceIdAsync(
                It.IsAny<DeviceId>(),
                It.IsAny<CancellationToken>()),
            Times.Never);

        _smsService.Verify(
            x => x.SendSMSAsync(
                It.IsAny<List<string>>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()),
            Times.Never);

        _emailService.Verify(
            x => x.SendEmailAsync(
                It.IsAny<List<string>>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()),
            Times.Never);

        // MQTT still works independently from SMS
        _mqttPublisher.Verify(
            x => x.PublishAsync(
                "ALARME/PC/MACHINE001",
                It.IsAny<object>()),
            Times.Once);

        _signalService.Verify(
            x => x.NotifyMaintenanceUpdated(),
            Times.Once);
    }

    // =========================================================
    // TEST 5
    // EMPTY MATRICULE => NO MQTT
    // =========================================================

    [Fact]
    public async Task Handle_ShouldNotPublishMqtt_WhenMatriculeIsEmpty()
    {
        // Arrange
        var device = CreateDevice(
            "Machine 001",
            "");

        var employee = CreateEmployee();

        var alerte = CreateAlerte(device);

        SetAlerteDevice(alerte, device);

        _alerteRepository
            .Setup(x => x.GetOneAsync(
                It.IsAny<AlerteId>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(alerte);

        _employeeRepository
            .Setup(x => x.GetOneAsync(
                It.IsAny<EmployeeId>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(employee);

        _smsConfigurationRepository
            .Setup(x => x.GetConfigurationAsync(
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((SMSConfiguration)null!);

        _maintenanceRepository
            .Setup(x => x.GetLatestByDeviceIdAsync(
                It.IsAny<DeviceId>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((Maintenance)null!);

        var command = new TraiterAlerteCommand(
            alerte.AlerteId.Value,
            employee.EmployeeId.Value);

        var handler = CreateHandler();

        // Act
        var result = await handler.Handle(
            command,
            CancellationToken.None);

        // Assert
        Assert.True(result);

        // Empty matricule => MQTT MUST NOT be called
        _mqttPublisher.Verify(
            x => x.PublishAsync(
                It.IsAny<string>(),
                It.IsAny<object>()),
            Times.Never);

        _signalService.Verify(
            x => x.NotifyMaintenanceUpdated(),
            Times.Once);
    }

    // =========================================================
    // TEST 6
    // SMS CONFIGURATION NULL
    // =========================================================

    [Fact]
    public async Task Handle_ShouldNotSendSms_WhenSmsConfigurationDoesNotExist()
    {
        // Arrange
        var device = CreateDevice();
        var employee = CreateEmployee();
        var alerte = CreateAlerte(device);

        SetAlerteDevice(alerte, device);

        _alerteRepository
            .Setup(x => x.GetOneAsync(
                It.IsAny<AlerteId>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(alerte);

        _employeeRepository
            .Setup(x => x.GetOneAsync(
                It.IsAny<EmployeeId>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(employee);

        _smsConfigurationRepository
            .Setup(x => x.GetConfigurationAsync(
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((SMSConfiguration)null!);

        _maintenanceRepository
            .Setup(x => x.GetLatestByDeviceIdAsync(
                It.IsAny<DeviceId>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((Maintenance)null!);

        var command = new TraiterAlerteCommand(
            alerte.AlerteId.Value,
            employee.EmployeeId.Value);

        var handler = CreateHandler();

        // Act
        var result = await handler.Handle(
            command,
            CancellationToken.None);

        // Assert
        Assert.True(result);

        _smsRepository.Verify(
            x => x.GetByDeviceIdAsync(
                It.IsAny<DeviceId>(),
                It.IsAny<CancellationToken>()),
            Times.Never);

        _smsService.Verify(
            x => x.SendSMSAsync(
                It.IsAny<List<string>>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()),
            Times.Never);

        _emailService.Verify(
            x => x.SendEmailAsync(
                It.IsAny<List<string>>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()),
            Times.Never);

        // MQTT remains independent
        _mqttPublisher.Verify(
            x => x.PublishAsync(
                "ALARME/PC/MACHINE001",
                It.IsAny<object>()),
            Times.Once);
    }

    // =========================================================
    // TEST 7
    // DATE PRESENT + NO PREVIOUS MAINTENANCE
    // =========================================================

    [Fact]
    public async Task Handle_ShouldCreateMaintenance_WhenDateExistsAndNoPreviousMaintenance()
    {
        // Arrange
        var device = CreateDevice();
        var employee = CreateEmployee();

        var alertDate = new DateTime(
            2026,
            8,
            21,
            10,
            30,
            0,
            DateTimeKind.Utc);

        var alerte = CreateAlerte(
            device,
            alertDate);

        SetAlerteDevice(alerte, device);

        _alerteRepository
            .Setup(x => x.GetOneAsync(
                It.IsAny<AlerteId>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(alerte);

        _employeeRepository
            .Setup(x => x.GetOneAsync(
                It.IsAny<EmployeeId>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(employee);

        _maintenanceRepository
            .Setup(x => x.GetLatestByDeviceIdAsync(
                It.IsAny<DeviceId>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((Maintenance)null!);

        _smsConfigurationRepository
            .Setup(x => x.GetConfigurationAsync(
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((SMSConfiguration)null!);

        var command = new TraiterAlerteCommand(
            alerte.AlerteId.Value,
            employee.EmployeeId.Value);

        var handler = CreateHandler();

        // Act
        var result = await handler.Handle(
            command,
            CancellationToken.None);

        // Assert
        Assert.True(result);

        Assert.True(alerte.Traiter);

        _maintenanceRepository.Verify(
            x => x.GetLatestByDeviceIdAsync(
                device.DeviceId,
                It.IsAny<CancellationToken>()),
            Times.Once);

        _maintenanceRepository.Verify(
            x => x.AddAsync(
                It.IsAny<Maintenance>(),
                It.IsAny<CancellationToken>()),
            Times.Once);

        _maintenanceRepository.Verify(
            x => x.UpdateBulkAsync(
                It.IsAny<Maintenance>(),
                It.IsAny<CancellationToken>()),
            Times.Never);

        _mqttPublisher.Verify(
            x => x.PublishAsync(
                "ALARME/PC/MACHINE001",
                It.IsAny<object>()),
            Times.Once);

        _signalService.Verify(
            x => x.NotifyMaintenanceUpdated(),
            Times.Once);
    }
}