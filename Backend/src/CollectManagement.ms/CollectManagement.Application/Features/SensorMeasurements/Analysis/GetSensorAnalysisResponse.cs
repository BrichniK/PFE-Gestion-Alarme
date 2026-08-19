namespace CollectManagement.Application.Features.SensorMeasurements.Analysis;
public sealed class GetSensorAnalysisResponse
{
    public Ulid DeviceId { get; init; }

    public int MeasurementCount { get; init; }

    public int FailureCount { get; init; }

    public double FailureRate { get; init; }

    public SensorMetricAnalysis Temperature { get; init; } = new();

    public SensorMetricAnalysis Vibration { get; init; } = new();

    public SensorMetricAnalysis Pressure { get; init; } = new();

    public SensorMetricAnalysis Humidity { get; init; } = new();

    public string GlobalTrend { get; init; } = string.Empty;

    public string RiskLevel { get; init; } = string.Empty;

    public string Recommendation { get; init; } = string.Empty;
}

public record SensorMetricAnalysis
{
    public double? Average { get; init; }

    public double? Minimum { get; init; }

    public double? Maximum { get; init; }

    public double? RecentAverage { get; init; }

    public double? HistoricalAverage { get; init; }

    public double? VariationPercentage { get; init; }

    public string Trend { get; init; } = "NoData";
}