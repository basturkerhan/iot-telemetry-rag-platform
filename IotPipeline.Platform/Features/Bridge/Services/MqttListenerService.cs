using IotPipeline.Platform.Common.Configurations;
using IotPipeline.Platform.Features.Bridge.Contracts;
using MassTransit;
using Microsoft.Extensions.Options;
using MQTTnet;
using System.Text;
using System.Text.Json;

namespace IotPipeline.Platform.Features.Bridge.Services;

public class MqttListenerService(
    IConfiguration configuration,
    IServiceScopeFactory scopeFactory,
    ILogger<MqttListenerService> logger,
    IOptions<MqttSettings> mqttSettings) : BackgroundService
{
    private readonly IConfiguration _configuration = configuration;
    private readonly IServiceScopeFactory _scopeFactory = scopeFactory;
    private readonly ILogger<MqttListenerService> _logger = logger;
    private readonly MqttSettings _mqttSettings = mqttSettings.Value;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        MqttClientFactory mqttFactory = new();
        string bridgeId = $"Bridge_{Guid.NewGuid():N}";
        JsonSerializerOptions jsonSerializerOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        using var mqttClient = mqttFactory.CreateMqttClient();

        var options = new MqttClientOptionsBuilder()
            .WithTcpServer(_mqttSettings.Host, _mqttSettings.Port)
            .WithClientId(bridgeId)
            .Build();

        mqttClient.ApplicationMessageReceivedAsync += async e =>
        {
            var payload = Encoding.UTF8.GetString(e.ApplicationMessage.Payload);
            try
            {
                TelemetryReceivedEvent? telemetry = JsonSerializer.Deserialize<TelemetryReceivedEvent>(payload, jsonSerializerOptions);

                if (telemetry != null)
                {
                    using var scope = _scopeFactory.CreateScope();
                    var publishEndpoint = scope.ServiceProvider.GetRequiredService<IPublishEndpoint>();
                    await publishEndpoint.Publish(telemetry, stoppingToken);

                    if (_logger.IsEnabled(LogLevel.Information))
                    {
                        _logger.LogInformation("Received from MQTT -> Published to RabbitMQ: {DeviceId}", telemetry.DeviceId);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "MQTT message error");
            }
        };

        await mqttClient.ConnectAsync(options, stoppingToken);
        await mqttClient.SubscribeAsync(_mqttSettings.Topic, cancellationToken: stoppingToken);

        if (_logger.IsEnabled(LogLevel.Information))
        {
            _logger.LogInformation("Bridge Service connected to MQTT Broker and listening to {Topic}", _mqttSettings.Topic);
        }

        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(1000, stoppingToken);
        }
    }
}