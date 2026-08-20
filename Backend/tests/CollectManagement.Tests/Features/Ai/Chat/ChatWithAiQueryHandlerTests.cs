using CollectManagement.Application.Features.AI.Chat;
using CollectManagement.Application.Features.SensorMeasurements.Analysis;
using CollectManagement.Application.Interfaces.Repositories.Alertes;
using CollectManagement.Application.Interfaces.Repositories.Devices;
using CollectManagement.Application.Interfaces.Services;
using CollectManagement.Domain.Alertes;
using CollectManagement.Domain.Devices;
using FluentAssertions;
using MediatR;
using Moq;

namespace CollectManagement.Tests.Features.AI.Chat;

public class ChatWithAiQueryHandlerTests
{
    private readonly Mock<IDeviceRepository> _deviceRepository;
    private readonly Mock<IAlerteRepository> _alerteRepository;
    private readonly Mock<ISender> _sender;
    private readonly Mock<IAiService> _aiService;

    private readonly ChatWithAiQueryHandler _handler;

    public ChatWithAiQueryHandlerTests()
    {
        _deviceRepository = new Mock<IDeviceRepository>();
        _alerteRepository = new Mock<IAlerteRepository>();
        _sender = new Mock<ISender>();
        _aiService = new Mock<IAiService>();

        _handler = new ChatWithAiQueryHandler(
            _deviceRepository.Object,
            _alerteRepository.Object,
            _sender.Object,
            _aiService.Object);
    }

