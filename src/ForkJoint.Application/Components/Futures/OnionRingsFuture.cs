namespace ForkJoint.Application.Components.Futures
{
    using Contracts;
    using GreenPipes;
    using MassTransit;
    using MassTransit.Futures;
    using MassTransit.Registration;

    public class OnionRingsFuture :
        Future<OrderOnionRings, OnionRingsCompleted>
    {
        public OnionRingsFuture()
        {
            ConfigureCommand(x => x.CorrelateById(context => context.Message.OrderLineId));

            SendRequest<CookOnionRings>()
                .OnResponseReceived<OnionRingsReady>(x =>
                {
                    x.SetCompletedUsingInitializer(context => new {Description = $"{context.Message.Quantity} Onion Rings"});
                });
        }
    }

    public class OnionRingsFutureDefinition : FutureDefinition<OnionRingsFuture>
    {
        public OnionRingsFutureDefinition()
        {
            ConcurrentMessageLimit = ConcurrentMessageLimits.GlobalValue;
        }

        protected override void ConfigureSaga(IReceiveEndpointConfigurator endpointConfigurator, ISagaConfigurator<FutureState> sagaConfigurator)
        {
            endpointConfigurator.UseMessageRetry(cfg => cfg.Immediate(5));
            endpointConfigurator.UseInMemoryOutbox();
        }
    }
}
