namespace ForkJoint.Application.Components.Futures
{
    using Contracts;
    using GreenPipes;
    using MassTransit;
    using MassTransit.Futures;
using MassTransit.RabbitMqTransport;
    using MassTransit.Registration;
    using System;

    public class ShakeFuture :
        Future<OrderShake, ShakeCompleted>
    {
        public ShakeFuture()
        {
            ConfigureCommand(x => x.CorrelateById(context => context.Message.OrderLineId));

            SendRequest<PourShake>()
                .OnResponseReceived<ShakeReady>(x =>
                    x.SetCompletedUsingInitializer(context => new { Description = $"{context.Message.Size} {context.Message.Flavor} Shake" }));
        }
    }

    public class ShakeFutureDefinition : FutureDefinition<ShakeFuture>
    {
        public ShakeFutureDefinition()
        {
            ConcurrentMessageLimit = GlobalValues.ConcurrentMessageLimit ?? Environment.ProcessorCount * 4;
        }

        protected override void ConfigureSaga(IReceiveEndpointConfigurator endpointConfigurator, ISagaConfigurator<FutureState> sagaConfigurator)
        {
            if (endpointConfigurator is IRabbitMqReceiveEndpointConfigurator && GlobalValues.UseLazyQueues)
            {
                ((IRabbitMqReceiveEndpointConfigurator)endpointConfigurator).Lazy = GlobalValues.UseLazyQueues;
            }

            if (endpointConfigurator is IRabbitMqReceiveEndpointConfigurator && GlobalValues.PrefetchCount != null)
            {
                ((IRabbitMqReceiveEndpointConfigurator)endpointConfigurator).PrefetchCount = (int)GlobalValues.PrefetchCount;
            }

            endpointConfigurator.UseMessageRetry(cfg => cfg.Intervals(500, 15000, 60000));

            endpointConfigurator.UseInMemoryOutbox();
        }
    }
}
