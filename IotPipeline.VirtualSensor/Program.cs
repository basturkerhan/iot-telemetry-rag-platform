using Microsoft.Extensions.Configuration;
using MQTTnet;
using System.Text.Json;

var configuration = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddEnvironmentVariables()
    .Build();

var mqttHost = configuration["MqttSettings:Host"] ?? "localhost";
var mqttPort = int.Parse(configuration["MqttSettings:Port"] ?? "1883");
var interval = int.Parse(configuration["MqttSettings:PublishIntervalMs"] ?? "2000");
var topicPrefix = configuration["MqttSettings:TopicPrefix"] ?? "factory/telemetry";

// 2. MQTT Client
var mqttFactory = new MqttClientFactory();
using var mqttClient = mqttFactory.CreateMqttClient();

var mqttOptions = new MqttClientOptionsBuilder()
    .WithTcpServer(mqttHost, mqttPort)
    .WithClientId($"Simulator_{Guid.NewGuid():N}")
    .Build();

Console.WriteLine($"MQTT Broker'a bağlanılıyor ({mqttHost}:{mqttPort})...");
await mqttClient.ConnectAsync(mqttOptions);
Console.WriteLine("Bağlantı başarılı! Sensör verileri gönderiliyor... (Durdurmak için CTRL+C)");

var random = new Random();
var deviceIds = new[] { "DEV-101", "DEV-102", "DEV-103" };

while (true)
{
    foreach (var deviceId in deviceIds)
    {
        var telemetryData = new
        {
            DeviceId = deviceId,
            Temperature = Math.Round(20.0 + (random.NextDouble() * 15.0), 2),
            Humidity = Math.Round(40.0 + (random.NextDouble() * 30.0), 2),
            Vibration = Math.Round(random.NextDouble() * 5.0, 2),
            Timestamp = DateTime.UtcNow
        };

        string jsonPayload = JsonSerializer.Serialize(telemetryData);

        var message = new MqttApplicationMessageBuilder()
            .WithTopic($"{topicPrefix}/{deviceId}")
            .WithPayload(jsonPayload)
            .WithQualityOfServiceLevel(MQTTnet.Protocol.MqttQualityOfServiceLevel.AtLeastOnce)
            .Build();

        await mqttClient.PublishAsync(message);
        Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] Yayınlandı ({deviceId}): {jsonPayload}");
    }

    await Task.Delay(interval);
}