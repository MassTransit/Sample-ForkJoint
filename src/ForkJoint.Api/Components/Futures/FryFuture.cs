namespace ForkJoint.Api.Components.Futures
{
    using Contracts;
    using ForkJoint.Components;


    public class FryFuture :
        RequestFuture<OrderFry, FryCompleted, CookFry, FryReady>
    {
        public FryFuture()
        {
            Event(() => FutureRequested, x => x.CorrelateById(context => context.Message.OrderLineId));

            Response(x => x.Init(context => new {Description = $"{context.Message.Size} Fries"}));
        }
    }
}