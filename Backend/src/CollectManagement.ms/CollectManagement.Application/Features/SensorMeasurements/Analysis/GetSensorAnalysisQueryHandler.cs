using CollectManagement.Application.Interfaces.Repositories.SensorMeasurements;
using CollectManagement.Domain.SensorMeasurements;
using MediatR;

namespace CollectManagement.Application.Features.SensorMeasurements.Analysis;

public sealed class GetSensorAnalysisQueryHandler
    : IRequestHandler<GetSensorAnalysisQuery, GetSensorAnalysisResponse>
{
    private const int RecentMeasurementCount = 10;
    private const int HistoricalMeasurementCount = 50;

    private const double TrendThresholdPercentage = 5.0;

    private readonly ISensorMeasurementRepository _repository;

    public GetSensorAnalysisQueryHandler(
        ISensorMeasurementRepository repository)
    {
        _repository = repository;
    }

    public async Task<GetSensorAnalysisResponse> Handle(
        GetSensorAnalysisQuery request,
        CancellationToken cancellationToken)
    {
        var measurements = await _repository
            .GetForAnalysisAsync(
                request.DeviceId,
                request.SensorCode,
                cancellationToken)
            .ConfigureAwait(false);

        if (measurements.Count == 0)
        {
            return CreateNoDataResponse(request.DeviceId);
        }

        /*
         * IMPORTANT :
         * Le repository retourne déjà les mesures triées par MeasuredAt.
         * On recrée néanmoins une liste triée ici pour garantir
         * le comportement de l'analyse indépendamment du repository.
         */
        var orderedMeasurements = measurements
            .OrderBy(x => x.MeasuredAt)
            .ToList();

        var measurementCount = orderedMeasurements.Count;

        /*
         * Nombre total d'échecs sur toute la période.
         */
        var failureCount = orderedMeasurements.Count(x => x.IsFailure);

        var failureRate = CalculateFailureRate(
            failureCount,
            measurementCount);

        /*
         * ---------------------------------------------------------
         * CONSTRUCTION DES DEUX PÉRIODES
         * ---------------------------------------------------------
         *
         * Exemple avec 2512 mesures :
         *
         * Historique : mesures 2453 → 2502 = 50 mesures
         * Récent     : mesures 2503 → 2512 = 10 mesures
         *
         * Les deux périodes ne se chevauchent PAS.
         */

        var recentMeasurements = orderedMeasurements
            .TakeLast(
                Math.Min(
                    RecentMeasurementCount,
                    measurementCount))
            .ToList();

        var historicalAvailableCount =
            Math.Max(
                0,
                measurementCount - recentMeasurements.Count);

        var historicalTakeCount =
            Math.Min(
                HistoricalMeasurementCount,
                historicalAvailableCount);

        var historicalMeasurements = orderedMeasurements
            .Skip(
                historicalAvailableCount -
                historicalTakeCount)
            .Take(historicalTakeCount)
            .ToList();

        /*
         * Analyse des quatre indicateurs.
         */
        var temperature = AnalyzeMetric(
            orderedMeasurements,
            historicalMeasurements,
            recentMeasurements,
            x => x.Temperature);

        var vibration = AnalyzeMetric(
            orderedMeasurements,
            historicalMeasurements,
            recentMeasurements,
            x => x.Vibration);

        var pressure = AnalyzeMetric(
            orderedMeasurements,
            historicalMeasurements,
            recentMeasurements,
            x => x.Pressure);

        var humidity = AnalyzeMetric(
            orderedMeasurements,
            historicalMeasurements,
            recentMeasurements,
            x => x.Humidity);

        /*
         * Tendance globale.
         */
        var globalTrend = CalculateGlobalTrend(
            temperature,
            vibration,
            pressure,
            humidity);

        /*
         * Niveau de risque.
         */
        var riskLevel = CalculateRiskLevel(
            failureRate,
            temperature,
            vibration,
            pressure,
            humidity);

        /*
         * Recommandation.
         */
        var recommendation = GenerateRecommendation(
            riskLevel,
            globalTrend,
            temperature,
            vibration,
            pressure,
            humidity);

        return new GetSensorAnalysisResponse
        {
            DeviceId = request.DeviceId,

            MeasurementCount = measurementCount,

            FailureCount = failureCount,

            FailureRate = failureRate,

            Temperature = temperature,

            Vibration = vibration,

            Pressure = pressure,

            Humidity = humidity,

            GlobalTrend = globalTrend,

            RiskLevel = riskLevel,

            Recommendation = recommendation
        };
    }

    private static GetSensorAnalysisResponse CreateNoDataResponse(
        Ulid deviceId)
    {
        return new GetSensorAnalysisResponse
        {
            DeviceId = deviceId,

            MeasurementCount = 0,

            FailureCount = 0,

            FailureRate = 0,

            Temperature = new SensorMetricAnalysis(),

            Vibration = new SensorMetricAnalysis(),

            Pressure = new SensorMetricAnalysis(),

            Humidity = new SensorMetricAnalysis(),

            GlobalTrend = "NoData",

            RiskLevel = "Unknown",

            Recommendation =
                "Aucune donnée disponible pour cette machine."
        };
    }

    private static double CalculateFailureRate(
        int failureCount,
        int measurementCount)
    {
        if (measurementCount <= 0)
        {
            return 0;
        }

        return Math.Round(
            failureCount * 100.0 / measurementCount,
            2);
    }

    private static SensorMetricAnalysis AnalyzeMetric(
        IReadOnlyList<SensorMeasurement> allMeasurements,
        IReadOnlyList<SensorMeasurement> historicalMeasurements,
        IReadOnlyList<SensorMeasurement> recentMeasurements,
        Func<SensorMeasurement, double?> selector)
    {
        /*
         * Toutes les valeurs disponibles.
         */
        var allValues = GetValues(
            allMeasurements,
            selector);

        /*
         * Valeurs de référence historique.
         */
        var historicalValues = GetValues(
            historicalMeasurements,
            selector);

        /*
         * Valeurs récentes.
         */
        var recentValues = GetValues(
            recentMeasurements,
            selector);

        /*
         * Aucune donnée pour ce capteur.
         */
        if (allValues.Count == 0)
        {
            return new SensorMetricAnalysis();
        }

        /*
         * Statistiques globales.
         */
        var average = allValues.Average();

        var minimum = allValues.Min();

        var maximum = allValues.Max();

        /*
         * Impossible de comparer les périodes
         * si l'une des deux périodes ne contient
         * aucune valeur pour ce capteur.
         */
        if (historicalValues.Count == 0 ||
            recentValues.Count == 0)
        {
            return new SensorMetricAnalysis
            {
                Average = Round(average),

                Minimum = Round(minimum),

                Maximum = Round(maximum),

                RecentAverage = null,

                HistoricalAverage = null,

                VariationPercentage = null,

                Trend = "NoData"
            };
        }

        /*
         * Moyenne historique.
         */
        var historicalAverage =
            historicalValues.Average();

        /*
         * Moyenne récente.
         */
        var recentAverage =
            recentValues.Average();

        /*
         * Calcul du pourcentage de variation.
         *
         * Exemple :
         *
         * historique = 50
         * récent     = 45
         *
         * variation =
         * ((45 - 50) / 50) * 100
         * = -10 %
         */
        var variationPercentage =
            CalculateVariationPercentage(
                historicalAverage,
                recentAverage);

        /*
         * Détermination de la tendance.
         */
        var trend = DetermineTrend(
            variationPercentage);

        return new SensorMetricAnalysis
        {
            Average = Round(average),

            Minimum = Round(minimum),

            Maximum = Round(maximum),

            HistoricalAverage =
                Round(historicalAverage),

            RecentAverage =
                Round(recentAverage),

            VariationPercentage =
                Round(variationPercentage),

            Trend = trend
        };
    }

    private static List<double> GetValues(
        IReadOnlyList<SensorMeasurement> measurements,
        Func<SensorMeasurement, double?> selector)
    {
        return measurements
            .Select(selector)
            .Where(value => value.HasValue &&
                            !double.IsNaN(value.Value) &&
                            !double.IsInfinity(value.Value))
            .Select(value => value!.Value)
            .ToList();
    }

    private static double CalculateVariationPercentage(
        double historicalAverage,
        double recentAverage)
    {
        /*
         * Cas particulier :
         *
         * historique = 0
         * récent = 0
         *
         * aucune variation.
         */
        if (historicalAverage == 0)
        {
            return recentAverage == 0
                ? 0
                : 100;
        }

        return (
            (recentAverage - historicalAverage)
            / Math.Abs(historicalAverage)
        ) * 100;
    }

    private static string DetermineTrend(
        double variationPercentage)
    {
        if (variationPercentage >
            TrendThresholdPercentage)
        {
            return "Increasing";
        }

        if (variationPercentage <
            -TrendThresholdPercentage)
        {
            return "Decreasing";
        }

        return "Stable";
    }

    private static string CalculateGlobalTrend(
        SensorMetricAnalysis temperature,
        SensorMetricAnalysis vibration,
        SensorMetricAnalysis pressure,
        SensorMetricAnalysis humidity)
    {
        var metrics = new[]
        {
            temperature,
            vibration,
            pressure,
            humidity
        };

        var validMetrics = metrics
            .Where(x => x.Trend != "NoData")
            .ToList();

        if (validMetrics.Count == 0)
        {
            return "NoData";
        }

        var increasingCount = validMetrics.Count(
            x => x.Trend == "Increasing");

        var decreasingCount = validMetrics.Count(
            x => x.Trend == "Decreasing");

        /*
         * Au moins deux indicateurs augmentent
         * => tendance globale de dégradation.
         */
        if (increasingCount >= 2)
        {
            return "Degradation";
        }

        /*
         * Au moins deux indicateurs diminuent
         * => tendance globale d'amélioration.
         */
        if (decreasingCount >= 2)
        {
            return "Improvement";
        }

        return "Stable";
    }

    private static string CalculateRiskLevel(
        double failureRate,
        SensorMetricAnalysis temperature,
        SensorMetricAnalysis vibration,
        SensorMetricAnalysis pressure,
        SensorMetricAnalysis humidity)
    {
        var score = 0;

        /*
         * ---------------------------------------------------------
         * SCORE DES ÉCHECS
         * ---------------------------------------------------------
         *
         * < 2 %       => +0
         * 2 % - <5 %  => +2
         * >= 5 %      => +3
         */
        if (failureRate >= 5)
        {
            score += 3;
        }
        else if (failureRate >= 2)
        {
            score += 2;
        }

        /*
         * ---------------------------------------------------------
         * SCORE DES TENDANCES
         * ---------------------------------------------------------
         *
         * Chaque indicateur en augmentation ajoute 1 point.
         */
        score += CountIncreasing(
            temperature,
            vibration,
            pressure,
            humidity);

        /*
         * ---------------------------------------------------------
         * NIVEAU DE RISQUE
         * ---------------------------------------------------------
         */
        if (score >= 5)
        {
            return "High";
        }

        if (score >= 3)
        {
            return "Moderate";
        }

        return "Low";
    }

    private static int CountIncreasing(
        params SensorMetricAnalysis[] metrics)
    {
        return metrics.Count(
            metric => metric.Trend == "Increasing");
    }

    private static string GenerateRecommendation(
        string riskLevel,
        string globalTrend,
        SensorMetricAnalysis temperature,
        SensorMetricAnalysis vibration,
        SensorMetricAnalysis pressure,
        SensorMetricAnalysis humidity)
    {
        var increasingSensors = new List<string>();

        if (temperature.Trend == "Increasing")
        {
            increasingSensors.Add("température");
        }

        if (vibration.Trend == "Increasing")
        {
            increasingSensors.Add("vibration");
        }

        if (pressure.Trend == "Increasing")
        {
            increasingSensors.Add("pression");
        }

        if (humidity.Trend == "Increasing")
        {
            increasingSensors.Add("humidité");
        }

        /*
         * RISQUE ÉLEVÉ
         */
        if (riskLevel == "High")
        {
            if (increasingSensors.Count > 0)
            {
                return
                    $"Risque élevé. Surveiller particulièrement : " +
                    $"{string.Join(", ", increasingSensors)}. " +
                    "Une intervention préventive est recommandée.";
            }

            return
                "Risque élevé. " +
                "Une intervention préventive est recommandée.";
        }

        /*
         * RISQUE MODÉRÉ
         */
        if (riskLevel == "Moderate")
        {
            if (increasingSensors.Count > 0)
            {
                return
                    $"Risque modéré. Surveiller : " +
                    $"{string.Join(", ", increasingSensors)}. " +
                    "Une intervention préventive est recommandée " +
                    "si la tendance continue.";
            }

            return
                "Risque modéré. " +
                "Une surveillance renforcée est recommandée.";
        }

        /*
         * RISQUE FAIBLE + AMÉLIORATION
         */
        if (globalTrend == "Improvement")
        {
            return
                "Les indicateurs montrent une amélioration. " +
                "Maintenir la surveillance habituelle.";
        }

        /*
         * RISQUE FAIBLE + STABILITÉ
         */
        if (globalTrend == "Stable")
        {
            return
                "Les indicateurs sont globalement stables. " +
                "Maintenir la surveillance préventive.";
        }

        /*
         * Cas général.
         */
        return
            "Maintenir la surveillance de la machine.";
    }

    private static double Round(double value)
    {
        return Math.Round(value, 2);
    }
}