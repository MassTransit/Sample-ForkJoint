namespace ForkJoint.Application.Components.Futures
{
    using Contracts;
    using GreenPipes;
    using MassTransit;
    using MassTransit.Futures;
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
            //ConcurrentMessageLimit = ConcurrentMessageLimits.GlobalValue;

            ConcurrentMessageLimit = Environment.ProcessorCount * 4;
        }

        protected override void ConfigureSaga(IReceiveEndpointConfigurator endpointConfigurator, ISagaConfigurator<FutureState> sagaConfigurator)
        {
            endpointConfigurator.UseMessageRetry(cfg => cfg.Intervals(500, 15000, 60000));

            endpointConfigurator.UseInMemoryOutbox();
        }
    }
}
