namespace ForkJoint.Api.Components.Futures
{
    using System.Threading.Tasks;
    using Automatonymous;
    using Contracts;
    using ForkJoint.Components;
    using MassTransit;


    public class FryFuture :
        RequestFuture<OrderFry, FryCompleted, CookFry, FryReady>
    {
        public FryFuture()
        {
            Event(() => FutureRequested, x => x.CorrelateById(context => context.Message.OrderLineId));
        }

        protected override Task<CookFry> CreateCommand(ConsumeEventContext<FutureState, OrderFry> context)
        {
            return context.Init<CookFry>(new
            {
                context.Data.OrderId,
                context.Data.OrderLineId,
                context.Data.Size
            });
        }

        protected override Task<FryCompleted> CreateCompleted(ConsumeEventContext<FutureState, FryReady> context)
        {
            return Init<FryReady, FryCompleted>(context, new {Description = $"{context.Data.Size} Fries"});
        }
    }
}