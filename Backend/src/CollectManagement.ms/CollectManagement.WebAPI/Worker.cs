using CollectManagement.Application.Interfaces.Repositories.Mqtt;

namespace CollectManagement.WebAPI;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly IServiceProvider _serviceProvider;

    public Worker(ILogger<Worker> logger, IServiceProvider serviceProvider)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
    }

    public override async Task StartAsync(CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Starting MQTT Worker...");
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Exception occurred in start: {Message}", exception.Message);
        }

        await base.StartAsync(cancellationToken);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            await using var scope = _serviceProvider.CreateAsyncScope();
            var mqttService = scope.ServiceProvider.GetRequiredService<IMqttService>();

            _logger.LogInformation("Connecting to MQTT broker...");
            await mqttService.ConnectAsync(stoppingToken);

            while (!stoppingToken.IsCancellationRequested)
            {
                if (!mqttService.IsConnected)
                {
                    _logger.LogWarning("MQTT connection lost, attempting to reconnect...");
                    try
                    {
                        await mqttService.ConnectAsync(stoppingToken);
                    }
                    catch (Exception ex)
                    {
                        // Don't terminate the worker; retry on next loop
                        _logger.LogError(ex, "Reconnect attempt failed in worker loop");
                    }
                }

                await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("MQTT Worker is stopping.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in MQTT Worker");
        }
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("MQTT Worker stopping...");
        
        try
        {
            await using var scope = _serviceProvider.CreateAsyncScope();
            var mqttService = scope.ServiceProvider.GetRequiredService<IMqttService>();
            await mqttService.DisconnectAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error disconnecting MQTT on shutdown");
        }

        await base.StopAsync(cancellationToken);
    }
}
