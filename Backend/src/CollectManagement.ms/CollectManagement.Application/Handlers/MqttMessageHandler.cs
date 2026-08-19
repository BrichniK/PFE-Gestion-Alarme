using System.Text;
using System.Globalization;
using CollectManagement.Application.Interfaces.Repositories.SensorMeasurements;
using CollectManagement.Domain.SensorMeasurements;
using CollectManagement.Domain.SensorMeasurements.ValueObjects;
using CollectManagement.Application.Interfaces.Employees;
using CollectManagement.Application.Interfaces.Repositories.Alertes;
using CollectManagement.Application.Interfaces.Repositories.ConfigurationGenerales;
using CollectManagement.Application.Interfaces.Repositories.Devices;
using CollectManagement.Application.Interfaces.Repositories.Maintenances;
using CollectManagement.Application.Interfaces.Repositories.Plannings;
using CollectManagement.Application.Interfaces.Repositories.SMS;
using CollectManagement.Application.Interfaces.Repositories.Types;
using CollectManagement.Application.Interfaces.Repositories.SMSConfigurations;
using CollectManagement.Application.Interfaces.Services;
using CollectManagement.Application.Shared;
using CollectManagement.Domain.Alertes;
using CollectManagement.Domain.Alertes.ValueObjects;
using CollectManagement.Domain.Devices;
using CollectManagement.Domain.Devices.ValueObjects;
using CollectManagement.Domain.Maintenances;
using CollectManagement.Domain.Maintenances.ObjectValues;
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
        _configurationGeneraleRepository = configurationGeneraleRepository;
        _sensorMeasurementRepository = sensorMeasurementRepository;
    }

