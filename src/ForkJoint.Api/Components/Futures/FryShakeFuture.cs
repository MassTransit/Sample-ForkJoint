namespace ForkJoint.Api.Components.Futures
{
    using Automatonymous;
    using Contracts;
    using ForkJoint.Components;
    using FutureActivities;
    using MassTransit;


    // ReSharper disable UnassignedGetOnlyAutoProperty
    // ReSharper disable MemberCanBePrivate.Global
    public class FryShakeFuture :
        OrderLineFuture<OrderFryShake, FryShakeCompleted, Fault<OrderFryShake>>
    {
        public FryShakeFuture()
        {
            Event(() => FutureRequested, x => x.CorrelateById(context => context.Message.OrderLineId));

            Initially(
                When(FutureRequested)
                    .Activity(x => x.OfType<OrderFryActivity>())
                    .Activity(x => x.OfType<OrderShakeActivity>())
            );

            Response(x => x.Init(context =>
            {
                var message = context.Instance.GetRequest<OrderFryShake>();

                return new {Description = $"{message.Size} {message.Flavor} FryShake"};
            }));
        }
    }
}