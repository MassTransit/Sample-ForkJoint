namespace ForkJoint.Api.Components.Futures
{
    using Contracts;
    using MassTransit;
    using MassTransit.Futures;


    public class ComboFuture :
        Future<OrderCombo, ComboCompleted>
    {
        public ComboFuture()
        {
            Event(() => FutureRequested, x => x.CorrelateById(context => context.Message.OrderLineId));

            SendRequest<OrderFry, FryCompleted>(x =>
            {
                x.Pending(m => m.OrderLineId, m => m.OrderLineId);

                x.Command(c =>
                {
                    c.Init(context => new
                    {
                        OrderId = context.Instance.CorrelationId,
                        OrderLineId = InVar.Id,
                        Size = Size.Medium,
                    });
                });
            });

            // change to SendRequest()/GetResponse() to resolve results separately!
            SendRequest<OrderShake, ShakeCompleted>(x =>
            {
                x.Pending(m => m.OrderLineId, m => m.OrderLineId);

                x.Command(c =>
                {
                    c.Init(context => new
                    {
                        OrderId = context.Instance.CorrelationId,
                        OrderLineId = InVar.Id,
                        Size = Size.Medium,
                        Flavor = "Chocolate",
                    });
                });
            });


            Response(x => x.Init(context => new {Description = "Simple Combo"}));
        }
    }
}
