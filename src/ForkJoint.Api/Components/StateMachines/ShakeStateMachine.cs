namespace ForkJoint.Api.Components.StateMachines
{
    using System.Linq;
    using System.Threading.Tasks;
    using Automatonymous;
    using Automatonymous.Binders;
    using Contracts;
    using MassTransit;


    // ReSharper disable UnassignedGetOnlyAutoProperty
    // ReSharper disable MemberCanBePrivate.Global
    public class ShakeStateMachine :
        MassTransitStateMachine<ShakeState>
    {
        public ShakeStateMachine()
        {
            Event(() => ShakeOrdered, x => x.CorrelateById(context => context.Message.OrderLineId));
            Event(() => ShakeCompleted, x => x.CorrelateById(context => context.Message.OrderLineId));
            Event(() => ShakeFaulted, x => x.CorrelateById(context => context.Message.Message.OrderLineId));

            InstanceState(x => x.CurrentState, WaitingForCompletion, Completed, Faulted);

            Initially(
                When(ShakeOrdered)
                    .InitializeFuture()
                    .Then(context =>
                    {
                        context.Instance.OrderId = context.Data.OrderId;
                        context.Instance.Flavor = context.Data.Flavor;
                        context.Instance.Size = context.Data.Size;
                    })
                    .Activity(x => x.OfInstanceType<PourShakeActivity>())
                    .TransitionTo(WaitingForCompletion)
            );

            During(WaitingForCompletion,
                When(ShakeOrdered)
                    .PendingRequestStarted()
            );

            During(Completed,
                When(ShakeOrdered)
                    .RespondAsync(x => x.CreateShakeCompleted())
            );

            During(Faulted,
                When(ShakeOrdered)
                    .RespondAsync(x => x.CreateShakeFaulted())
            );

            DuringAny(
                When(ShakeCompleted)
                    .SetCompleted(x => x.CreateShakeCompleted())
                    .TransitionTo(Completed),
                When(ShakeFaulted)
                    .FaultShake()
                    .SetFaulted(x => x.CreateShakeFaulted())
                    .TransitionTo(Faulted)
            );
        }

        public State WaitingForCompletion { get; }
        public State Completed { get; }
        public State Faulted { get; }

        public Event<OrderShake> ShakeOrdered { get; }
        public Event<ShakeReady> ShakeCompleted { get; }
        public Event<Fault<PourShake>> ShakeFaulted { get; }
    }


    public static class ShakeStateMachineExtensions
    {
        public static EventActivityBinder<ShakeState, Fault<PourShake>> FaultShake(this EventActivityBinder<ShakeState,
            Fault<PourShake>> binder)
        {
            return binder.Then(context => context.Instance.ExceptionInfo = context.Data.Exceptions.FirstOrDefault());
        }

        public static Task<ShakeCompleted> CreateShakeCompleted<T>(this ConsumeEventContext<ShakeState, T> context)
            where T : class
        {
            return context.Init<ShakeCompleted>(new
            {
                context.Instance.Created,
                context.Instance.Completed,
                context.Instance.OrderId,
                OrderLineId = context.Instance.CorrelationId,
                context.Instance.Size,
                Description = $"{context.Instance.Size} Shake"
            });
        }

        public static Task<ShakeFaulted> CreateShakeFaulted<T>(this ConsumeEventContext<ShakeState, T> context)
            where T : class
        {
            return context.Init<ShakeFaulted>(new
            {
                context.Instance.Created,
                context.Instance.Faulted,
                context.Instance.OrderId,
                OrderLineId = context.Instance.CorrelationId,
                Description = $"{context.Instance.Size} Shake",
                context.Instance.ExceptionInfo
            });
        }
    }
}