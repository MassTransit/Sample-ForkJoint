namespace ForkJoint.Api.Components.Futures
{
    using Contracts;
    using MassTransit.Futures;


    public class OnionRingsFuture :
        Future<OrderOnionRings, OnionRingsCompleted>
    {
        public OnionRingsFuture()
        {
            Event(() => FutureRequested, x => x.CorrelateById(context => context.Message.OrderLineId));

            SendRequest<CookOnionRings, OnionRingsReady>(x =>
            {
                x.Response(r => r.Init(context => new {Description = $"{context.Message.Quantity} Onion Rings"}));
            });
        }
    }
}
