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
            if (endpointConfigurator is IRabbitMqReceiveEndpointConfigurator)
            {
                var rabbitMqReceiveEndpointConfigurator = (IRabbitMqReceiveEndpointConfigurator)endpointConfigurator;

                if (GlobalValues.PrefetchCount != null)
                    rabbitMqReceiveEndpointConfigurator.PrefetchCount = (int)GlobalValues.PrefetchCount;

                if (GlobalValues.UseQuorumQueues)
                    rabbitMqReceiveEndpointConfigurator.SetQuorumQueue();

                if (GlobalValues.UseLazyQueues && !GlobalValues.UseQuorumQueues)
                    rabbitMqReceiveEndpointConfigurator.Lazy = GlobalValues.UseLazyQueues;
            }

            endpointConfigurator.UseMessageRetry(GlobalValues.RetryPolicy);

            endpointConfigurator.UseInMemoryOutbox();
        }
    }
}
