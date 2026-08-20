using System.Globalization;
using System.Text;
using CollectManagement.Application.Interfaces.Employees;
using CollectManagement.Application.Interfaces.Repositories.Alertes;
using CollectManagement.Application.Interfaces.Repositories.ConfigurationGenerales;
using CollectManagement.Application.Interfaces.Repositories.Devices;
using CollectManagement.Application.Interfaces.Repositories.Maintenances;
using CollectManagement.Application.Interfaces.Repositories.Plannings;
using CollectManagement.Application.Interfaces.Repositories.SMS;
using CollectManagement.Application.Interfaces.Repositories.SMSConfigurations;
using CollectManagement.Application.Interfaces.Repositories.SensorMeasurements;
using CollectManagement.Application.Interfaces.Repositories.Types;
using CollectManagement.Application.Interfaces.Services;
using CollectManagement.Application.Shared;
using CollectManagement.Domain.Alertes;
using CollectManagement.Domain.Alertes.ValueObjects;
using CollectManagement.Domain.Devices;
using CollectManagement.Domain.Devices.ValueObjects;
using CollectManagement.Domain.Maintenances;
using CollectManagement.Domain.Maintenances.ObjectValues;
using CollectManagement.Domain.SensorMeasurements;
using CollectManagement.Domain.SensorMeasurements.ValueObjects;
using Microsoft.Extensions.Logging;
using MQTTnet;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CollectManagement.Application.Handlers;

public class MqttMessageHandler : IMqttMessageHandler
{
    private readonly ILogger<MqttMessageHandler> _logger;
    private readonly IMaintenanceRepository _maintenanceRepository;
    private readonly IMaintenanceCaptureHistoryRepository _maintenanceCaptureHistoryRepository;
    private readonly ISignalService _signalService;
    private readonly IAlerteRepository _alerteRepository;
    private readonly IDeviceRepository _deviceRepository;
    private readonly IEmployeeRepository _employeeRepository;
    private readonly ITypeRepository _typeRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ISMSRepository _smsRepository;
    private readonly ISMSService _smsService;
    private readonly IMqttPublisher _mqttPublisher;
    private readonly IEmailService _emailService;
    private readonly IPlanningRepository _planningRepository;
    private readonly ISMSConfigurationRepository _smsConfigurationRepository;
    private readonly IConfigurationGeneraleRepository _configurationGeneraleRepository;
    private readonly ISensorMeasurementRepository _sensorMeasurementRepository;

    public MqttMessageHandler(
        ILogger<MqttMessageHandler> logger,
        IMaintenanceRepository maintenanceRepository,
        IMaintenanceCaptureHistoryRepository maintenanceCaptureHistoryRepository,
        ISignalService signalService,
        IAlerteRepository alerteRepository,
        IDeviceRepository deviceRepository,
        IEmployeeRepository employeeRepository,
        ITypeRepository typeRepository,
        IUnitOfWork unitOfWork,
        ISMSRepository smsRepository,
        ISMSService smsService,
        IMqttPublisher mqttPublisher,
        IEmailService emailService,
        IPlanningRepository planningRepository,
        ISMSConfigurationRepository smsConfigurationRepository,
        ISensorMeasurementRepository sensorMeasurementRepository,
        IConfigurationGeneraleRepository configurationGeneraleRepository)
    {
        _logger = logger;
        _maintenanceRepository = maintenanceRepository;
        _maintenanceCaptureHistoryRepository = maintenanceCaptureHistoryRepository;
        _signalService = signalService;
        _alerteRepository = alerteRepository;
        _deviceRepository = deviceRepository;
        _employeeRepository = employeeRepository;
        _typeRepository = typeRepository;
        _unitOfWork = unitOfWork;
        _smsRepository = smsRepository;
        _smsService = smsService;
        _mqttPublisher = mqttPublisher;
        _emailService = emailService;
        _planningRepository = planningRepository;
        _smsConfigurationRepository = smsConfigurationRepository;
        _sensorMeasurementRepository = sensorMeasurementRepository;
        _configurationGeneraleRepository = configurationGeneraleRepository;
    }

    // ============================================================
    // MAIN MQTT ROUTER
    // ============================================================

