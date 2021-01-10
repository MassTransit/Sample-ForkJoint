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
    public class OnionRingsStateMachine :
        MassTransitStateMachine<OnionRingsState>
    {
        public OnionRingsStateMachine()
        {
            Event(() => OnionRingsRequested, x => x.CorrelateById(context => context.Message.OrderLineId));
            Event(() => OnionRingsCompleted, x => x.CorrelateById(context => context.Message.OrderLineId));
            Event(() => OnionRingsFaulted, x => x.CorrelateById(context => context.Message.Message.OrderLineId));

            InstanceState(x => x.CurrentState, WaitingForCompletion, Completed, Faulted);

            Initially(
                When(OnionRingsRequested)
                    .InitializeFuture()
                    .Then(context =>
                    {
                        context.Instance.OrderId = context.Data.OrderId;
                        context.Instance.Quantity = context.Data.Quantity;
                    })
                    .Activity(x => x.OfInstanceType<FryOnionRingsActivity>())
                    .TransitionTo(WaitingForCompletion)
            );

            During(WaitingForCompletion,
                When(OnionRingsRequested)
                    .PendingRequestStarted()
            );

            During(Completed,
                When(OnionRingsRequested)
                    .RespondAsync(x => x.CreateOnionRingsCompleted())
            );

            During(Faulted,
                When(OnionRingsRequested)
                    .RespondAsync(x => x.CreateOnionRingsFaulted())
            );

            DuringAny(
                When(OnionRingsCompleted)
                    .SetCompleted(x => x.CreateOnionRingsCompleted())
                    .TransitionTo(Completed),
                When(OnionRingsFaulted)
                    .FaultOnionRings()
                    .SetFaulted(x => x.CreateOnionRingsFaulted())
                    .TransitionTo(Faulted)
            );
        }

        public State WaitingForCompletion { get; }
        public State Completed { get; }
        public State Faulted { get; }

        public Event<OrderOnionRings> OnionRingsRequested { get; }
        public Event<OnionRingsFried> OnionRingsCompleted { get; }
        public Event<Fault<FryOnionRings>> OnionRingsFaulted { get; }
    }


    public static class OnionRingsStateMachineExtensions
    {
        public static EventActivityBinder<OnionRingsState, Fault<FryOnionRings>> FaultOnionRings(this EventActivityBinder<OnionRingsState,
            Fault<FryOnionRings>> binder)
        {
            return binder.Then(context => context.Instance.ExceptionInfo = context.Data.Exceptions.FirstOrDefault());
        }

        public static Task<OnionRingsCompleted> CreateOnionRingsCompleted<T>(this ConsumeEventContext<OnionRingsState, T> context)
            where T : class
        {
            return context.Init<OnionRingsCompleted>(new
            {
                context.Instance.Created,
                context.Instance.Completed,
                context.Instance.OrderId,
                OrderLineId = context.Instance.CorrelationId,
                context.Instance.Quantity,
                Description = $"{context.Instance.Quantity} Onion Rings"
            });
        }

        public static Task<OnionRingsFaulted> CreateOnionRingsFaulted<T>(this ConsumeEventContext<OnionRingsState, T> context)
            where T : class
        {
            return context.Init<OnionRingsFaulted>(new
            {
                context.Instance.Created,
                context.Instance.Faulted,
                context.Instance.OrderId,
                OrderLineId = context.Instance.CorrelationId,
                Description = $"{context.Instance.Quantity} Onion Rings",
                context.Instance.ExceptionInfo
            });
        }
    }
}