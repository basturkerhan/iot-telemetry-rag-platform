namespace IotPipeline.Platform.Common.Configurations;

public class RabbitMQSettings
{
    public string Host { get; init; } = null!;
    public string Username { get; init; } = null!;
    public string Password { get; init; } = null!;
    public int Port { get; init; }
}
