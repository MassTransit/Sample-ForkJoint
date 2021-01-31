namespace ForkJoint.Api.Components.Futures
{
    using System.Threading.Tasks;
    using Automatonymous;
    using Contracts;
    using ForkJoint.Components;
    using ForkJoint.Components.Internals;
    using MassTransit;


    // ReSharper disable UnassignedGetOnlyAutoProperty
    // ReSharper disable MemberCanBePrivate.Global
    public abstract class OrderLineFuture<TRequest, TCompleted, TFaulted> :
        Future<TRequest, TCompleted, TFaulted>
        where TRequest : class
        where TCompleted : class
        where TFaulted : class
    {
        protected OrderLineFuture()
        {
            Event(() => LineCompleted, x =>
            {
                x.CorrelateById(context => context.Message.OrderId);
                x.OnMissingInstance(m => m.Fault());
            });
            Event(() => LineFaulted, x =>
            {
                x.CorrelateById(context => context.Message.Message.OrderId);
                x.OnMissingInstance(m => m.Fault());
            });

            DuringAny(
                When(LineCompleted)
                    .SetResult(x => x.Data.OrderLineId, x => x.Data)
                    .IfElse(context => context.Instance.Completed.HasValue,
                        completed => completed
                            .SetFutureCompleted(x => CreateCompleted(x))
                            .RespondToSubscribers(x => GetCompleted(x))
                            .TransitionTo(Completed),
                        notCompleted => notCompleted.If(context => context.Instance.Faulted.HasValue,
                            faulted => faulted
                                .SetFutureFaulted(x => CreateFaulted(x))
                                .RespondToSubscribers(x => GetFaulted(x))
                                .TransitionTo(Faulted))),
                When(LineFaulted)
                    .SetFault(context => context.Data.Message.OrderLineId, x => Task.FromResult(x.Data))
                    .If(context => context.Instance.Faulted.HasValue,
                        faulted => faulted
                            .SetFutureFaulted(x => CreateFaulted(x))
                            .RespondToSubscribers(x => GetFaulted(x))
                            .TransitionTo(Faulted))
            );
        }

        public Event<OrderLineCompleted> LineCompleted { get; protected set; }
        public Event<Fault<OrderLine>> LineFaulted { get; protected set; }

        protected abstract Task<TCompleted> CreateCompleted(ConsumeEventContext<FutureState, OrderLineCompleted> context);

        protected abstract Task<TFaulted> CreateFaulted<T>(ConsumeEventContext<FutureState, T> context)
            where T : class;
    }
}