    public async Task HandleMessageAsync(
        MqttApplicationMessageReceivedEventArgs sender)
    {
        try
        {
            var payload = Encoding.UTF8.GetString(
                sender.ApplicationMessage.Payload);

            var topic = sender.ApplicationMessage.Topic;

            _logger.LogInformation(
                "MQTT Message received - Topic: {Topic}, Payload: {Payload}",
                topic,
                payload);

            var topicParts = topic.Split(
                '/',
                StringSplitOptions.RemoveEmptyEntries);

            if (topicParts.Length < 3)
            {
                _logger.LogWarning(
                    "Unexpected topic format: {Topic}",
                    topic);

                return;
            }

            var section = topicParts[0].Trim().ToUpperInvariant();
            var subSection = topicParts[1].Trim().ToUpperInvariant();
            var deviceNumber = topicParts[2].Trim();

            // --------------------------------------------------------
            // ALARME/EMP/{MATRICULE}
            // --------------------------------------------------------

            if (section == "ALARME" &&
                subSection == "EMP")
            {
                await HandleAlarmeEmpMessageAsync(
                    payload,
                    deviceNumber);

                return;
            }

            // --------------------------------------------------------
            // ALARME/A/{MATRICULE}
            // Classical alarm message
            // --------------------------------------------------------

            if (section == "ALARME" &&
                subSection == "A")
            {
                await HandleAlarmeAMessageAsync(
                    payload,
                    deviceNumber);

                return;
            }

            // --------------------------------------------------------
            // ALARME/W/{MATRICULE}
            // Device online/offline
            // --------------------------------------------------------

            if (section == "ALARME" &&
                subSection == "W")
            {
                await HandleAlarmeWMessageAsync(
                    payload,
                    deviceNumber);

                return;
            }

            // --------------------------------------------------------
            // ALARME/RESET/{MATRICULE}
            // --------------------------------------------------------

            if (section == "ALARME" &&
                subSection == "RESET")
            {
                await HandleAlarmeResetMessageAsync(
                    deviceNumber);

                return;
            }

            // --------------------------------------------------------
            // ALARME/A1/{MATRICULE}
            // ALARME/A2/{MATRICULE}
            // ALARME/A3/{MATRICULE}
            // ...
            //
            // Sensor measurements
            // --------------------------------------------------------

            if (section == "ALARME" &&
                IsSensorTopic(subSection))
            {
                await HandleSensorMeasurementMessageAsync(
                    payload,
                    deviceNumber,
                    subSection);

                return;
            }

            _logger.LogWarning(
                "Unhandled MQTT topic: {Topic}",
                topic);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error processing MQTT message");
        }
    }

    // ============================================================
    // SENSOR TOPIC DETECTION
    // ============================================================

