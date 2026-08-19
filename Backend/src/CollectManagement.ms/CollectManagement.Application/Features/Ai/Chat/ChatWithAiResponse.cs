namespace CollectManagement.Application.Features.AI.Chat;

public sealed class ChatWithAiResponse
{
    public string Message { get; init; } = string.Empty;

    public string? DeviceMatricule { get; init; }

    public string? DeviceName { get; init; }

    public string? RiskLevel { get; init; }

    public string? GlobalTrend { get; init; }

    public double? FailureRate { get; init; }

    public string? Recommendation { get; init; }
}