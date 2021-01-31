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
    public class FryShakeFuture :
        OrderLineFuture<OrderFryShake, FryShakeCompleted, Fault<OrderFryShake>>
    {
        public FryShakeFuture()
        {
            Event(() => FutureRequested, x => x.CorrelateById(context => context.Message.OrderLineId));
            Event(() => RequestFutureRequested, x => x.CorrelateById(context => context.Message.Request.OrderLineId));

            Initially(
                When(FutureRequested)
                    .Activity(x => x.OfType<OrderFryActivity>())
                    .Activity(x => x.OfType<OrderShakeActivity>())
            );
        }

        protected override Task<FryShakeCompleted> CreateCompleted(ConsumeEventContext<FutureState, OrderLineCompleted> context)
        {
            var message = context.Instance.GetRequest<OrderFryShake>();

            return Init<OrderLineCompleted, FryShakeCompleted>(context, new {Description = $"{message.Size} {message.Flavor} FryShake"});
        }

        protected override Task<Fault<OrderFryShake>> CreateFaulted<T>(ConsumeEventContext<FutureState, T> context)
        {
            var message = context.Instance.GetRequest<OrderFryShake>();
            Fault<OrderLine> faulted = context.Instance.Faults.Select(x => x.Value.ToObject<Fault<OrderLine>>()).First();

            return context.Init<Fault<OrderFryShake>>(new
            {
                faulted.FaultId,
                faulted.FaultedMessageId,
                Timestamp = context.Instance.Faulted,
                faulted.Exceptions,
                faulted.Host,
                faulted.FaultMessageTypes,
                Message = message
            });
        }
    }
}