    [Fact]
    public async Task Handle_Should_Return_Message_When_Message_Is_Empty()
    {
        // Arrange
        var query = new ChatWithAiQuery("");

        // Act
        var result = await _handler.Handle(
            query,
            CancellationToken.None);

        // Assert
        result.Should().NotBeNull();

        result.Message
            .Should()
            .Be("Veuillez saisir une question.");

        _deviceRepository.Verify(
            x => x.GetByMatriculeAsync(
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()),
            Times.Never);

        _sender.Verify(
            x => x.Send(
                It.IsAny<GetSensorAnalysisQuery>(),
                It.IsAny<CancellationToken>()),
            Times.Never);

        _aiService.Verify(
            x => x.GenerateResponseAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_Should_Return_Message_When_Message_Contains_No_Device_Matricule()
    {
        // Arrange
        var query = new ChatWithAiQuery(
            "Quel est le risque de panne ?");

        // Act
        var result = await _handler.Handle(
            query,
            CancellationToken.None);

        // Assert
        result.Should().NotBeNull();

        result.Message.Should().Contain(
            "Veuillez préciser son matricule");

        result.DeviceMatricule.Should().BeNull();

        _deviceRepository.Verify(
            x => x.GetByMatriculeAsync(
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()),
            Times.Never);

        _sender.Verify(
            x => x.Send(
                It.IsAny<GetSensorAnalysisQuery>(),
                It.IsAny<CancellationToken>()),
            Times.Never);

        _aiService.Verify(
            x => x.GenerateResponseAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_Should_Return_Message_When_Device_Does_Not_Exist()
    {
        // Arrange
        var query = new ChatWithAiQuery(
            "Est-ce que MACHINE001 présente un risque de panne ?");

        _deviceRepository
            .Setup(x => x.GetByMatriculeAsync(
                "MACHINE001",
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((Device?)null);

        // Act
        var result = await _handler.Handle(
            query,
            CancellationToken.None);

        // Assert
        result.Should().NotBeNull();

        result.DeviceMatricule
            .Should()
            .Be("MACHINE001");

        result.Message.Should().Contain(
            "Aucun dispositif");

        result.Message.Should().Contain(
            "MACHINE001");

        _sender.Verify(
            x => x.Send(
                It.IsAny<GetSensorAnalysisQuery>(),
                It.IsAny<CancellationToken>()),
            Times.Never);

        _aiService.Verify(
            x => x.GenerateResponseAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_Should_Return_Ai_Response_When_Device_Exists()
    {
        // Arrange
        var deviceId = Ulid.NewUlid();

        // Device.Create(name, matricule, nombreCapteur)
        var device = Device.Create(
            new CollectManagement.Domain.Devices.ValueObjects.DeviceId(deviceId),
            "Machine Test",
            "MACHINE001",
            4);

        var analysis = CreateAnalysisResponse();

        var alerts = new Dictionary<string, Alerte>();

        _deviceRepository
            .Setup(x => x.GetByMatriculeAsync(
                "MACHINE001",
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(device);

        _sender
            .Setup(x => x.Send(
                It.Is<GetSensorAnalysisQuery>(
                    q => q.DeviceId == deviceId),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(analysis);

        _alerteRepository
            .Setup(x => x.GetLatestUnprocessedCaptureAlertsByDeviceAsync(
                device.DeviceId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(alerts);

        _aiService
            .Setup(x => x.GenerateResponseAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                "La machine présente un risque modéré.");

        var query = new ChatWithAiQuery(
            "Est-ce que MACHINE001 présente un risque de panne ?");

        // Act
        var result = await _handler.Handle(
            query,
            CancellationToken.None);

        // Assert
        result.Should().NotBeNull();

        result.Message
            .Should()
            .Be("La machine présente un risque modéré.");

        result.DeviceMatricule
            .Should()
            .Be("MACHINE001");

        result.DeviceName
            .Should()
            .Be("Machine Test");

        result.RiskLevel
            .Should()
            .Be("Moderate");

        result.GlobalTrend
            .Should()
            .Be("Degradation");

        result.FailureRate
            .Should()
            .Be(5);

        result.Recommendation
            .Should()
            .Contain("Surveiller");

        _deviceRepository.Verify(
            x => x.GetByMatriculeAsync(
                "MACHINE001",
                It.IsAny<CancellationToken>()),
            Times.Once);

        _sender.Verify(
            x => x.Send(
                It.Is<GetSensorAnalysisQuery>(
                    q => q.DeviceId == deviceId),
                It.IsAny<CancellationToken>()),
            Times.Once);

        _alerteRepository.Verify(
            x => x.GetLatestUnprocessedCaptureAlertsByDeviceAsync(
                device.DeviceId,
                It.IsAny<CancellationToken>()),
            Times.Once);

        _aiService.Verify(
            x => x.GenerateResponseAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_Should_Send_User_Message_To_Ai_Service()
    {
        // Arrange
        var deviceId = Ulid.NewUlid();

        var device = Device.Create(
            new CollectManagement.Domain.Devices.ValueObjects.DeviceId(deviceId),
            "Machine Test",
            "MACHINE001",
            1);

        var analysis = CreateAnalysisResponse();

        _deviceRepository
            .Setup(x => x.GetByMatriculeAsync(
                "MACHINE001",
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(device);

        _sender
            .Setup(x => x.Send(
                It.IsAny<GetSensorAnalysisQuery>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(analysis);

        _alerteRepository
            .Setup(x => x.GetLatestUnprocessedCaptureAlertsByDeviceAsync(
                device.DeviceId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                new Dictionary<string, Alerte>());

        _aiService
            .Setup(x => x.GenerateResponseAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync("Analyse terminée.");

        var question =
            "Est-ce que MACHINE001 est en bon état ?";

        var query = new ChatWithAiQuery(question);

        // Act
        await _handler.Handle(
            query,
            CancellationToken.None);

        // Assert
        _aiService.Verify(
            x => x.GenerateResponseAsync(
                question,
                It.Is<string>(context =>
                    context.Contains("MACHINE001") &&
                    context.Contains("Machine Test") &&
                    context.Contains("Moderate") &&
                    context.Contains("Degradation")),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_Should_Include_Analysis_Data_In_Ai_Context()
    {
        // Arrange
        var deviceId = Ulid.NewUlid();

        var device = Device.Create(
            new CollectManagement.Domain.Devices.ValueObjects.DeviceId(deviceId),
            "Machine Test",
            "MACHINE001",
            4);

        var analysis = CreateAnalysisResponse();

        _deviceRepository
            .Setup(x => x.GetByMatriculeAsync(
                "MACHINE001",
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(device);

        _sender
            .Setup(x => x.Send(
                It.IsAny<GetSensorAnalysisQuery>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(analysis);

        _alerteRepository
            .Setup(x => x.GetLatestUnprocessedCaptureAlertsByDeviceAsync(
                device.DeviceId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                new Dictionary<string, Alerte>());

        string? capturedContext = null;

        _aiService
            .Setup(x => x.GenerateResponseAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .Callback<string, string, CancellationToken>(
                (_, context, _) =>
                {
                    capturedContext = context;
                })
            .ReturnsAsync("Réponse IA");

        var query = new ChatWithAiQuery(
            "Analyse MACHINE001");

        // Act
        await _handler.Handle(
            query,
            CancellationToken.None);

        // Assert
        capturedContext.Should().NotBeNull();

        capturedContext.Should().Contain("MACHINE001");
        capturedContext.Should().Contain("Machine Test");

        capturedContext.Should().Contain(
            "Nombre de mesures analysées");

        capturedContext.Should().Contain("100");

        capturedContext.Should().Contain(
            "Nombre d'échecs");

        capturedContext.Should().Contain("5");

        capturedContext.Should().Contain(
            "Taux d'échec");

        capturedContext.Should().Contain("5,00 %");

        capturedContext.Should().Contain("Moderate");
        capturedContext.Should().Contain("Degradation");

        capturedContext.Should().Contain("TEMPÉRATURE");
        capturedContext.Should().Contain("VIBRATION");
        capturedContext.Should().Contain("PRESSION");
        capturedContext.Should().Contain("HUMIDITÉ");

        capturedContext.Should().Contain(
            "Aucune alerte non traitée récente");
    }

    [Fact]
    public async Task Handle_Should_Extract_Device_Matricule_With_Hyphen()
    {
        // Arrange
        var deviceId = Ulid.NewUlid();

        var device = Device.Create(
            new CollectManagement.Domain.Devices.ValueObjects.DeviceId(deviceId),
            "Device Test",
            "DEV-001",
            1);

        _deviceRepository
            .Setup(x => x.GetByMatriculeAsync(
                "DEV-001",
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(device);

        _sender
            .Setup(x => x.Send(
                It.IsAny<GetSensorAnalysisQuery>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateAnalysisResponse());

        _alerteRepository
            .Setup(x => x.GetLatestUnprocessedCaptureAlertsByDeviceAsync(
                device.DeviceId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                new Dictionary<string, Alerte>());

        _aiService
            .Setup(x => x.GenerateResponseAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync("Analyse");

        var query = new ChatWithAiQuery(
            "Analyse le dispositif DEV-001");

        // Act
        var result = await _handler.Handle(
            query,
            CancellationToken.None);

        // Assert
        result.DeviceMatricule
            .Should()
            .Be("DEV-001");

        _deviceRepository.Verify(
            x => x.GetByMatriculeAsync(
                "DEV-001",
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_Should_Extract_Device_Matricule_With_Underscore()
    {
        // Arrange
        var deviceId = Ulid.NewUlid();

        var device = Device.Create(
            new CollectManagement.Domain.Devices.ValueObjects.DeviceId(deviceId),
            "Device Test",
            "DEV_001",
            1);

        _deviceRepository
            .Setup(x => x.GetByMatriculeAsync(
                "DEV_001",
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(device);

        _sender
            .Setup(x => x.Send(
                It.IsAny<GetSensorAnalysisQuery>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateAnalysisResponse());

        _alerteRepository
            .Setup(x => x.GetLatestUnprocessedCaptureAlertsByDeviceAsync(
                device.DeviceId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                new Dictionary<string, Alerte>());

        _aiService
            .Setup(x => x.GenerateResponseAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync("Analyse");

        var query = new ChatWithAiQuery(
            "Analyse DEV_001");

        // Act
        var result = await _handler.Handle(
            query,
            CancellationToken.None);

        // Assert
        result.DeviceMatricule
            .Should()
            .Be("DEV_001");

        _deviceRepository.Verify(
            x => x.GetByMatriculeAsync(
                "DEV_001",
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_Should_Propagate_Ai_Response()
    {
        // Arrange
        var deviceId = Ulid.NewUlid();

        var device = Device.Create(
            new CollectManagement.Domain.Devices.ValueObjects.DeviceId(deviceId),
            "Machine Test",
            "MACHINE001",
            2);

        var expectedResponse =
            "La machine est actuellement stable.";

        _deviceRepository
            .Setup(x => x.GetByMatriculeAsync(
                "MACHINE001",
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(device);

        _sender
            .Setup(x => x.Send(
                It.IsAny<GetSensorAnalysisQuery>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateAnalysisResponse());

        _alerteRepository
            .Setup(x => x.GetLatestUnprocessedCaptureAlertsByDeviceAsync(
                device.DeviceId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                new Dictionary<string, Alerte>());

        _aiService
            .Setup(x => x.GenerateResponseAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        var query = new ChatWithAiQuery(
            "Que peux-tu me dire sur MACHINE001 ?");

        // Act
        var result = await _handler.Handle(
            query,
            CancellationToken.None);

        // Assert
        result.Message
            .Should()
            .Be(expectedResponse);

        result.DeviceMatricule
            .Should()
            .Be("MACHINE001");

        result.DeviceName
            .Should()
            .Be("Machine Test");
    }

    private static GetSensorAnalysisResponse CreateAnalysisResponse()
    {
        return new GetSensorAnalysisResponse
        {
            DeviceId = Ulid.NewUlid(),

            MeasurementCount = 100,

            FailureCount = 5,

            FailureRate = 5.0,

            Temperature = new SensorMetricAnalysis
            {
                Average = 75.5,
                Minimum = 60.0,
                Maximum = 90.0,
                HistoricalAverage = 70.0,
                RecentAverage = 78.0,
                VariationPercentage = 11.43,
                Trend = "Increasing"
            },

            Vibration = new SensorMetricAnalysis
            {
                Average = 3.5,
                Minimum = 1.0,
                Maximum = 6.0,
                HistoricalAverage = 3.0,
                RecentAverage = 3.8,
                VariationPercentage = 26.67,
                Trend = "Increasing"
            },

            Pressure = new SensorMetricAnalysis
            {
                Average = 1013.0,
                Minimum = 1000.0,
                Maximum = 1025.0,
                HistoricalAverage = 1013.0,
                RecentAverage = 1012.0,
                VariationPercentage = -0.10,
                Trend = "Stable"
            },

            Humidity = new SensorMetricAnalysis
            {
                Average = 60.0,
                Minimum = 45.0,
                Maximum = 75.0,
                HistoricalAverage = 60.0,
                RecentAverage = 59.0,
                VariationPercentage = -1.67,
                Trend = "Stable"
            },

            GlobalTrend = "Degradation",

            RiskLevel = "Moderate",

            Recommendation =
                "Risque modéré. Surveiller : température, vibration."
        };
    }
}