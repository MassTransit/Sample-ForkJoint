namespace ForkJoint.Api.Components.Futures
{
    using Contracts;
    using ForkJoint.Components;


    public class ShakeFuture :
        RequestFuture<OrderShake, ShakeCompleted, PourShake, ShakeReady>
    {
        public ShakeFuture()
        {
            Event(() => FutureRequested, x => x.CorrelateById(context => context.Message.OrderLineId));

            // this isn't needed but shown for completeness
            Command(x => x.Init(context => new
            {
                context.Message.Flavor,
                context.Message.Size
            }));

            Response(x => x.Init(context => new
            {
                // only need the calculated property
                Description = $"{context.Message.Size} {context.Message.Flavor} Shake"
            }));
        }
    }
}