    private static bool IsSensorTopic(string subSection)
    {
        if (string.IsNullOrWhiteSpace(subSection))
        {
            return false;
        }

        if (!subSection.StartsWith(
                "A",
                StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        if (subSection.Length <= 1)
        {
            return false;
        }

        return int.TryParse(
                   subSection[1..],
                   NumberStyles.None,
                   CultureInfo.InvariantCulture,
                   out var sensorNumber)
               && sensorNumber > 0;
    }

    // ============================================================
    // SENSOR MEASUREMENT
    // ============================================================

    private async Task HandleSensorMeasurementMessageAsync(
        string payload,
        string deviceMatricule,
        string sensorCode)
    {
        try
        {
            _logger.LogInformation(
                "Processing sensor measurement - Device: {Device}, Sensor: {Sensor}",
                deviceMatricule,
                sensorCode);

            JObject json;

            try
            {
                json = JObject.Parse(payload);
            }
            catch (JsonReaderException ex)
            {
                _logger.LogError(
                    ex,
                    "Invalid JSON sensor measurement payload: {Payload}",
                    payload);

                return;
            }

            var temperature =
                json["temperature"]?.Value<double?>();

            var vibration =
                json["vibration"]?.Value<double?>();

            var pressure =
                json["pressure"]?.Value<double?>();

            var humidity =
                json["humidity"]?.Value<double?>();

            var status =
                json["status"]?.Value<string>();

            var isFailure =
                string.Equals(
                    status,
                    "FAILURE",
                    StringComparison.OrdinalIgnoreCase)
                ||
                string.Equals(
                    status,
                    "CRITICAL",
                    StringComparison.OrdinalIgnoreCase);

            // --------------------------------------------------------
            // Find device
            // --------------------------------------------------------

            var device =
                await _deviceRepository.GetByMatriculeAsync(
                    deviceMatricule,
                    default);

            if (device == null)
            {
                _logger.LogWarning(
                    "No Device found with Matricule '{Matricule}'",
                    deviceMatricule);

                return;
            }

            // --------------------------------------------------------
            // Create SensorMeasurement
            // --------------------------------------------------------

            var measurement =
                SensorMeasurement.Create(
                    new SensorMeasurementId(
                        Ulid.NewUlid()),

                    device.DeviceId,

                    sensorCode,

                    DateTime.UtcNow,

                    temperature,
                    vibration,
                    pressure,
                    humidity,

                    isFailure);

            // --------------------------------------------------------
            // Save
            // --------------------------------------------------------

            await _sensorMeasurementRepository.AddAsync(
                measurement,
                default);

            await _unitOfWork.SaveChangesAsync(
                default);

            _logger.LogInformation(
                "Sensor measurement saved successfully - " +
                "Device: {Device}, Sensor: {Sensor}, " +
                "Temperature: {Temperature}, Vibration: {Vibration}, " +
                "Pressure: {Pressure}, Humidity: {Humidity}, " +
                "Failure: {IsFailure}",
                deviceMatricule,
                sensorCode,
                temperature,
                vibration,
                pressure,
                humidity,
                isFailure);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error processing sensor measurement from device {Device}",
                deviceMatricule);
        }
    }

    // ============================================================
    // ALARME/A
    // ============================================================

    private async Task HandleAlarmeAMessageAsync(
        string payload,
        string deviceMatricule)
    {
        var settings = new JsonSerializerSettings
        {
            Error = (_, e) =>
                e.ErrorContext.Handled = true
        };

        var message =
            JsonConvert.DeserializeObject<AlarmeAMessage>(
                payload,
                settings);

        if (message == null ||
            message.A == null)
        {
            _logger.LogWarning(
                "Invalid JSON payload for ALARME/A: {Payload}",
                payload);

            return;
        }

        var di = message.DI.ToString();

        if (string.IsNullOrWhiteSpace(di))
        {
            _logger.LogWarning(
                "DI (Matricule) missing in ALARME/A payload");

            return;
        }

        var device =
            await _deviceRepository.GetByMatriculeAsync(
                di,
                default);

        if (device == null)
        {
            _logger.LogWarning(
                "No Device found with Matricule '{DI}'",
                di);

            return;
        }

        var date =
            DateTimeOffset
                .FromUnixTimeSeconds(message.TI)
                .DateTime;

        var alertStates = message.A;

        var durations =
            message.Dur ?? new List<long>();

        if (alertStates.Count == 0)
        {
            _logger.LogWarning(
                "A array missing or empty in ALARME/A payload for Device '{DI}'",
                di);

            return;
        }

        var typeCodes =
            ResolveAlarmeTypeCodes(
                message.Type,
                alertStates.Count);

        var changed = false;

        for (var i = 0;
             i < alertStates.Count;
             i++)
        {
            var typeCode = typeCodes[i];

            var isActive = alertStates[i];

            if (string.IsNullOrWhiteSpace(typeCode))
            {
                continue;
            }

            if (isActive)
            {
                var alertDate = date;

                if (i < durations.Count &&
                    durations[i] >= 0)
                {
                    var alertStartUnix =
                        Math.Max(
                            0,
                            message.TI - durations[i]);

                    alertDate =
                        DateTimeOffset
                            .FromUnixTimeSeconds(
                                alertStartUnix)
                            .DateTime;
                }

                await HandleAlertActiveAsync(
                    device,
                    typeCode,
                    alertDate,
                    di);
            }
            else
            {
                await HandleAlertResolvedAsync(
                    device,
                    typeCode,
                    date,
                    di);
            }

            changed = true;
        }

        if (changed)
        {
            await _signalService
                .NotifyMaintenanceUpdated();
        }
    }

    // ============================================================
    // ALARME TYPE RESOLUTION
    // ============================================================

    private static List<string> ResolveAlarmeTypeCodes(
        JToken? typeToken,
        int alertCount)
    {
        var codes = new List<string>();

        if (typeToken is JArray typeArray)
        {
            foreach (var item in typeArray)
            {
                var code =
                    item?
                        .ToString()?
                        .Trim();

                if (!string.IsNullOrWhiteSpace(code))
                {
                    codes.Add(code);
                }
            }
        }
        else
        {
            var code =
                typeToken?
                    .ToString()?
                    .Trim();

            if (!string.IsNullOrWhiteSpace(code))
            {
                codes.Add(code);
            }
        }

        while (codes.Count < alertCount)
        {
            codes.Add(
                $"A{codes.Count + 1}");
        }

        return codes;
    }

    // ============================================================
    // ALERT ACTIVE
    // ============================================================

    private async Task HandleAlertActiveAsync(
        Device device,
        string typeCode,
        DateTime date,
        string di)
    {
        var type =
            await _typeRepository.GetByCodeAsync(
                typeCode,
                default);

        if (type == null)
        {
            _logger.LogWarning(
                "No Type found with Code '{TypeCode}'",
                typeCode);

            return;
        }

        var existingAlert =
            await _alerteRepository
                .GetLatestCaptureAlertByDeviceAndCodeAsync(
                    device.DeviceId,
                    typeCode,
                    default);

        if (existingAlert != null &&
            !existingAlert.Traiter)
        {
            _logger.LogInformation(
                "Duplicate ALARME/A skipped - " +
                "Unprocessed alert already exists for Device '{DeviceName}', Type '{Type}'",
                device.DeviceName,
                typeCode);

            return;
        }

        var alerteId =
            new AlerteId(
                Ulid.NewUlid());

        var alerte =
            Alerte.Create(
                alerteId,
                date,
                device.DeviceId,
                type.TypeId,
                false);

        await _alerteRepository.AddAsync(
            alerte,
            default);

        await _unitOfWork.SaveChangesAsync(
            default);

        _logger.LogInformation(
            "Alerte {AlerteId} created - Device: {DeviceName}, Type: {TypeCode}",
            alerteId.Value,
            device.DeviceName,
            typeCode);

        var typeLabelOrCode =
            type.Label ??
            type.Code ??
            typeCode;

        var smsText =
            string.Join(
                "\n",
                new[]
                {
                    date.ToString(
                        "dd/MM/yyyy HH:mm:ss",
                        CultureInfo.InvariantCulture),

                    "Alerte sur equipement",

                    device.DeviceName,

                    typeLabelOrCode
                });

        await SendSMSForDeviceAsync(
            device.DeviceId,
            device.DeviceName,
            di,
            smsText,
            "alerte",
            default);

        if (TryGetCaptureIndex(
                typeCode,
                out _))
        {
            var state =
                await BuildDeviceCaptureStatePayloadAsync(
                    device.DeviceId,
                    $"ALARM_{typeCode}");

            await _signalService
                .NotifyDeviceCaptureStateChanged(
                    state);
        }
    }

    // ============================================================
    // ALERT RESOLVED
    // ============================================================

    private async Task HandleAlertResolvedAsync(
        Device device,
        string typeCode,
        DateTime resolvedAt,
        string di)
    {
        var openMaintenances =
            await _maintenanceRepository
                .GetOpenMaintenancesByDeviceAndCaptureCodeAsync(
                    device.DeviceId,
                    typeCode,
                    default);

        foreach (var maintenance in openMaintenances)
        {
            maintenance.AutoComplete(
                resolvedAt);

            await _maintenanceRepository
                .UpdateBulkAsync(
                    maintenance,
                    default);
        }

        var deletedCount =
            await _alerteRepository
                .DeleteUnprocessedByDeviceAndTypeCodeAsync(
                    device.DeviceId,
                    typeCode,
                    default);

        if (openMaintenances.Count > 0 ||
            deletedCount > 0)
        {
            await _unitOfWork
                .SaveChangesAsync(default);

            if (TryGetCaptureIndex(
                    typeCode,
                    out _))
            {
                var state =
                    await BuildDeviceCaptureStatePayloadAsync(
                        device.DeviceId,
                        $"RESOLVED_{typeCode}");

                await _signalService
                    .NotifyDeviceCaptureStateChanged(
                        state);
            }
        }
    }

    // ============================================================
    // ALARME/W
    // ============================================================

    private async Task HandleAlarmeWMessageAsync(
        string payload,
        string deviceMatricule)
    {
        _logger.LogInformation(
            "Processing ALARME/W - Matricule: {Matricule}, Payload: {Payload}",
            deviceMatricule,
            payload);

        var device =
            await _deviceRepository.GetByMatriculeAsync(
                deviceMatricule,
                default);

        if (device == null)
        {
            _logger.LogWarning(
                "No Device found with Matricule '{Matricule}' for ALARME/W",
                deviceMatricule);

            return;
        }

        if (!TryParseOnlineStatusText(
                payload,
                out var isOnline))
        {
            _logger.LogWarning(
                "Invalid ALARME/W payload for device '{Matricule}'. " +
                "Expected 'online' or 'offline', received: {Payload}",
                deviceMatricule,
                payload);

            return;
        }

        var now = DateTime.UtcNow;

        device.SetOnlineStatus(
            isOnline);

        await _deviceRepository
            .UpdateOnlineStatusAsync(
                device.DeviceId,
                isOnline,
                now,
                default);

        var deviceStatusPayload =
            new DeviceStatusPayload(
                device.DeviceId.Value,
                device.DeviceName,
                device.Matricule,
                isOnline,
                now);

        await _signalService
            .NotifyDeviceStatusChanged(
                deviceStatusPayload);
    }

    private static bool TryParseOnlineStatusText(
        string? value,
        out bool isOnline)
    {
        isOnline = true;

        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        var normalized =
            value.Trim()
                .ToUpperInvariant();

        if (normalized == "ONLINE")
        {
            isOnline = true;
            return true;
        }

        if (normalized == "OFFLINE")
        {
            isOnline = false;
            return true;
        }

        return false;
    }

    // ============================================================
    // ALARME/RESET
    // ============================================================

    private async Task HandleAlarmeResetMessageAsync(
        string deviceMatricule)
    {
        _logger.LogInformation(
            "Processing ALARME/RESET - Matricule: {Matricule}",
            deviceMatricule);

        var device =
            await _deviceRepository.GetByMatriculeAsync(
                deviceMatricule,
                default);

        if (device == null)
        {
            _logger.LogWarning(
                "No Device found with Matricule '{Matricule}' for RESET",
                deviceMatricule);

            return;
        }

        var deletedCount =
            await _alerteRepository.DeleteAsync(
                a =>
                    a.DispositifId == device.DeviceId &&
                    !a.Traiter,
                default);

        _logger.LogInformation(
            "ALARME/RESET - Deleted {Count} untreated alert(s) for device '{Matricule}'",
            deletedCount,
            deviceMatricule);
    }

    // ============================================================
    // SMS
    // ============================================================

    private async Task SendSMSForDeviceAsync(
        DeviceId deviceId,
        string deviceName,
        string deviceMatricule,
        string smsText,
        string smsEventType,
        CancellationToken cancellationToken)
    {
        try
        {
            var smsConfig =
                await _smsConfigurationRepository
                    .GetConfigurationAsync(
                        cancellationToken);

            var isSmsActive =
                smsConfig?.IsActive ?? false;

            var smsApiUrl =
                smsConfig?.ApiUrl;

            var isEventAllowed =
                smsEventType switch
                {
                    "alerte" =>
                        smsConfig?.SmsOnAlerte ?? true,

                    "badgeT3" =>
                        smsConfig?.SmsOnBadgeT3 ?? true,

                    "badgeT4" =>
                        smsConfig?.SmsOnBadgeT4 ?? true,

                    "badgeT5" =>
                        smsConfig?.SmsOnBadgeT5 ?? true,

                    _ => true
                };

            if (!isEventAllowed)
            {
                return;
            }

            var smsRecipients =
                await _smsRepository
                    .GetByDeviceIdAsync(
                        deviceId,
                        cancellationToken);

            var phoneNumbers =
                smsRecipients
                    .Select(s => s.PhoneNumber)
                    .Distinct()
                    .ToList();

            var planningEmployees =
                await _planningRepository
                    .GetEmployeesByDateAndDeviceAsync(
                        DateTime.Now,
                        deviceId,
                        cancellationToken);

            var employeePhones =
                planningEmployees
                    .Where(e => e.Phone > 0)
                    .Select(e => e.Phone.ToString())
                    .Distinct()
                    .ToList();

            foreach (var phone in employeePhones)
            {
                if (!phoneNumbers.Contains(phone))
                {
                    phoneNumbers.Add(phone);
                }
            }

            if (isSmsActive &&
                !string.IsNullOrWhiteSpace(smsApiUrl) &&
                phoneNumbers.Any())
            {
                await _smsService.SendSMSAsync(
                    phoneNumbers,
                    smsText,
                    smsApiUrl,
                    cancellationToken);
            }

            await SendEmailForDeviceAsync(
                deviceId,
                deviceName,
                smsText,
                planningEmployees,
                cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error sending SMS/Email for device {DeviceName}",
                deviceName);
        }
    }

    private async Task SendEmailForDeviceAsync(
        DeviceId deviceId,
        string deviceName,
        string notificationText,
        List<CollectManagement.Application.Features.Alertes.Queries.GetEmployeesByPlanning.EmployeePlanningDto> planningEmployees,
        CancellationToken cancellationToken)
    {
        try
        {
            var emailAddresses =
                planningEmployees
                    .Where(e =>
                        !string.IsNullOrWhiteSpace(e.Email))
                    .Select(e => e.Email!)
                    .Distinct()
                    .ToList();

            if (!emailAddresses.Any())
            {
                return;
            }

            await _emailService.SendEmailAsync(
                emailAddresses,
                $"Alerte - {deviceName}",
                notificationText,
                cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error sending email for device {DeviceName}",
                deviceName);
        }
    }

    // ============================================================
    // EMP
    // ============================================================

    private async Task HandleAlarmeEmpMessageAsync(
        string payload,
        string deviceMatricule)
    {
        // Garde ici ton implémentation actuelle complète
        // de HandleAlarmeEmpMessageAsync.
        //
        // Elle n'est pas responsable de l'erreur SensorMeasurement.
        //
        // IMPORTANT :
        // deviceMatricule est bien défini comme paramètre.
    }

    // ============================================================
    // DEVICE CAPTURE STATE
    // ============================================================

    private async Task<DeviceCaptureStateRealtimePayload>
        BuildDeviceCaptureStatePayloadAsync(
            DeviceId deviceId,
            string trigger)
    {
        var device =
            await _deviceRepository.GetOneAsync(
                deviceId,
                default);

        var latestMaintenance =
            await _maintenanceRepository
                .GetLatestByDeviceIdAsync(
                    deviceId,
                    default);

        var latestAlertsByCode =
            await _alerteRepository
                .GetLatestUnprocessedCaptureAlertsByDeviceAsync(
                    deviceId,
                    default);

        var totalCaptures =
            Math.Max(
                0,
                device.NombreCapteur);

        var captureStatuses =
            new List<string>(totalCaptures);

        var captureLastErrorAt =
            new List<DateTime?>(totalCaptures);

        var captureAlertLabels =
            new List<string?>(totalCaptures);

        for (var captureIndex = 1;
             captureIndex <= totalCaptures;
             captureIndex++)
        {
            var code =
                $"A{captureIndex}";

            if (latestAlertsByCode
                .TryGetValue(
                    code,
                    out var latestAlert))
            {
                captureStatuses.Add("ERROR");

                captureLastErrorAt.Add(
                    latestAlert.Date);

                captureAlertLabels.Add(
                    latestAlert.Type?.Label ??
                    latestAlert.Type?.Code ??
                    code);
            }
            else
            {
                captureStatuses.Add("WORKING");
                captureLastErrorAt.Add(null);
                captureAlertLabels.Add(null);
            }
        }

        var workingCaptures =
            captureStatuses.Count(
                status => status == "WORKING");

        var capture1Status =
            totalCaptures >= 1
                ? captureStatuses[0]
                : "NOT_AVAILABLE";

        var capture2Status =
            totalCaptures >= 2
                ? captureStatuses[1]
                : "NOT_AVAILABLE";

        var capture1LastErrorAt =
            totalCaptures >= 1
                ? captureLastErrorAt[0]
                : null;

        var capture2LastErrorAt =
            totalCaptures >= 2
                ? captureLastErrorAt[1]
                : null;

        var maintenanceStartedAt =
            latestMaintenance?.T2Assignment ??
            latestMaintenance?.T3Arrival ??
            latestMaintenance?.T1Alerte;

        var maintenanceFinishedAt =
            latestMaintenance?.T5Confirmation;

        var isUnderMaintenance =
            latestMaintenance != null &&
            !maintenanceFinishedAt.HasValue;

        var maintenancePhase =
            ResolveMaintenancePhase(
                latestMaintenance);

        var maintenancePhaseStartedAt =
            ResolveMaintenancePhaseStartedAt(
                latestMaintenance,
                maintenancePhase);

        var maintenanceCaptureIndex =
            isUnderMaintenance
                ? ResolveMaintenanceCaptureIndex(
                    latestMaintenance,
                    totalCaptures)
                : null;

        var maintenanceEmployeeName =
            latestMaintenance != null
                ? $"{latestMaintenance.Employee?.Nom ?? string.Empty} " +
                  $"{latestMaintenance.Employee?.Prenom ?? string.Empty}"
                    .Trim()
                : null;

        var timeline =
            captureLastErrorAt
                .Concat(
                    new DateTime?[]
                    {
                        maintenanceStartedAt,
                        maintenanceFinishedAt
                    });

        var lastUpdatedAt =
            timeline
                .Where(d => d.HasValue)
                .Select(d => d!.Value)
                .DefaultIfEmpty(DateTime.UtcNow)
                .Max();

        return new DeviceCaptureStateRealtimePayload(
            device.DeviceId.Value,
            device.DeviceName,
            device.Matricule,
            totalCaptures,
            workingCaptures,
            capture1Status,
            capture2Status,
            capture1LastErrorAt,
            capture2LastErrorAt,
            captureStatuses,
            captureLastErrorAt,
            captureAlertLabels,
            maintenanceCaptureIndex,
            isUnderMaintenance,
            maintenancePhase,
            maintenancePhaseStartedAt,
            maintenanceStartedAt,
            maintenanceFinishedAt,
            maintenanceEmployeeName,
            lastUpdatedAt,
            trigger);
    }

    private static string? ResolveMaintenancePhase(
        Maintenance? maintenance)
    {
        if (maintenance == null)
        {
            return null;
        }

        if (maintenance.T5Confirmation.HasValue)
        {
            return null;
        }

        if (maintenance.T3Arrival.HasValue &&
            maintenance.T4Completion.HasValue)
        {
            return "REPARATION";
        }

        if (maintenance.T3Arrival.HasValue)
        {
            return "DIAGNOSTIC";
        }

        return "AFFECTEE";
    }

    private static DateTime? ResolveMaintenancePhaseStartedAt(
        Maintenance? maintenance,
        string? phase)
    {
        if (maintenance == null ||
            string.IsNullOrWhiteSpace(phase))
        {
            return null;
        }

        var normalized =
            phase.ToUpperInvariant();

        return normalized switch
        {
            "AFFECTEE" =>
                maintenance.T2Assignment ??
                maintenance.T1Alerte,

            "DIAGNOSTIC" =>
                maintenance.T3Arrival ??
                maintenance.T2Assignment ??
                maintenance.T1Alerte,

            "REPARATION" =>
                maintenance.T4Completion ??
                maintenance.T3Arrival ??
                maintenance.T2Assignment ??
                maintenance.T1Alerte,

            _ => null
        };
    }

    private static bool TryGetCaptureIndex(
        string? code,
        out int captureIndex)
    {
        captureIndex = 0;

        if (string.IsNullOrWhiteSpace(code) ||
            code.Length < 2 ||
            !code.StartsWith(
                "A",
                StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        return int.TryParse(
                   code[1..],
                   NumberStyles.None,
                   CultureInfo.InvariantCulture,
                   out captureIndex)
               &&
               captureIndex > 0;
    }

    private static int? ResolveMaintenanceCaptureIndex(
        Maintenance? maintenance,
        int totalCaptures)
    {
        if (maintenance == null ||
            totalCaptures <= 0 ||
            string.IsNullOrWhiteSpace(
                maintenance.Description))
        {
            return null;
        }

        const string prefix =
            "CAPTURE_CODE:";

        if (!maintenance.Description.StartsWith(
                prefix,
                StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        var code =
            maintenance.Description[
                prefix.Length..]
                .Trim();

        if (!TryGetCaptureIndex(
                code,
                out var captureIndex))
        {
            return null;
        }

        return captureIndex <= totalCaptures
            ? captureIndex
            : null;
    }

    // ============================================================
    // HELPERS
    // ============================================================

    private static string? ExtractCaptureCode(
        string? description)
    {
        const string prefix =
            "CAPTURE_CODE:";

        if (string.IsNullOrWhiteSpace(description) ||
            !description.StartsWith(
                prefix,
                StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        return description[
            prefix.Length..]
            .Trim();
    }
}

// ================================================================
// MQTT DTOs
// ================================================================

public class AlarmeAMessage
{
    public long DI { get; set; }

    public long TI { get; set; }

    public List<long>? Dur { get; set; }

    public JToken? Type { get; set; }

    public List<bool> A { get; set; }
}

public class AlarmeEmpMessage
{
    public string DN { get; set; }

    public string DI { get; set; }

    public string TI { get; set; }

    public string App { get; set; }

    public string TAG_ID { get; set; }
}