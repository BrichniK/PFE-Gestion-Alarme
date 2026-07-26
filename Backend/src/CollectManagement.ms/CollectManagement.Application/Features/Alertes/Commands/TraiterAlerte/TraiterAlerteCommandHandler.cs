using CollectManagement.Application.Interfaces.Repositories.Alertes;
using CollectManagement.Application.Interfaces.Repositories.Maintenances;
using CollectManagement.Application.Interfaces.Repositories.Plannings;
using CollectManagement.Application.Interfaces.Repositories.SMS;
using CollectManagement.Application.Interfaces.Repositories.SMSConfigurations;
using CollectManagement.Application.Interfaces.Services;
using CollectManagement.Domain.Alertes.ValueObjects;
using CollectManagement.Domain.Maintenances;
using CollectManagement.Domain.Maintenances.ObjectValues;
using System.Globalization;
using CollectManagement.Application.Interfaces.Employees;
using CollectManagement.Domain.Employess.ObjectValues;

namespace CollectManagement.Application.Features.Alertes.Commands.TraiterAlerte;

public class TraiterAlerteCommandHandler
    : IRequestHandler<TraiterAlerteCommand, bool>
{
    private readonly IAlerteRepository _alerteRepository;
    private readonly IMaintenanceRepository _maintenanceRepository;
    private readonly IEmployeeRepository _employeeRepository;
    private readonly IMqttPublisher _mqttPublisher;
    private readonly ISMSRepository _smsRepository;
    private readonly ISMSService _smsService;
    private readonly IPlanningRepository _planningRepository;
    private readonly ISMSConfigurationRepository _smsConfigurationRepository;
    private readonly IEmailService _emailService;
    private readonly ISignalService _signalService;

    public TraiterAlerteCommandHandler(
        IAlerteRepository alerteRepository,
        IMaintenanceRepository maintenanceRepository,
        IEmployeeRepository employeeRepository,
        IMqttPublisher mqttPublisher,
        ISMSRepository smsRepository,
        ISMSService smsService,
        IPlanningRepository planningRepository,
        ISMSConfigurationRepository smsConfigurationRepository,
        IEmailService emailService,
        ISignalService signalService)
    {
        _alerteRepository = alerteRepository;
        _maintenanceRepository = maintenanceRepository;
        _employeeRepository = employeeRepository;
        _mqttPublisher = mqttPublisher;
        _smsRepository = smsRepository;
        _smsService = smsService;
        _planningRepository = planningRepository;
        _smsConfigurationRepository = smsConfigurationRepository;
        _emailService = emailService;
        _signalService = signalService;
    }

    public async Task<bool> Handle(TraiterAlerteCommand request, CancellationToken cancellationToken)
    {
        var alerteId = new AlerteId(request.AlerteId);
        var alerte = await _alerteRepository.GetOneAsync(alerteId, cancellationToken);

        if (alerte == null)
            return false;

        var employeeId = new EmployeeId(request.EmployeeId);
        var employee = await _employeeRepository.GetOneAsync(employeeId, cancellationToken);

        if (employee == null)
            return false;

        // Mark alerte as processed
        alerte.SetTraiter();
        await _alerteRepository.UpdateBulkAsync(alerte, cancellationToken);

        // Create maintenance record
        var maintenanceId = new MaintenanceId(Ulid.NewUlid());

        var maintenance = Maintenance.Create(
            maintenanceId,
            alerte.DispositifId,
            employeeId,
            alerte.Date,              // T1 = alerte date
            DateTime.Now,          // T2 = now
            null,                     // T3 = null
            null,                     // T4 = null
            null,                     // T5 = null
            null,                     // T6 = null
            BuildMaintenanceDescription(alerte.Type?.Code)
        );

        await _maintenanceRepository
            .AddAsync(maintenance, cancellationToken)
            .ConfigureAwait(false);

        // Set T6NextAlert on the previous maintenance for the same device
        if (alerte.Date.HasValue)
        {
            var previousMaintenance = await _maintenanceRepository.GetLatestByDeviceIdAsync(alerte.DispositifId, cancellationToken);
            if (previousMaintenance != null && previousMaintenance.MaintenanceId != maintenanceId)
            {
                previousMaintenance.SetT6NextAlert(alerte.Date.Value);
                await _maintenanceRepository.UpdateBulkAsync(previousMaintenance, cancellationToken);
            }
        }

        // Check SMS configuration — only send if SMS is active and traitement event is allowed
        var smsConfig = await _smsConfigurationRepository.GetConfigurationAsync(cancellationToken);
        var smsIsActive = smsConfig?.IsActive ?? false;
        var smsApiUrl = smsConfig?.ApiUrl;
        var smsOnTraitement = smsConfig?.SmsOnTraitement ?? true;

        if (alerte.Dispositif != null && smsIsActive && smsOnTraitement && !string.IsNullOrWhiteSpace(smsApiUrl))
        {
            // 1. Get phone numbers from SMS table (configured recipients for the device)
            var smsRecipients = await _smsRepository.GetByDeviceIdAsync(alerte.DispositifId, cancellationToken);
            var phoneNumbers = smsRecipients
                .Select(s => s.PhoneNumber)
                .Distinct()
                .ToList();

            // 2. Get employee phone numbers and emails filtered by planning (same date + same device)
            var emailAddresses = new List<string>();

            if (alerte.Date.HasValue)
            {
                var planningEmployees = await _planningRepository.GetEmployeesByDateAndDeviceAsync(
                    alerte.Date.Value,
                    alerte.DispositifId,
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

                // Collect email addresses from planning employees
                emailAddresses = planningEmployees
                    .Where(e => !string.IsNullOrWhiteSpace(e.Email))
                    .Select(e => e.Email!)
                    .Distinct()
                    .ToList();
            }

            // Also add the assigned employee's email
            if (!string.IsNullOrWhiteSpace(employee.Email) && !emailAddresses.Contains(employee.Email))
            {
                emailAddresses.Add(employee.Email);
            }

            var deviceName = alerte.Dispositif.DeviceName;
            var typeLabelOrCode = alerte.Type?.Label ?? alerte.Type?.Code ?? "anomalie";
            var employeeFullName = $"{employee.Nom} {employee.Prenom}".Trim();

            var notificationText =
                $"Maintenance assignée à {employeeFullName}.\n" +
                $"Équipement : {deviceName}.\n" +
                $"Type d'alerte : {typeLabelOrCode}.\n" +
                $"Merci d'intervenir pour corriger le problème.";

            // Send SMS
            if (phoneNumbers.Any())
            {
                await _smsService.SendSMSAsync(phoneNumbers, notificationText, smsApiUrl, cancellationToken);
            }

            // Send Email
            if (emailAddresses.Any())
            {
                var emailSubject = $"Alerte Maintenance - {deviceName} ({typeLabelOrCode})";
                await _emailService.SendEmailAsync(emailAddresses, emailSubject, notificationText, cancellationToken);
            }
        }

        // Publish MQTT message to device
        var matricule = alerte.Dispositif?.Matricule;
        if (!string.IsNullOrWhiteSpace(matricule))
        {
            var topic = $"ALARME/PC/{matricule}";
            var payload = new
            {
                TI = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                NomEmployee = $"{employee.Nom} {employee.Prenom}",
                TagId = employee.Rfid,
                DI = matricule
            };

            await _mqttPublisher.PublishAsync(topic, payload);
        }

        await _signalService.NotifyMaintenanceUpdated();

        return true;
    }

    private static string BuildMaintenanceDescription(string? captureCode)
    {
        if (string.IsNullOrWhiteSpace(captureCode))
        {
            return string.Empty;
        }

        if (!captureCode.StartsWith("A", StringComparison.OrdinalIgnoreCase))
        {
            return string.Empty;
        }

        if (!int.TryParse(captureCode[1..], NumberStyles.None, CultureInfo.InvariantCulture, out var captureIndex)
            || captureIndex <= 0)
        {
            return string.Empty;
        }

        return $"CAPTURE_CODE:A{captureIndex}";
    }
}

