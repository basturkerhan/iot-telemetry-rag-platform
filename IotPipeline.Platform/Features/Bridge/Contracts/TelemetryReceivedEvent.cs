namespace IotPipeline.Platform.Features.Bridge.Contracts;

public record TelemetryReceivedEvent(
    string DeviceId,
    double Temperature,
    double Humidity,
    double Vibration,
    DateTime Timestamp
);
