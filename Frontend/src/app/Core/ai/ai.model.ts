export type AiRiskLevel =
    | 'Low'
    | 'Moderate'
    | 'High'
    | 'Unknown';

export type AiTrend =
    | 'Increasing'
    | 'Decreasing'
    | 'Stable'
    | 'NoData';

export type AiGlobalTrend =
    | 'Improvement'
    | 'Degradation'
    | 'Stable'
    | 'NoData';

export interface AiMetricAnalysis {
    average: number | null;
    minimum: number | null;
    maximum: number | null;
    recentAverage: number | null;
    historicalAverage: number | null;
    variationPercentage: number | null;
    trend: AiTrend;
}

export interface AiPrediction {
    deviceId: string;

    measurementCount: number;
    failureCount: number;
    failureRate: number;

    temperature: AiMetricAnalysis;
    vibration: AiMetricAnalysis;
    pressure: AiMetricAnalysis;
    humidity: AiMetricAnalysis;

    globalTrend: AiGlobalTrend;
    riskLevel: AiRiskLevel;
    recommendation: string;
}