namespace ForkJoint.Api.Components.Futures
{
    using System.Threading.Tasks;
    using Automatonymous;
    using Contracts;
    using ForkJoint.Components;
    using MassTransit;


    public class OnionRingsFuture :
        RequestFuture<OrderOnionRings, OnionRingsCompleted, CookOnionRings, OnionRingsReady>
    {
        public OnionRingsFuture()
        {
            Event(() => FutureRequested, x => x.CorrelateById(context => context.Message.OrderLineId));
        }

        protected override Task<CookOnionRings> CreateCommand(ConsumeEventContext<FutureState, OrderOnionRings> context)
        {
            return context.Init<CookOnionRings>(new
            {
                context.Data.OrderId,
                context.Data.OrderLineId,
                context.Data.Quantity
            });
        }

        protected override Task<OnionRingsCompleted> CreateCompleted(ConsumeEventContext<FutureState, OnionRingsReady> context)
        {
            return Init<OnionRingsReady, OnionRingsCompleted>(context, new {Description = $"{context.Data.Quantity} Onion Rings"});
        }
    }
}