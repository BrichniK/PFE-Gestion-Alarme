using CollectManagement.Application.Features.SensorMeasurements.Analysis;
using CollectManagement.Application.Interfaces.Repositories.Alertes;
using CollectManagement.Application.Interfaces.Repositories.Devices;
using CollectManagement.Application.Interfaces.Services;
using CollectManagement.Domain.Alertes;
using CollectManagement.Domain.Devices;
using MediatR;

namespace CollectManagement.Application.Features.AI.Chat;

public sealed class ChatWithAiQueryHandler
    : IRequestHandler<ChatWithAiQuery, ChatWithAiResponse>
{
    private readonly IDeviceRepository _deviceRepository;
    private readonly IAlerteRepository _alerteRepository;
    private readonly ISender _sender;
    private readonly IAiService _aiService;

    public ChatWithAiQueryHandler(
        IDeviceRepository deviceRepository,
        IAlerteRepository alerteRepository,
        ISender sender,
        IAiService aiService)
    {
        _deviceRepository = deviceRepository;
        _alerteRepository = alerteRepository;
        _sender = sender;
        _aiService = aiService;
    }

    public async Task<ChatWithAiResponse> Handle(
        ChatWithAiQuery request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Message))
        {
            return new ChatWithAiResponse
            {
                Message = "Veuillez saisir une question."
            };
        }

        /*
         * 1. Identification du dispositif
         *
         * Exemple :
         * "Est-ce que MACHINE001 présente un risque de panne ?"
         *
         * -> MACHINE001
         */
        var matricule = ExtractDeviceMatricule(request.Message);

        if (string.IsNullOrWhiteSpace(matricule))
        {
            return new ChatWithAiResponse
            {
                Message =
                    "Je peux analyser un dispositif industriel. " +
                    "Veuillez préciser son matricule, par exemple : " +
                    "\"Est-ce que MACHINE001 présente un risque de panne ?\""
            };
        }

        /*
         * 2. Recherche du dispositif
         */
        var device = await _deviceRepository
            .GetByMatriculeAsync(
                matricule,
                cancellationToken)
            .ConfigureAwait(false);

        if (device is null)
        {
            return new ChatWithAiResponse
            {
                DeviceMatricule = matricule,

                Message =
                    $"Aucun dispositif avec le matricule '{matricule}' " +
                    "n'a été trouvé."
            };
        }

        /*
         * 3. Analyse des mesures du dispositif
         *
         * On réutilise le moteur d'analyse existant.
         *
         * Le handler d'analyse fournit :
         * - nombre de mesures
         * - nombre d'échecs
         * - taux d'échec
         * - risque
         * - tendance
         * - température
         * - vibration
         * - pression
         * - humidité
         * - recommandations
         */
        var analysis = await _sender
            .Send(
                new GetSensorAnalysisQuery(
                    device.DeviceId.Value),
                cancellationToken)
            .ConfigureAwait(false);

        /*
         * 4. Récupération des dernières alertes non traitées
         *
         * On utilise uniquement une méthode réellement
         * présente dans IAlerteRepository.
         */
        var alerts = await _alerteRepository
            .GetLatestUnprocessedCaptureAlertsByDeviceAsync(
                device.DeviceId,
                cancellationToken)
            .ConfigureAwait(false);

        /*
         * 5. Construction du contexte destiné à Gemini
         */
        var context = BuildAiContext(
            device,
            analysis,
            alerts);

        /*
         * 6. Génération de la réponse IA
         *
         * Gemini n'effectue pas les calculs métier.
         *
         * Il interprète les données calculées
         * par notre application.
         */
        var message = await _aiService
            .GenerateResponseAsync(
                request.Message,
                context,
                cancellationToken)
            .ConfigureAwait(false);

        /*
         * 7. Réponse API
         */
        return new ChatWithAiResponse
        {
            Message = message,

            DeviceMatricule = device.Matricule,

            DeviceName = device.DeviceName,

            RiskLevel = analysis.RiskLevel,

            GlobalTrend = analysis.GlobalTrend,

            FailureRate = analysis.FailureRate,

            Recommendation = analysis.Recommendation
        };
    }

    /// <summary>
    /// Construit le contexte métier envoyé au service IA.
    ///
    /// IMPORTANT :
    /// Gemini ne reçoit pas directement la base de données.
    /// Il reçoit uniquement les informations nécessaires
    /// calculées par notre application.
    /// </summary>
    private static string BuildAiContext(
        Device device,
        GetSensorAnalysisResponse analysis,
        IReadOnlyDictionary<string, Alerte> alerts)
    {
        var alertContext = BuildAlertContext(alerts);

        return $"""
                ================================
                CONTEXTE DU DISPOSITIF INDUSTRIEL
                ================================

                IDENTIFICATION
                --------------------------------

                Matricule :
                {device.Matricule}

                Nom :
                {device.DeviceName}

                Nombre de capteurs :
                {device.NombreCapteur}

                État de connexion :
                {(device.IsOnline ? "En ligne" : "Hors ligne")}

                Dernière activité :
                {FormatDate(device.LastSeen)}


                ================================
                DONNÉES HISTORIQUES
                ================================

                Nombre de mesures analysées :
                {analysis.MeasurementCount}

                Nombre d'échecs :
                {analysis.FailureCount}

                Taux d'échec :
                {analysis.FailureRate:F2} %


                ================================
                ÉVALUATION DU RISQUE
                ================================

                Niveau de risque calculé par le système :
                {analysis.RiskLevel}

                Tendance globale :
                {analysis.GlobalTrend}

                Recommandation calculée par le système :
                {analysis.Recommendation}


                ================================
                TEMPÉRATURE
                ================================

                Moyenne :
                {analysis.Temperature.Average}

                Minimum :
                {analysis.Temperature.Minimum}

                Maximum :
                {analysis.Temperature.Maximum}

                Moyenne historique :
                {analysis.Temperature.HistoricalAverage}

                Moyenne récente :
                {analysis.Temperature.RecentAverage}

                Variation :
                {analysis.Temperature.VariationPercentage} %

                Tendance :
                {analysis.Temperature.Trend}


                ================================
                VIBRATION
                ================================

                Moyenne :
                {analysis.Vibration.Average}

                Minimum :
                {analysis.Vibration.Minimum}

                Maximum :
                {analysis.Vibration.Maximum}

                Moyenne historique :
                {analysis.Vibration.HistoricalAverage}

                Moyenne récente :
                {analysis.Vibration.RecentAverage}

                Variation :
                {analysis.Vibration.VariationPercentage} %

                Tendance :
                {analysis.Vibration.Trend}


                ================================
                PRESSION
                ================================

                Moyenne :
                {analysis.Pressure.Average}

                Minimum :
                {analysis.Pressure.Minimum}

                Maximum :
                {analysis.Pressure.Maximum}

                Moyenne historique :
                {analysis.Pressure.HistoricalAverage}

                Moyenne récente :
                {analysis.Pressure.RecentAverage}

                Variation :
                {analysis.Pressure.VariationPercentage} %

                Tendance :
                {analysis.Pressure.Trend}


                ================================
                HUMIDITÉ
                ================================

                Moyenne :
                {analysis.Humidity.Average}

                Minimum :
                {analysis.Humidity.Minimum}

                Maximum :
                {analysis.Humidity.Maximum}

                Moyenne historique :
                {analysis.Humidity.HistoricalAverage}

                Moyenne récente :
                {analysis.Humidity.RecentAverage}

                Variation :
                {analysis.Humidity.VariationPercentage} %

                Tendance :
                {analysis.Humidity.Trend}


                ================================
                ALERTES
                ================================

                {alertContext}


                ================================
                RÈGLES D'INTERPRÉTATION POUR L'IA
                ================================

                1. Utiliser uniquement les informations présentes
                   dans le contexte.

                2. Ne jamais inventer une mesure.

                3. Ne jamais inventer une alerte.

                4. Ne jamais inventer une maintenance.

                5. Ne jamais inventer une intervention technique.

                6. Ne jamais affirmer qu'une panne est certaine.

                7. Le niveau de risque calculé par le système
                   doit être respecté.

                8. Les statistiques des capteurs doivent être
                   utilisées pour expliquer la situation.

                9. Les alertes doivent être utilisées comme
                   informations complémentaires.

                10. Une tendance ne constitue pas une certitude
                    de panne.

                11. Si les données sont insuffisantes pour répondre,
                    le préciser clairement.

                12. Répondre en français.

                13. Donner une réponse claire, professionnelle
                    et adaptée à la maintenance industrielle.
                """;
    }

    /// <summary>
    /// Transforme les alertes en texte compréhensible par l'IA.
    /// </summary>
    private static string BuildAlertContext(
        IReadOnlyDictionary<string, Alerte> alerts)
    {
        if (alerts.Count == 0)
        {
            return
                "Aucune alerte non traitée récente " +
                "n'est disponible pour ce dispositif.";
        }

        var lines = new List<string>();

        foreach (var entry in alerts)
        {
            var captureCode = entry.Key;
            var alert = entry.Value;

            lines.Add(
                $"""
                - Code de capture : {captureCode}
                  Date : {FormatDate(alert.Date)}
                  Traité : {(alert.Traiter ? "Oui" : "Non")}
                  TypeId : {alert.TypeId}
                """);
        }

        return string.Join(
            Environment.NewLine,
            lines);
    }

    /// <summary>
    /// Formate les dates de manière stable avant envoi à l'IA.
    /// </summary>
    private static string FormatDate(DateTime? date)
    {
        return date?.ToString("yyyy-MM-dd HH:mm:ss")
               ?? "Inconnue";
    }

    /// <summary>
    /// Extrait un matricule de type :
    ///
    /// MACHINE001
    /// DEV001
    /// DEV-001
    /// DEV_001
    /// MACHINE-001
    /// </summary>
    private static string? ExtractDeviceMatricule(
        string message)
    {
        var match = System.Text.RegularExpressions.Regex.Match(
            message,
            @"\b[A-Za-z]+[-_]?\d+\b",
            System.Text.RegularExpressions.RegexOptions.IgnoreCase);

        return match.Success
            ? match.Value
            : null;
    }
}