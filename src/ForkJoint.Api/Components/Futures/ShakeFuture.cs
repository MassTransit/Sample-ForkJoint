namespace ForkJoint.Api.Components.Futures
{
    using System.Threading.Tasks;
    using Automatonymous;
    using Contracts;
    using ForkJoint.Components;
    using MassTransit;


    public class ShakeFuture :
        RequestFuture<OrderShake, ShakeCompleted, PourShake, ShakeReady>
    {
        public ShakeFuture()
        {
            Event(() => FutureRequested, x => x.CorrelateById(context => context.Message.OrderLineId));
        }

        protected override Task<PourShake> CreateCommand(ConsumeEventContext<FutureState, OrderShake> context)
        {
            return context.Init<PourShake>(new
            {
                context.Data.OrderId,
                context.Data.OrderLineId,
                context.Data.Flavor,
                context.Data.Size
            });
        }

        protected override Task<ShakeCompleted> CreateCompleted(ConsumeEventContext<FutureState, ShakeReady> context)
        {
            return Init<ShakeReady, ShakeCompleted>(context, new {Description = $"{context.Data.Size} {context.Data.Flavor} Shake"});
        }
    }
}