namespace ForkJoint.Api.Components.Futures
{
    using Contracts;
    using ForkJoint.Components;


    public class OnionRingsFuture :
        RequestFuture<OrderOnionRings, OnionRingsCompleted, CookOnionRings, OnionRingsReady>
    {
        public OnionRingsFuture()
        {
            Event(() => FutureRequested, x => x.CorrelateById(context => context.Message.OrderLineId));
            Event(() => RequestFutureRequested, x => x.CorrelateById(context => context.Message.Request.OrderLineId));

            Response(x => x.Init(context => new {Description = $"{context.Message.Quantity} Onion Rings"}));
        }
    }
}