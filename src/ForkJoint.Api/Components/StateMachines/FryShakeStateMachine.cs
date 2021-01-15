namespace ForkJoint.Api.Components.StateMachines
{
    using System.Threading.Tasks;
    using Automatonymous;
    using Automatonymous.Binders;
    using Contracts;
    using MassTransit;


    // ReSharper disable UnassignedGetOnlyAutoProperty
    // ReSharper disable MemberCanBePrivate.Global
    public class FryShakeStateMachine :
        MassTransitStateMachine<FryShakeState>
    {
        public FryShakeStateMachine()
        {
            Event(() => FryShakeOrdered, x => x.CorrelateById(context => context.Message.OrderLineId));
            Event(() => FryCompleted, x => x.CorrelateById(context => context.Message.OrderLineId));
            Event(() => FryFaulted, x => x.CorrelateById(context => context.Message.OrderLineId));
            Event(() => ShakeCompleted, x => x.CorrelateById(context => context.Message.OrderLineId));
            Event(() => ShakeFaulted, x => x.CorrelateById(context => context.Message.OrderLineId));

            InstanceState(x => x.CurrentState, WaitingForCompletion, Completed, Faulted);

            Initially(
                When(FryShakeOrdered)
                    .InitializeFuture()
                    .Then(context =>
                    {
                        context.Instance.OrderId = context.Data.OrderId;
                        context.Instance.Size = context.Data.Size;
                    })
                    .Activity(x => x.OfType<OrderFryShakeActivity>())
                    .TransitionTo(WaitingForCompletion)
            );

            During(WaitingForCompletion,
                When(FryShakeOrdered)
                    .PendingRequestStarted()
            );

            During(Completed,
                When(FryShakeOrdered)
                    .RespondAsync(x => x.CreateFryShakeCompleted())
            );

            During(Faulted,
                When(FryShakeOrdered)
                    .RespondAsync(x => x.CreateFryShakeFaulted())
            );

            DuringAny(
                When(FryFaulted)
                    .FaultFry()
                    .SetFaulted(x => x.CreateFryShakeFaulted())
                    .TransitionTo(Faulted)
            );

            DuringAny(
                When(ShakeFaulted)
                    .FaultShake()
                    .SetFaulted(x => x.CreateFryShakeFaulted())
                    .TransitionTo(Faulted)
            );

            DuringAny(
                When(BothCompleted)
                    .SetCompleted(x => x.CreateFryShakeCompleted())
            );

            CompositeEvent(() => BothCompleted, x => x.BothState, FryCompleted, ShakeCompleted);
        }

        public State WaitingForCompletion { get; }
        public State Completed { get; }
        public State Faulted { get; }

        public Event BothCompleted { get; }

        public Event<OrderFryShake> FryShakeOrdered { get; }
        public Event<FryCompleted> FryCompleted { get; }
        public Event<FryFaulted> FryFaulted { get; }
        public Event<ShakeCompleted> ShakeCompleted { get; }
        public Event<ShakeFaulted> ShakeFaulted { get; }
    }


    public static class FryShakeStateMachineExtensions
    {
        public static EventActivityBinder<FryShakeState, FryFaulted> FaultFry(this EventActivityBinder<FryShakeState, FryFaulted> binder)
        {
            return binder.Then(context => context.Instance.ExceptionInfo = context.Data.ExceptionInfo);
        }

        public static EventActivityBinder<FryShakeState, ShakeFaulted> FaultShake(this EventActivityBinder<FryShakeState, ShakeFaulted> binder)
        {
            return binder.Then(context => context.Instance.ExceptionInfo = context.Data.ExceptionInfo);
        }

        public static Task<FryShakeCompleted> CreateFryShakeCompleted<T>(this ConsumeEventContext<FryShakeState, T> context)
            where T : class
        {
            return context.Init<FryShakeCompleted>(new
            {
                context.Instance.Created,
                context.Instance.Completed,
                context.Instance.OrderId,
                OrderLineId = context.Instance.CorrelationId,
                context.Instance.Size,
                context.Instance.Flavor,
                Description = $"{context.Instance.Size} {context.Instance.Flavor} Fry Shake"
            });
        }

        public static Task<FryShakeCompleted> CreateFryShakeCompleted(this ConsumeEventContext<FryShakeState> context)
        {
            return context.Init<FryShakeCompleted>(new
            {
                context.Instance.Created,
                context.Instance.Completed,
                context.Instance.OrderId,
                OrderLineId = context.Instance.CorrelationId,
                context.Instance.Size,
                context.Instance.Flavor,
                Description = $"{context.Instance.Size} {context.Instance.Flavor} Fry Shake"
            });
        }

        public static Task<FryShakeFaulted> CreateFryShakeFaulted<T>(this ConsumeEventContext<FryShakeState, T> context)
            where T : class
        {
            return context.Init<FryShakeFaulted>(new
            {
                context.Instance.Created,
                context.Instance.Faulted,
                context.Instance.OrderId,
                OrderLineId = context.Instance.CorrelationId,
                Description = $"{context.Instance.Size} {context.Instance.Flavor} Fry Shake",
                context.Instance.ExceptionInfo
            });
        }
    }
}