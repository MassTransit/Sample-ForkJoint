namespace ForkJoint.Api.Components.Futures
{
    using System.Linq;
    using Contracts;
    using MassTransit;
    using MassTransit.Futures;


    public class ComboFuture :
        Future<OrderCombo, ComboCompleted>
    {
        public ComboFuture()
        {
            Event(() => CommandReceived, x => x.CorrelateById(context => context.Message.OrderLineId));

            var fry = SendRequest<OrderFry>(x =>
                {
                    x.UsingRequestInitializer(context => new
                    {
                        OrderId = context.Instance.CorrelationId,
                        OrderLineId = InVar.Id,
                        Size = Size.Medium
                    });

                    x.TrackPendingRequest(message => message.OrderLineId);
                })
                .OnResponseReceived<FryCompleted>(x => x.CompletePendingRequest(message => message.OrderLineId));

            var shake = SendRequest<OrderShake>(x =>
                {
                    x.UsingRequestInitializer(context => new
                    {
                        OrderId = context.Instance.CorrelationId,
                        OrderLineId = InVar.Id,
                        Size = Size.Medium,
                        Flavor = "Chocolate",
                    });

                    x.TrackPendingRequest(message => message.OrderLineId);
                })
                .OnResponseReceived<ShakeCompleted>(x => x.CompletePendingRequest(message => message.OrderLineId));


            WhenAllCompleted(x =>
            {
                x.SetCompletedUsingInitializer(context =>
                {
                    var fryCompleted = context.SelectResults<FryCompleted>().FirstOrDefault();
                    var shakeCompleted = context.SelectResults<ShakeCompleted>().FirstOrDefault();

                    return new {Description = $"Combo ({fryCompleted.Description}, {shakeCompleted.Description})"};
                });
            });
        }
    }
}