private async Task HandleSensorMeasurementMessageAsync(
    string payload,
    string deviceMatricule,
    string sensorCode)
{
    try
    {
        var json = JObject.Parse(payload);

        var temperature = json["temperature"]?.Value<double?>();
        var vibration = json["vibration"]?.Value<double?>();
        var pressure = json["pressure"]?.Value<double?>();
        var humidity = json["humidity"]?.Value<double?>();
        var status = json["status"]?.Value<string>();

        var isFailure =
            string.Equals(status, "FAILURE", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(status, "CRITICAL", StringComparison.OrdinalIgnoreCase);

        var device = await _deviceRepository.GetByMatriculeAsync(
            deviceMatricule,
            default);

        if (device == null)
        {
            _logger.LogWarning(
                "No Device found with Matricule '{Matricule}'",
                deviceMatricule);

            return;
        }

        var measurement = SensorMeasurement.Create(
            new SensorMeasurementId(Ulid.NewUlid()),
            device.DeviceId,
            sensorCode,
            DateTime.UtcNow,
            temperature,
            vibration,
            pressure,
            humidity,
            isFailure);

        await _sensorMeasurementRepository.AddAsync(
            measurement,
            default);

        await _unitOfWork.SaveChangesAsync(default);

        _logger.LogInformation(
            "Sensor measurement saved - Device: {Device}, Sensor: {Sensor}, Temperature: {Temperature}, Vibration: {Vibration}, Pressure: {Pressure}, Humidity: {Humidity}, Failure: {IsFailure}",
            deviceMatricule,
            sensorCode,
            temperature,
            vibration,
            pressure,
            humidity,
            isFailure);
    }
    catch (JsonReaderException ex)
    {
        _logger.LogError(
            ex,
            "Invalid JSON sensor measurement payload: {Payload}",
            payload);
    }
    catch (Exception ex)
    {
        _logger.LogError(
            ex,
            "Error processing sensor measurement from device {Device}",
            deviceMatricule);
    }
}


    

    public async Task HandleMessageAsync(MqttApplicationMessageReceivedEventArgs sender)
    {
        try
        {
            string payload = Encoding.UTF8.GetString(sender.ApplicationMessage.Payload);
            _logger.LogInformation("MQTT Message received - Topic: {Topic}, Payload: {Payload}",
                sender.ApplicationMessage.Topic, payload);

            var topicParts = sender.ApplicationMessage.Topic.Split('/');

            if (topicParts.Length < 3)
            {
                _logger.LogWarning("Unexpected topic format: {Topic}", sender.ApplicationMessage.Topic);
                return;
            }

            string section = topicParts[0].ToUpperInvariant();
            string subSection = topicParts[1].ToUpperInvariant();
            string deviceNumber = topicParts[2];

            if (section == "ALARME" && subSection == "EMP")
            {
                await HandleAlarmeEmpMessageAsync(payload, deviceNumber);
            }
            else if (section == "ALARME" && subSection == "A")
            {
                await HandleAlarmeAMessageAsync(payload, deviceNumber);
            }
            else if (section == "ALARME" && subSection == "W")
            {
                await HandleAlarmeWMessageAsync(payload, deviceNumber);
            }
            else if (section == "ALARME" && subSection == "RESET")
            {
                await HandleAlarmeResetMessageAsync(deviceNumber);
            }
            else if (section == "ALARME")
            {
                await HandleSensorMeasurementMessageAsync(
                    payload,
                    subSection,
                    deviceNumber);
            }
            else
            {
                _logger.LogWarning(
                    "Unhandled topic: {Topic}",
                    sender.ApplicationMessage.Topic);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing MQTT message");
        }
    }

    private async Task HandleAlarmeAMessageAsync(string payload, string deviceMatricule)
    {
        var settings = new JsonSerializerSettings
        {
            Error = (_, e) => e.ErrorContext.Handled = true
        };
        var message = JsonConvert.DeserializeObject<AlarmeAMessage>(payload, settings);
        if (message == null || message.A == null)
        {
            _logger.LogWarning("Invalid JSON payload for ALARME/A: {Payload}", payload);
            return;
        }

        string di = message.DI.ToString();

        if (string.IsNullOrWhiteSpace(di))
        {
            _logger.LogWarning("DI (Matricule) missing in ALARME/A payload");
            return;
        }

        var device = await _deviceRepository.GetByMatriculeAsync(di, default);
        if (device == null)
        {
            _logger.LogWarning("No Device found with Matricule '{DI}'", di);
            return;
        }

        var date = DateTimeOffset.FromUnixTimeSeconds(message.TI).DateTime;
        var alertStates = message.A;
        var durations = message.Dur ?? new List<long>();

        if (alertStates == null || alertStates.Count == 0)
        {
            _logger.LogWarning("A array missing or empty in ALARME/A payload for Device '{DI}'", di);
            return;
        }

        var typeCodes = ResolveAlarmeTypeCodes(message.Type, alertStates.Count);

        var changed = false;

        for (int i = 0; i < alertStates.Count; i++)
        {
            var typeCode = typeCodes[i];
            var isActive = alertStates[i];

            if (string.IsNullOrWhiteSpace(typeCode))
            {
                _logger.LogWarning("Type code missing at index {Index} in ALARME/A payload for Device '{DI}'", i, di);
                continue;
            }

            if (isActive)
            {
                var alertDate = date;
                if (i < durations.Count && durations[i] >= 0)
                {
                    var alertStartUnix = Math.Max(0, message.TI - durations[i]);
                    alertDate = DateTimeOffset.FromUnixTimeSeconds(alertStartUnix).DateTime;
                }

                // Alert is active — create alert if no unprocessed one exists
                await HandleAlertActiveAsync(device, typeCode, alertDate, di);
            }
            else
            {
                // Alert resolved — auto-complete open maintenances and delete unprocessed alerts
                await HandleAlertResolvedAsync(device, typeCode, date, di);
            }

            changed = true;
        }

        if (changed)
        {
            await _signalService.NotifyMaintenanceUpdated();
        }
    }

    private static List<string> ResolveAlarmeTypeCodes(JToken? typeToken, int alertCount)
    {
        var codes = new List<string>();

        if (typeToken is JArray typeArray)
        {
            foreach (var item in typeArray)
            {
                var code = item?.ToString()?.Trim();
                if (!string.IsNullOrWhiteSpace(code))
                {
                    codes.Add(code);
                }
            }
        }
        else
        {
            var code = typeToken?.ToString()?.Trim();
            if (!string.IsNullOrWhiteSpace(code))
            {
                codes.Add(code);
            }
        }

        while (codes.Count < alertCount)
        {
            codes.Add($"A{codes.Count + 1}");
        }

        return codes;
    }

    private async Task HandleAlertActiveAsync(
        Domain.Devices.Device device,
        string typeCode,
        DateTime date,
        string di)
    {
        var type = await _typeRepository.GetByCodeAsync(typeCode, default);
        if (type == null)
        {
            _logger.LogWarning("No Type found with Code '{TypeCode}'", typeCode);
            return;
        }

        // Skip if there's already an unprocessed alert with the same DeviceId and Type
        var existingAlert = await _alerteRepository.GetLatestCaptureAlertByDeviceAndCodeAsync(device.DeviceId, typeCode, default);
        if (existingAlert != null && !existingAlert.Traiter)
        {
            _logger.LogInformation(
                "Duplicate ALARME/A skipped - Unprocessed alert {AlerteId} already exists for Device '{DeviceName}' (Matricule: {DI}), Type: {Type}",
                existingAlert.AlerteId.Value, device.DeviceName, di, typeCode);
            return;
        }

        var alerteId = new AlerteId(Ulid.NewUlid());
        var alerte = Alerte.Create(
            alerteId,
            date,
            device.DeviceId,
            type.TypeId,
            false
        );

        await _alerteRepository.AddAsync(alerte, default);
        await _unitOfWork.SaveChangesAsync(default);

        _logger.LogInformation(
            "Alerte {AlerteId} created - Date: {Date}, Device: {DeviceName} (Matricule: {DI}), Type: {TypeCode}",
            alerteId.Value,
            date.ToString("dd/MM/yyyy HH:mm"),
            device.DeviceName,
            di,
            typeCode);

        // Send SMS notification
        var typeLabelOrCode = type.Label ?? type.Code ?? typeCode;
        var smsText = string.Join("\n", new[]
        {
            date.ToString("dd/MM/yyyy HH:mm:ss", CultureInfo.InvariantCulture),
            "Alerte sur equipement",
            device.DeviceName,
            typeLabelOrCode
        });

        await SendSMSForDeviceAsync(device.DeviceId, device.DeviceName, di, smsText, "alerte", default);

        if (TryGetCaptureIndex(typeCode, out _))
        {
            var deviceCaptureState = await BuildDeviceCaptureStatePayloadAsync(device.DeviceId, $"ALARM_{typeCode}");
            await _signalService.NotifyDeviceCaptureStateChanged(deviceCaptureState);
        }
    }

    private async Task HandleAlertResolvedAsync(
        Domain.Devices.Device device,
        string typeCode,
        DateTime resolvedAt,
        string di)
    {
        // 1. Auto-complete open maintenances (affecté/diagnostique/réparation) for this device + type
        var openMaintenances = await _maintenanceRepository
            .GetOpenMaintenancesByDeviceAndCaptureCodeAsync(device.DeviceId, typeCode, default);

        foreach (var maintenance in openMaintenances)
        {
            maintenance.AutoComplete(resolvedAt);
            await _maintenanceRepository.UpdateBulkAsync(maintenance, default);

            _logger.LogInformation(
                "Maintenance {MaintenanceId} auto-completed (alert resolved) - Device: {DeviceName} (Matricule: {DI}), Type: {TypeCode}",
                maintenance.MaintenanceId.Value, device.DeviceName, di, typeCode);
        }

        // 2. Delete unprocessed alerts for this device + type
        var deletedCount = await _alerteRepository
            .DeleteUnprocessedByDeviceAndTypeCodeAsync(device.DeviceId, typeCode, default);

        if (deletedCount > 0)
        {
            _logger.LogInformation(
                "Deleted {Count} unprocessed alert(s) (alert resolved) - Device: {DeviceName} (Matricule: {DI}), Type: {TypeCode}",
                deletedCount, device.DeviceName, di, typeCode);
        }

        if (openMaintenances.Count > 0 || deletedCount > 0)
        {
            await _unitOfWork.SaveChangesAsync(default);

            if (TryGetCaptureIndex(typeCode, out _))
            {
                var deviceCaptureState = await BuildDeviceCaptureStatePayloadAsync(device.DeviceId, $"RESOLVED_{typeCode}");
                await _signalService.NotifyDeviceCaptureStateChanged(deviceCaptureState);
            }
        }
    }
    
    private async Task SendSMSForDeviceAsync(
        Domain.Devices.ValueObjects.DeviceId deviceId,
        string deviceName,
        string deviceMatricule,
        string smsText,
        string smsEventType,
        CancellationToken cancellationToken)
    {
        try
        {
            // 0. Check SMS Configuration
            var smsConfig = await _smsConfigurationRepository.GetConfigurationAsync(cancellationToken);
            var isSmsActive = smsConfig?.IsActive ?? false;
            var smsApiUrl = smsConfig?.ApiUrl;

            // Check per-event toggle
            var isEventAllowed = smsEventType switch
            {
                "alerte" => smsConfig?.SmsOnAlerte ?? true,
                "badgeT3" => smsConfig?.SmsOnBadgeT3 ?? true,
                "badgeT4" => smsConfig?.SmsOnBadgeT4 ?? true,
                "badgeT5" => smsConfig?.SmsOnBadgeT5 ?? true,
                _ => true
            };

            if (!isEventAllowed)
            {
                _logger.LogInformation("SMS skipped for device {DeviceName} because SMS for event '{EventType}' is disabled.", deviceName, smsEventType);
                return;
            }

            // 1. Get phone numbers from SMS table (configured recipients for the device)
            var smsRecipients = await _smsRepository.GetByDeviceIdAsync(deviceId, cancellationToken);
            var phoneNumbers = smsRecipients
                .Select(s => s.PhoneNumber)
                .Distinct()
                .ToList();

            // 2. Get employee phone numbers filtered by planning (same date + same device)
            var planningEmployees = await _planningRepository.GetEmployeesByDateAndDeviceAsync(
                DateTime.Now,
                deviceId,
                cancellationToken);

            var employeePhones = planningEmployees
                .Where(e => e.Phone > 0)
                .Select(e => e.Phone.ToString())
                .Distinct()
                .ToList();

            // Merge both lists, avoiding duplicates
            foreach (var phone in employeePhones)
            {
                if (!phoneNumbers.Contains(phone))
                    phoneNumbers.Add(phone);
            }

            // Send SMS ONLY if active and we have URL and numbers
            if (isSmsActive && !string.IsNullOrWhiteSpace(smsApiUrl))
            {
                if (phoneNumbers.Any())
                {
                    await _smsService.SendSMSAsync(phoneNumbers, smsText, smsApiUrl, cancellationToken);

                    _logger.LogInformation("SMS sent to {Count} recipients for device {DeviceName}",
                        phoneNumbers.Count, deviceName);
                }
                else
                {
                    _logger.LogInformation("No SMS recipients found for device {DeviceName} (Matricule: {Matricule})",
                        deviceName, deviceMatricule);
                }
            }
            else
            {
                _logger.LogInformation("SMS not sent for device {DeviceName} because SMS configuration is inactive or API URL is missing.", deviceName);
            }

            // Send email to planning employees
            await SendEmailForDeviceAsync(deviceId, deviceName, smsText, planningEmployees, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending SMS/Email for device {DeviceName}", deviceName);
            // Don't throw - SMS/Email failure shouldn't prevent alert creation
        }
    }

    private async Task SendEmailForDeviceAsync(
        Domain.Devices.ValueObjects.DeviceId deviceId,
        string deviceName,
        string notificationText,
        List<CollectManagement.Application.Features.Alertes.Queries.GetEmployeesByPlanning.EmployeePlanningDto> planningEmployees,
        CancellationToken cancellationToken)
    {
        try
        {
            var emailAddresses = planningEmployees
                .Where(e => !string.IsNullOrWhiteSpace(e.Email))
                .Select(e => e.Email!)
                .Distinct()
                .ToList();

            if (!emailAddresses.Any())
            {
                _logger.LogInformation("No email recipients found for device {DeviceName}", deviceName);
                return;
            }

            var emailSubject = $"Alerte - {deviceName}";
            await _emailService.SendEmailAsync(emailAddresses, emailSubject, notificationText, cancellationToken);

            _logger.LogInformation("Email sent to {Count} recipients for device {DeviceName}",
                emailAddresses.Count, deviceName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending email for device {DeviceName}", deviceName);
        }
    }

    private async Task HandleAlarmeEmpMessageAsync(string payload, string deviceMatricule)
    {
        string responseTopic = $"ALARME/REMP/{deviceMatricule}";

        var message = JsonConvert.DeserializeObject<AlarmeEmpMessage>(payload);
        if (message == null)
        {
            _logger.LogWarning("Invalid JSON payload for ALARME/EMP: {Payload}", payload);
            var deviceFromTopic = await _deviceRepository.GetByMatriculeAsync(deviceMatricule, default);
            await PublishEmpResponseAndMaybeSendSmsAsync(deviceFromTopic, responseTopic, false, null, null, "Payload JSON invalide", default);
            return;
        }

        string tagId = message.TAG_ID;
        string di = message.DI;

        if (string.IsNullOrWhiteSpace(tagId))
        {
            _logger.LogWarning("TAG_ID missing in ALARME/EMP payload");
            var deviceFromTopic = await _deviceRepository.GetByMatriculeAsync(deviceMatricule, default);
            await PublishEmpResponseAndMaybeSendSmsAsync(deviceFromTopic, responseTopic, false, null, null, "TAG_ID manquant", default);
            return;
        }

        if (string.IsNullOrWhiteSpace(di))
        {
            _logger.LogWarning("DI (Matricule) missing in ALARME/EMP payload");
            var deviceFromTopic = await _deviceRepository.GetByMatriculeAsync(deviceMatricule, default);
            await PublishEmpResponseAndMaybeSendSmsAsync(deviceFromTopic, responseTopic, false, tagId, null, "DI (Matricule) manquant", default);
            return;
        }

        _logger.LogInformation("Processing ALARME/EMP - TAG_ID: {TagId}, DI: {DI}, DN: {DN}",
            tagId, di, message.DN);

        // Resolve device first (needed for per-device daily checks)
        var device = await _deviceRepository.GetByMatriculeAsync(di, default);
        if (device == null)
        {
            _logger.LogWarning("No device found with Matricule '{DI}' for ALARME/EMP.", di);
            await PublishEmpResponseAndMaybeSendSmsAsync(null, responseTopic, false, tagId, null, "Device non trouve", default);
            return;
        }

        var configGenerale = await _configurationGeneraleRepository.GetConfigurationAsync(default);
        var diagnostiqueObligatoire = configGenerale?.DiagnostiqueObligatoire ?? true;
        var accepterSeulementEmployesPlanifies = configGenerale?.AccepterSeulementEmployesPlanifies ?? false;

        if (accepterSeulementEmployesPlanifies)
        {
            var employeeFromTag = await _employeeRepository.GetByRfidAsync(tagId, default);
            if (employeeFromTag == null)
            {
                _logger.LogWarning(
                    "RFID '{TagId}' rejected in ALARME/EMP because employee was not found and planned-only mode is enabled.",
                    tagId);
                await PublishEmpResponseAndMaybeSendSmsAsync(device, responseTopic, false, tagId, null, "Employe non trouve", default);
                return;
            }

            var plannedEmployees = await _planningRepository.GetEmployeesByDateAndDeviceAsync(
                DateTime.Now,
                device.DeviceId,
                default);

            var isEmployeePlanned = plannedEmployees.Any(e => e.EmployeeId == employeeFromTag.EmployeeId.Value);
            if (!isEmployeePlanned)
            {
                _logger.LogInformation(
                    "RFID '{TagId}' rejected in ALARME/EMP because employee '{EmployeeId}' is not planned today for device '{DeviceId}'.",
                    tagId,
                    employeeFromTag.EmployeeId.Value,
                    device.DeviceId.Value);
                await PublishEmpResponseAndMaybeSendSmsAsync(
                    device,
                    responseTopic,
                    false,
                    tagId,
                    $"{employeeFromTag.Nom} {employeeFromTag.Prenom}".Trim(),
                    "Employe non planifie pour cet appareil aujourd'hui",
                    default);
                return;
            }
        }

        var today = DateTime.Today;
        var hasOpenTodayForDevice = await _maintenanceRepository.HasOpenMaintenanceForDeviceOnDateAsync(device.DeviceId, today, default);

        var maintenance = await _maintenanceRepository
            .GetLastByEmployeeRfidAndDeviceMatriculeAsync(tagId, di, default);

        var createdFromAlert = false;

        // If there is no open maintenance for this employee/device (T5 already set or no row),
        // we may need to create a new one from the latest unprocessed alert.
        if (maintenance == null || maintenance.T5Confirmation.HasValue)
        {
            // No active maintenance for this employee/device today.
            // If there is no open maintenance for this device today at all, create a new one from latest unprocessed alert.
            if (!hasOpenTodayForDevice)
            {
                _logger.LogInformation(
                    "No active maintenance today for Employee RFID '{TagId}' and Device Matricule '{DI}'. Creating new maintenance from latest unprocessed alert.",
                    tagId, di);

                maintenance = await CreateMaintenanceFromAlertAsync(tagId, di, message.TI, responseTopic);

                // If creation failed (no alert or employee/device missing), stop here.
                if (maintenance == null)
                {
                    return;
                }

                createdFromAlert = true;
            }
            else
            {
                // There is an open maintenance for this device today, but not bound to this employee.
                // Check if overwrite mode is enabled in ConfigurationGenerale.
                var ecraserActif = configGenerale?.EcraserEmployeMaintenance ?? false;

                if (ecraserActif)
                {
                    // Overwrite mode: reassign the existing open maintenance to the new employee.
                    var existingEmployee = await _employeeRepository.GetByRfidAsync(tagId, default);
                    if (existingEmployee == null)
                    {
                        _logger.LogWarning("No employee found with RFID '{TagId}'. Cannot reassign maintenance.", tagId);
                        await PublishEmpResponseAndMaybeSendSmsAsync(device, responseTopic, false, tagId, null, "Employe non trouve", default);
                        return;
                    }

                    var openMaintenance = await _maintenanceRepository.GetOpenMaintenanceForDeviceOnDateAsync(device.DeviceId, today, default);
                    if (openMaintenance == null)
                    {
                        _logger.LogWarning("Could not find open maintenance to reassign for Device Matricule '{DI}'.", di);
                        await PublishEmpResponseAndMaybeSendSmsAsync(device, responseTopic, false, tagId, null, "Maintenance ouverte introuvable", default);
                        return;
                    }

                    var previousEmployeeName = $"{openMaintenance.Employee?.Nom} {openMaintenance.Employee?.Prenom}".Trim();
                    openMaintenance.ReassignEmployee(existingEmployee.EmployeeId);
                    await _maintenanceRepository.UpdateBulkAsync(openMaintenance, default);
                    await _unitOfWork.SaveChangesAsync(default);

                    // Re-fetch with navigation properties
                    maintenance = await _maintenanceRepository.GetOneAsync(openMaintenance.MaintenanceId, default);

                    var newEmployeeName = $"{existingEmployee.Nom} {existingEmployee.Prenom}".Trim();
                    _logger.LogInformation(
                        "Maintenance {MaintenanceId} reassigned from '{PreviousEmployee}' to '{NewEmployee}' (RFID: {TagId}) for Device Matricule '{DI}'.",
                        openMaintenance.MaintenanceId.Value, previousEmployeeName, newEmployeeName, tagId, di);

                    await PublishEmpResponseAndMaybeSendSmsAsync(device, responseTopic, true, tagId, newEmployeeName,
                        $"Maintenance reassignee de {previousEmployeeName} a {newEmployeeName}", default, n: 1, smsEventType: "badgeT3");

                    await _signalService.NotifyMaintenanceUpdated();

                    var deviceCaptureStateReassign = await BuildDeviceCaptureStatePayloadAsync(device.DeviceId, "MAINTENANCE_REASSIGN");
                    await _signalService.NotifyDeviceCaptureStateChanged(deviceCaptureStateReassign);
                    return;
                }

                _logger.LogInformation(
                    "Open maintenance already exists today for Device Matricule '{DI}' but not for Employee RFID '{TagId}'. Ignoring ALARME/EMP.",
                    di, tagId);

                await PublishEmpResponseAndMaybeSendSmsAsync(device, responseTopic, false, tagId, null,
                    "Maintenance deja en cours pour cet appareil (autre employe)", default);
                return;
            }
        }

        // If we just created a maintenance from the alert, T1/T2/T3 are already set and T4 must stay null.
        // In this case, we do NOT advance the T-step with ProcessAlarmRfidScan.
        if (createdFromAlert)
        {
            if (!diagnostiqueObligatoire && !maintenance.T4Completion.HasValue)
            {
                // Diagnostic optional: align diagnostic and repair timestamps on first EMP badge.
                var t4 = maintenance.T3Arrival ?? maintenance.T2Assignment ?? DateTime.UtcNow.AddHours(1);
                maintenance.Update(
                    maintenance.DeviceId,
                    maintenance.EmployeeId,
                    maintenance.T1Alerte,
                    maintenance.T2Assignment,
                    maintenance.T3Arrival ?? t4,
                    t4,
                    maintenance.T5Confirmation,
                    maintenance.T6NextAlert,
                    maintenance.Description);

                await _maintenanceRepository.UpdateBulkAsync(maintenance, default);
                await _unitOfWork.SaveChangesAsync(default);
            }

            var empNameCreated = $"{maintenance.Employee?.Nom} {maintenance.Employee?.Prenom}".Trim();
            await PublishEmpResponseAndMaybeSendSmsAsync(device, responseTopic, true, tagId, empNameCreated,
                !diagnostiqueObligatoire
                    ? "Maintenance creee et passage direct en reparation (diagnostic optionnel)"
                    : "Maintenance creee et T1/T2/T3 enregistres a partir de l'alerte",
                default,
                n: !diagnostiqueObligatoire ? 2 : 1,
                smsEventType: !diagnostiqueObligatoire ? "badgeT4" : "badgeT3",
                maintenance: maintenance);

            await _signalService.NotifyMaintenanceUpdated();

            var deviceCaptureStateCreated = await BuildDeviceCaptureStatePayloadAsync(
                device.DeviceId,
                !diagnostiqueObligatoire ? "MAINTENANCE_T4" : "MAINTENANCE_CREATED");
            await _signalService.NotifyDeviceCaptureStateChanged(deviceCaptureStateCreated);
            return;
        }

        var stepCompleted = maintenance.ProcessAlarmRfidScan(diagnostiqueObligatoire);

        if (stepCompleted == null)
        {
            _logger.LogInformation(
                "Maintenance {MaintenanceId} already has T3, T4 and T5 set. No update needed.",
                maintenance.MaintenanceId.Value);
            //var empName = $"{maintenance.Employee?.Nom} {maintenance.Employee?.Prenom}".Trim();
            var empName = $"{maintenance.Employee?.Prenom}".Trim();
            await PublishEmpResponseAndMaybeSendSmsAsync(device, responseTopic, false, tagId, empName, "Maintenance deja complete (T3, T4 et T5 deja renseignes)", default);
            return;
        }

        await _maintenanceRepository.UpdateBulkAsync(maintenance, default);

        var capturedAt = stepCompleted == "T3"
            ? maintenance.T3Arrival
            : stepCompleted == "T4"
                ? maintenance.T4Completion
                : maintenance.T5Confirmation;

        if (!capturedAt.HasValue)
        {
            _logger.LogWarning(
                "Maintenance {MaintenanceId} step {Step} was updated but capture datetime was null.",
                maintenance.MaintenanceId.Value,
                stepCompleted);
            var empName2 = $"{maintenance.Employee?.Nom} {maintenance.Employee?.Prenom}".Trim();
            await PublishEmpResponseAndMaybeSendSmsAsync(device, responseTopic, false, tagId, empName2, $"Erreur interne: datetime de capture {stepCompleted} est null", default);
            return;
        }

        var status = stepCompleted == "T3" ? "IN_MAINTENANCE" : stepCompleted == "T4" ? "IN_CONFIRMATION" : "FINISHED";

        var captureHistoryId = new MaintenanceCaptureHistoryId(Ulid.NewUlid());
        var captureHistory = MaintenanceCaptureHistory.Create(
            captureHistoryId,
            maintenance.MaintenanceId,
            maintenance.DeviceId,
            maintenance.EmployeeId,
            tagId,
            stepCompleted,
            status,
            capturedAt.Value);

        await _maintenanceCaptureHistoryRepository.AddAsync(captureHistory, default);
        await _unitOfWork.SaveChangesAsync(default);

        _logger.LogInformation(
            "Maintenance {MaintenanceId} - {Step} updated to {DateTime} for Employee '{EmployeeName}' (RFID: {TagId}), Device Matricule: {DI}",
            maintenance.MaintenanceId.Value,
            stepCompleted,
            capturedAt,
            $"{maintenance.Employee?.Nom} {maintenance.Employee?.Prenom}",
            tagId,
            di);

        await _signalService.NotifyMaintenanceCaptureUpdated(
            new MaintenanceCaptureRealtimePayload(
                captureHistoryId.Value,
                maintenance.MaintenanceId.Value,
                maintenance.DeviceId.Value,
                maintenance.Device?.DeviceName,
                maintenance.Device?.Matricule,
                maintenance.EmployeeId.Value,
                maintenance.Employee?.Nom,
                maintenance.Employee?.Prenom,
                tagId,
                stepCompleted,
                status,
                capturedAt.Value,
                maintenance.T3Arrival,
                maintenance.T4Completion,
                maintenance.T5Confirmation));

        var deviceCaptureState = await BuildDeviceCaptureStatePayloadAsync(maintenance.DeviceId, $"MAINTENANCE_{stepCompleted}");
        await _signalService.NotifyDeviceCaptureStateChanged(deviceCaptureState);

        await _signalService.NotifyMaintenanceUpdated();

        var stepN = stepCompleted == "T3" ? 1 : stepCompleted == "T4" ? 2 : 3;
        var employeeName = $"{maintenance.Employee?.Nom} {maintenance.Employee?.Prenom}".Trim();
        var badgeEventType = $"badge{stepCompleted}";
        await PublishEmpResponseAndMaybeSendSmsAsync(device, responseTopic, true, tagId, employeeName, $"{stepCompleted} enregistre pour {employeeName}", default, n: stepN, smsEventType: badgeEventType, maintenance: maintenance);
    }

    private async Task PublishEmpResponseAsync(string topic, bool success, string? tagId, string? nom, string message, int? n = null)
    {
        try
        {
            var payload = new
            {
                success = success,
                tag_id = tagId,
                Nom = nom,
                Message = message,
                n = n
            };
            await _mqttPublisher.PublishAsync(topic, payload);
            _logger.LogInformation("Published MQTT response on {Topic} - success: {Success}, tag_id: {TagId}, Nom: {Nom}, Message: {Message}",
                topic, success, tagId, nom, message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to publish MQTT response on {Topic}", topic);
        }
    }

    private async Task PublishEmpResponseAndMaybeSendSmsAsync(
        Device? device,
        string topic,
        bool success,
        string? tagId,
        string? nom,
        string message,
        CancellationToken cancellationToken,
        int? n = null,
        string? smsEventType = null,
        Maintenance? maintenance = null)
    {
        await PublishEmpResponseAsync(topic, success, tagId, nom, message, n);

        if (device == null)
        {
            return;
        }

        if (smsEventType == null)
        {
            return;
        }

        var smsText = await BuildStepSmsTextAsync(device, nom, message, maintenance, cancellationToken);
        await SendSMSForDeviceAsync(device.DeviceId, device.DeviceName, device.Matricule, smsText, smsEventType, cancellationToken);
    }

    private async Task<string> BuildStepSmsTextAsync(
        Device device,
        string? employeeName,
        string sourceMessage,
        Maintenance? maintenance,
        CancellationToken cancellationToken)
    {
        var employeeNom = employeeName?.Trim();
        if (string.IsNullOrWhiteSpace(employeeNom))
        {
            employeeNom = maintenance?.Employee?.Nom;
        }

        var alertName = await ResolveAlertNameForMaintenanceMessageAsync(device.DeviceId, maintenance, cancellationToken);

        if (sourceMessage.StartsWith("Maintenance creee", StringComparison.OrdinalIgnoreCase))
        {
            var at = maintenance?.T2Assignment ?? DateTime.Now;
            return string.Join("\n", new[]
            {
                at.ToString("dd/MM/yyyy HH:mm:ss", CultureInfo.InvariantCulture),
                "Reparation assignie",
                employeeNom ?? "-",
                device.DeviceName,
                alertName
            });
        }

        if (sourceMessage.StartsWith("T3 enregistre pour", StringComparison.OrdinalIgnoreCase))
        {
            var at = maintenance?.T3Arrival ?? DateTime.Now;
            return string.Join("\n", new[]
            {
                at.ToString("dd/MM/yyyy HH:mm:ss", CultureInfo.InvariantCulture),
                "Debut diagnostique",
                employeeNom ?? "-",
                device.DeviceName,
                alertName
            });
        }

        if (sourceMessage.StartsWith("T4 enregistre pour", StringComparison.OrdinalIgnoreCase))
        {
            var at = maintenance?.T4Completion ?? DateTime.Now;
            return string.Join("\n", new[]
            {
                "Debut Reparation",
                at.ToString("dd/MM/yyyy HH:mm:ss", CultureInfo.InvariantCulture),
                employeeNom ?? "-",
                device.DeviceName,
                alertName
            });
        }

        if (sourceMessage.StartsWith("T5 enregistre pour", StringComparison.OrdinalIgnoreCase))
        {
            var at = maintenance?.T5Confirmation ?? DateTime.Now;
            return string.Join("\n", new[]
            {
                "Fin Reparation",
                at.ToString("dd/MM/yyyy HH:mm:ss", CultureInfo.InvariantCulture),
                employeeNom ?? "-",
                device.DeviceName,
                alertName
            });
        }

        return string.Join("\n", new[]
        {
            DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss", CultureInfo.InvariantCulture),
            sourceMessage,
            employeeNom ?? "-",
            device.DeviceName,
            alertName
        });
    }

    private async Task<string> ResolveAlertNameForMaintenanceMessageAsync(
        DeviceId deviceId,
        Maintenance? maintenance,
        CancellationToken cancellationToken)
    {
        var code = ExtractCaptureCode(maintenance?.Description);
        if (string.IsNullOrWhiteSpace(code))
        {
            var latestAlert = await _alerteRepository.GetLatestUnprocessedByDeviceIdAsync(deviceId, cancellationToken);
            return latestAlert?.Type?.Label ?? latestAlert?.Type?.Code ?? "-";
        }

        var type = await _typeRepository.GetByCodeAsync(code, cancellationToken);
        return type?.Label ?? type?.Code ?? code;
    }

    private static string? ExtractCaptureCode(string? description)
    {
        const string prefix = "CAPTURE_CODE:";
        if (string.IsNullOrWhiteSpace(description) || !description.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        return description[prefix.Length..].Trim();
    }

    private async Task<Maintenance?> CreateMaintenanceFromAlertAsync(string tagId, string deviceMatricule, string ti, string responseTopic)
    {
        var employee = await _employeeRepository.GetByRfidAsync(tagId, default);
        if (employee == null)
        {
            _logger.LogWarning("No employee found with RFID '{TagId}'. Cannot create maintenance.", tagId);
            var deviceForSms = await _deviceRepository.GetByMatriculeAsync(deviceMatricule, default);
            await PublishEmpResponseAndMaybeSendSmsAsync(deviceForSms, responseTopic, false, tagId, null, "Employe non trouve", default);
            return null;
        }

        var device = await _deviceRepository.GetByMatriculeAsync(deviceMatricule, default);
        if (device == null)
        {
            _logger.LogWarning("No device found with Matricule '{DI}'. Cannot create maintenance.", deviceMatricule);
            await PublishEmpResponseAndMaybeSendSmsAsync(null, responseTopic, false, tagId, $"{employee.Nom} {employee.Prenom}".Trim(), "Device non trouve", default);
            return null;
        }

        var latestAlert = await _alerteRepository.GetLatestUnprocessedByDeviceIdAsync(device.DeviceId, default);
        if (latestAlert == null)
        {
            _logger.LogWarning(
                "No unprocessed alert found for Device '{DeviceName}' (Matricule: {DI}). Cannot create maintenance.",
                device.DeviceName, deviceMatricule);
            await PublishEmpResponseAndMaybeSendSmsAsync(device, responseTopic, false, tagId, $"{employee.Nom} {employee.Prenom}".Trim(), "Aucune alerte non traitee trouvee pour ce device", default);
            return null;
        }

        // Parse TI from payload for T2/T3 (Unix seconds or ISO string)
        DateTime t2t3;
        if (long.TryParse(ti, out var unixSeconds))
        {
            t2t3 = DateTimeOffset.FromUnixTimeSeconds(unixSeconds).DateTime;
        }
        else if (!DateTime.TryParse(ti, CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal, out t2t3))
        {
            t2t3 = DateTime.Now;
        }

        // Add 1 hour offset for T3
        var t3 = t2t3;
        //var t3 = t2t3.AddHours(1);

        var maintenanceId = new MaintenanceId(Ulid.NewUlid());
        var description = latestAlert.Type?.Code != null
            ? $"CAPTURE_CODE:{latestAlert.Type.Code}"
            : string.Empty;

        var maintenance = Maintenance.Create(
            maintenanceId,
            device.DeviceId,
            employee.EmployeeId,
            t1Alerte: latestAlert.Date,
            t2Assignment: t2t3,
            t3Arrival: t3,
            t4Completion: null,
            t5Confirmation: null,
            t6NextAlert: null,
            description: description);

        await _maintenanceRepository.AddAsync(maintenance, default);

        // Set T6NextAlert on the previous maintenance for the same device
        if (latestAlert.Date.HasValue)
        {
            var previousMaintenance = await _maintenanceRepository.GetLatestByDeviceIdAsync(device.DeviceId, default);
            if (previousMaintenance != null && previousMaintenance.MaintenanceId != maintenanceId)
            {
                previousMaintenance.SetT6NextAlert(latestAlert.Date.Value);
                await _maintenanceRepository.UpdateBulkAsync(previousMaintenance, default);
            }
        }

        // Mark the source alert as processed
        latestAlert.SetTraiter();
        await _alerteRepository.UpdateBulkAsync(latestAlert, default);

        await _unitOfWork.SaveChangesAsync(default);

        // Re-fetch with navigation properties so the caller has Device/Employee loaded
        maintenance = await _maintenanceRepository.GetOneAsync(maintenanceId, default);

        _logger.LogInformation(
            "Maintenance {MaintenanceId} auto-created from Alert {AlerteId} - T1: {T1}, Device: {DeviceName} (Matricule: {DI}), Employee: {EmployeeName} (RFID: {TagId})",
            maintenanceId.Value,
            latestAlert.AlerteId.Value,
            latestAlert.Date,
            device.DeviceName,
            deviceMatricule,
            $"{employee.Nom} {employee.Prenom}",
            tagId);

        return maintenance;
    }

    private async Task<DeviceCaptureStateRealtimePayload> BuildDeviceCaptureStatePayloadAsync(DeviceId deviceId, string trigger)
    {
        var device = await _deviceRepository.GetOneAsync(deviceId, default);
        var latestMaintenance = await _maintenanceRepository.GetLatestByDeviceIdAsync(deviceId, default);
        var latestAlertsByCode = await _alerteRepository.GetLatestUnprocessedCaptureAlertsByDeviceAsync(deviceId, default);

        var totalCaptures = Math.Max(0, device.NombreCapteur);
        var captureStatuses = new List<string>(totalCaptures);
        var captureLastErrorAt = new List<DateTime?>(totalCaptures);
        var captureAlertLabels = new List<string?>(totalCaptures);

        for (var captureIndex = 1; captureIndex <= totalCaptures; captureIndex++)
        {
            var code = $"A{captureIndex}";
            if (latestAlertsByCode.TryGetValue(code, out var latestAlert))
            {
                captureStatuses.Add("ERROR");
                captureLastErrorAt.Add(latestAlert.Date);
                captureAlertLabels.Add(latestAlert.Type?.Label ?? latestAlert.Type?.Code ?? code);
            }
            else
            {
                captureStatuses.Add("WORKING");
                captureLastErrorAt.Add(null);
                captureAlertLabels.Add(null);
            }
        }

        var workingCaptures = captureStatuses.Count(status => status == "WORKING");
        var capture1Status = totalCaptures >= 1 ? captureStatuses[0] : "NOT_AVAILABLE";
        var capture2Status = totalCaptures >= 2 ? captureStatuses[1] : "NOT_AVAILABLE";
        var capture1LastErrorAt = totalCaptures >= 1 ? captureLastErrorAt[0] : null;
        var capture2LastErrorAt = totalCaptures >= 2 ? captureLastErrorAt[1] : null;

        var maintenanceStartedAt = latestMaintenance?.T2Assignment ?? latestMaintenance?.T3Arrival ?? latestMaintenance?.T1Alerte;
        var maintenanceFinishedAt = latestMaintenance?.T5Confirmation;
        var isUnderMaintenance = latestMaintenance != null && !maintenanceFinishedAt.HasValue;
        var maintenancePhase = ResolveMaintenancePhase(latestMaintenance);
        var maintenancePhaseStartedAt = ResolveMaintenancePhaseStartedAt(latestMaintenance, maintenancePhase);
        var maintenanceCaptureIndex = isUnderMaintenance
            ? ResolveMaintenanceCaptureIndex(latestMaintenance, totalCaptures)
            : null;
        var maintenanceEmployeeName = latestMaintenance != null
            ? $"{latestMaintenance.Employee?.Nom ?? string.Empty} {latestMaintenance.Employee?.Prenom ?? string.Empty}".Trim()
            : null;

        if (isUnderMaintenance && !maintenanceCaptureIndex.HasValue)
        {
            var fallbackProcessedAlert = await _alerteRepository
                .GetLatestProcessedCaptureAlertByDeviceBeforeAsync(
                    deviceId,
                    maintenanceStartedAt,
                    default);

            if (TryGetCaptureIndex(fallbackProcessedAlert?.Type?.Code, out var fallbackIndex)
                && fallbackIndex <= totalCaptures)
            {
                maintenanceCaptureIndex = fallbackIndex;
            }
        }

        var timeline = captureLastErrorAt
            .Concat(new DateTime?[] { maintenanceStartedAt, maintenanceFinishedAt });

        var lastUpdatedAt = timeline
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

    private static string? ResolveMaintenancePhase(Maintenance? maintenance)
    {
        if (maintenance == null)
        {
            return null;
        }

        if (maintenance.T5Confirmation.HasValue)
        {
            return null;
        }

        if (maintenance.T3Arrival.HasValue && maintenance.T4Completion.HasValue)
        {
            return "REPARATION";
        }

        if (maintenance.T3Arrival.HasValue)
        {
            return "DIAGNOSTIC";
        }

        return "AFFECTEE";
    }

    private static DateTime? ResolveMaintenancePhaseStartedAt(Maintenance? maintenance, string? phase)
    {
        if (maintenance == null || string.IsNullOrWhiteSpace(phase))
        {
            return null;
        }

        var normalized = phase.ToUpperInvariant();
        if (normalized == "AFFECTEE")
        {
            return maintenance.T2Assignment ?? maintenance.T1Alerte;
        }

        if (normalized == "DIAGNOSTIC")
        {
            return maintenance.T3Arrival ?? maintenance.T2Assignment ?? maintenance.T1Alerte;
        }

        if (normalized == "REPARATION")
        {
            return maintenance.T4Completion ?? maintenance.T3Arrival ?? maintenance.T2Assignment ?? maintenance.T1Alerte;
        }

        return null;
    }

    private static bool TryGetCaptureIndex(string? code, out int captureIndex)
    {
        captureIndex = 0;
        if (string.IsNullOrWhiteSpace(code) || code.Length < 2 || !code.StartsWith("A", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        return int.TryParse(code[1..], NumberStyles.None, CultureInfo.InvariantCulture, out captureIndex)
               && captureIndex > 0;
    }

    private static int? ResolveMaintenanceCaptureIndex(Maintenance? maintenance, int totalCaptures)
    {
        if (maintenance == null || totalCaptures <= 0 || string.IsNullOrWhiteSpace(maintenance.Description))
        {
            return null;
        }

        const string prefix = "CAPTURE_CODE:";
        if (!maintenance.Description.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        var code = maintenance.Description[prefix.Length..].Trim();
        if (!TryGetCaptureIndex(code, out var captureIndex))
        {
            return null;
        }

        return captureIndex <= totalCaptures ? captureIndex : null;
    }

    private async Task HandleAlarmeResetMessageAsync(string deviceMatricule)
    {
        _logger.LogInformation("Processing ALARME/RESET - Matricule: {Matricule}", deviceMatricule);

        var device = await _deviceRepository.GetByMatriculeAsync(deviceMatricule, default);
        if (device == null)
        {
            _logger.LogWarning("No Device found with Matricule '{Matricule}' for RESET", deviceMatricule);
            return;
        }

        var deletedCount = await _alerteRepository.DeleteAsync(
            a => a.DispositifId == device.DeviceId && !a.Traiter,
            default);

        _logger.LogInformation(
            "ALARME/RESET - Deleted {Count} untreated alert(s) for device '{Matricule}' (DeviceId: {DeviceId})",
            deletedCount, deviceMatricule, device.DeviceId);
    }

    private async Task HandleAlarmeWMessageAsync(string payload, string deviceMatricule)
    {
        _logger.LogInformation("Processing ALARME/W - Matricule: {Matricule}, Payload: {Payload}", deviceMatricule, payload);

        var device = await _deviceRepository.GetByMatriculeAsync(deviceMatricule, default);
        if (device == null)
        {
            _logger.LogWarning("No Device found with Matricule '{Matricule}' for ALARME/W", deviceMatricule);
            return;
        }

        if (!TryParseOnlineStatusText(payload, out var isOnline))
        {
            _logger.LogWarning(
                "Invalid ALARME/W payload for device '{Matricule}'. Expected 'online' or 'offline', received: {Payload}",
                deviceMatricule,
                payload);
            return;
        }

        // Update device online status from MQTT heartbeat payload.
        var now = DateTime.UtcNow;
        device.SetOnlineStatus(isOnline);
        await _deviceRepository.UpdateOnlineStatusAsync(device.DeviceId, isOnline, now, default);

        _logger.LogInformation(
            "Device status updated from ALARME/W - Matricule: {Matricule}, DeviceId: {DeviceId}, IsOnline: {IsOnline}",
            deviceMatricule, device.DeviceId.Value, isOnline);

        // Notify frontend about device status change
        var deviceStatusPayload = new DeviceStatusPayload(
            device.DeviceId.Value,
            device.DeviceName,
            device.Matricule,
            isOnline,
            now
        );

        await _signalService.NotifyDeviceStatusChanged(deviceStatusPayload);
    }

    private static bool TryParseOnlineStatusText(string? value, out bool isOnline)
    {
        isOnline = true;
        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        var normalized = value.Trim().ToUpperInvariant();
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
}

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
