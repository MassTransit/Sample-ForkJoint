namespace ForkJoint.Application.Components.Futures
{
    using Contracts;
    using MassTransit.Futures;
    using MassTransit.Registration;

    public class ShakeFuture :
        Future<OrderShake, ShakeCompleted>
    {
        public ShakeFuture()
        {
            ConfigureCommand(x => x.CorrelateById(context => context.Message.OrderLineId));

            SendRequest<PourShake>()
                .OnResponseReceived<ShakeReady>(x =>
                    x.SetCompletedUsingInitializer(context => new {Description = $"{context.Message.Size} {context.Message.Flavor} Shake"}));
        }
    }

    public class ShakeFutureDefinition : FutureDefinition<ShakeFuture>
    {
        public ShakeFutureDefinition()
        {
            ConcurrentMessageLimit = 32;
        }
    }
}
