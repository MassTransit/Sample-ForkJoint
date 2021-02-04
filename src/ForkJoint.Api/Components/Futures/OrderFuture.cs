namespace ForkJoint.Api.Components.Futures
{
    using System.Linq;
    using Automatonymous;
    using Contracts;
    using FutureActivities;
    using MassTransit;


    // ReSharper disable UnassignedGetOnlyAutoProperty
    // ReSharper disable MemberCanBePrivate.Global
    public class OrderFuture :
        OrderLineFuture<SubmitOrder, OrderCompleted, OrderFaulted>
    {
        public OrderFuture()
        {
            Event(() => FutureRequested, x => x.CorrelateById(context => context.Message.OrderId));

            Initially(
                When(FutureRequested)
                    .Activity(x => x.OfType<OrderBurgersActivity>())
                    .Activity(x => x.OfType<OrderFriesActivity>())
                    .Activity(x => x.OfType<OrderShakesActivity>())
                    .Activity(x => x.OfType<OrderFryShakesActivity>())
            );

            Response(r => r.Init(context => new
            {
                LinesCompleted = context.Instance.Results.Select(x => x.Value.ToObject<OrderLineCompleted>()).ToDictionary(x => x.OrderLineId),
            }));

            Fault(f => f.Init(context => new
            {
                LinesCompleted = context.Instance.Results.Select(x => x.Value.ToObject<OrderLineCompleted>()).ToDictionary(x => x.OrderLineId),
                LinesFaulted = context.Instance.Faults.Select(x => x.Value.ToObject<Fault<OrderLine>>()).ToDictionary(x => x.Message.OrderLineId),
                Exceptions = context.Instance.Faults.SelectMany(x => x.Value.ToObject<Fault>().Exceptions).ToArray()
            }));
        }
    }
}