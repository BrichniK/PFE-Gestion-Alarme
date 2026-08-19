export interface SensorMetricAnalysis {
    average: number | null;
    minimum: number | null;
    maximum: number | null;
    recentAverage: number | null;
    historicalAverage: number | null;
    variationPercentage: number | null;
    trend: string;
}

export interface DeviceAnalysis {
    deviceId: string;
    measurementCount: number;
    failureCount: number;
    failureRate: number;

    temperature: SensorMetricAnalysis;
    vibration: SensorMetricAnalysis;
    pressure: SensorMetricAnalysis;
    humidity: SensorMetricAnalysis;

    globalTrend: string;
    riskLevel: string;
    recommendation: string;
}