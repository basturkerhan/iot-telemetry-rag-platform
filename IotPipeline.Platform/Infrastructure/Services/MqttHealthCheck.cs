using IotPipeline.Platform.Common.Configurations;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using MQTTnet;

namespace IotPipeline.Platform.Infrastructure.Services;

public class MqttHealthCheck(IOptions<MqttSettings> mqttSettings) : IHealthCheck
{
    private readonly MqttSettings _settings = mqttSettings.Value;

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var factory = new MqttClientFactory();
            using var mqttClient = factory.CreateMqttClient();

            var options = new MqttClientOptionsBuilder()
                .WithTcpServer(_settings.Host, _settings.Port)
                .WithTimeout(TimeSpan.FromSeconds(2))
                .Build();

            using var timeoutCts = new CancellationTokenSource(TimeSpan.FromSeconds(3));
            using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutCts.Token);

            var connectResult = await mqttClient.ConnectAsync(options, linkedCts.Token);

            if (connectResult.ResultCode == MqttClientConnectResultCode.Success)
            {
                await mqttClient.DisconnectAsync(cancellationToken: cancellationToken);
                return HealthCheckResult.Healthy("Online");
            }

            return HealthCheckResult.Unhealthy($"Offline: {connectResult.ResultCode}");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("Offline", ex);
        }
    }
}
