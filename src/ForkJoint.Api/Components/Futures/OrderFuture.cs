namespace ForkJoint.Api.Components.Futures
{
    using System.Linq;
    using System.Threading.Tasks;
    using Automatonymous;
    using Contracts;
    using ForkJoint.Components;
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
        }

        protected override Task<OrderCompleted> CreateCompleted(ConsumeEventContext<FutureState, OrderLineCompleted> context)
        {
            return Init<OrderLineCompleted, OrderCompleted>(context, new
            {
                LinesCompleted = context.Instance.Results.Select(x => x.Value.ToObject<OrderLineCompleted>()).ToDictionary(x => x.OrderLineId),
            });
        }

        protected override Task<OrderFaulted> CreateFaulted<T>(ConsumeEventContext<FutureState, T> context)
        {
            return Init<T, OrderFaulted>(context,
                new
                {
                    LinesCompleted = context.Instance.Results.Select(x => x.Value.ToObject<OrderLineCompleted>()).ToDictionary(x => x.OrderLineId),
                    LinesFaulted = context.Instance.Faults.Select(x => x.Value.ToObject<Fault<OrderLine>>()).ToDictionary(x => x.Message.OrderLineId),
                    Exceptions = context.Instance.Faults.SelectMany(x => x.Value.ToObject<Fault>().Exceptions).ToArray()
                });
        }
    }
}