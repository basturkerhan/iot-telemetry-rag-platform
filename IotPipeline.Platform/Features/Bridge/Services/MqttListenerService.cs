using IotPipeline.Platform.Common.Configurations;
using IotPipeline.Platform.Features.Bridge.Contracts;
using MassTransit;
using Microsoft.Extensions.Options;
using MQTTnet;
using System.Diagnostics;
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
    private readonly ActivitySource _activitySource = new(configuration["OTEL:ServiceName"] ?? "IotPipeline.Platform");

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
                    ActivityContext parsedContext = default;
                    bool hasParentContext = false;
                    if (!string.IsNullOrEmpty(telemetry.TraceId) && !string.IsNullOrEmpty(telemetry.SpanId))
                    {
                        try
                        {
                            var traceId = ActivityTraceId.CreateFromString(telemetry.TraceId);
                            var spanId = ActivitySpanId.CreateFromString(telemetry.SpanId);

                            if (traceId != default && spanId != default)
                            {
                                parsedContext = new ActivityContext(traceId, spanId, ActivityTraceFlags.Recorded);
                                hasParentContext = true;
                            }
                        }
                        catch {}
                    }

                    using var activity = hasParentContext
                        ? _activitySource.StartActivity("ConsumeFromMqtt", ActivityKind.Consumer, parsedContext)
                        : _activitySource.StartActivity("ConsumeFromMqtt", ActivityKind.Consumer);

                    activity?.SetTag("iot.device.id", telemetry.DeviceId);
                    activity?.SetTag("iot.temperature", telemetry.Temperature);
                    activity?.SetTag("iot.humidity", telemetry.Humidity);
                    activity?.SetTag("iot.vibration", telemetry.Vibration);
                    activity?.SetTag("iot.timestamp", telemetry.Timestamp);

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