namespace ForkJoint.Api.Components.Futures
{
    using Contracts;
    using MassTransit.Futures;


    public class FryFuture :
        Future<OrderFry, FryCompleted>
    {
        public FryFuture()
        {
            Event(() => FutureRequested, x => x.CorrelateById(context => context.Message.OrderLineId));

            SendRequest<CookFry, FryReady>(x =>
            {
                x.Response(r => r.Init(context => new {Description = $"{context.Message.Size} Fries"}));
            });
        }
    }
}
