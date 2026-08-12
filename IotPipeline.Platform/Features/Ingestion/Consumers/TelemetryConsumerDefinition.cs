using MassTransit;

namespace IotPipeline.Platform.Features.Ingestion.Consumers;

public class TelemetryConsumerDefinition : ConsumerDefinition<TelemetryConsumer>
{
    public TelemetryConsumerDefinition()
    {
        Endpoint(e => e.PrefetchCount = 200);
    }

    protected override void ConfigureConsumer(
        IReceiveEndpointConfigurator endpointConfigurator,
        IConsumerConfigurator<TelemetryConsumer> consumerConfigurator,
        IRegistrationContext context)
    {
        consumerConfigurator.Options<BatchOptions>(options => options
            .SetMessageLimit(100)
            .SetTimeLimit(TimeSpan.FromSeconds(3))
            .SetConcurrencyLimit(2));
    }
}