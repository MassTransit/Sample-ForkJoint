namespace ForkJoint.Api.Components.Futures
{
    using Contracts;
    using MassTransit.Futures;


    public class ShakeFuture :
        Future<OrderShake, ShakeCompleted>
    {
        public ShakeFuture()
        {
            Event(() => FutureRequested, x => x.CorrelateById(context => context.Message.OrderLineId));

            SendRequest<PourShake, ShakeReady>(x =>
            {
                x.Response(r => r.Init(context => new {Description = $"{context.Message.Size} {context.Message.Flavor} Shake"}));
            });
        }
    }
}
