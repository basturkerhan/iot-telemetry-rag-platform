namespace IotPipeline.Platform.Common.Configurations;

public class MqttSettings
{
    public string Host { get; init; } = null!;
    public int Port { get; init; }
    public string Topic { get; init; } = null!;
}
