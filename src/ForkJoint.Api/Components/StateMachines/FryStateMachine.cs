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
    public class FryStateMachine :
        MassTransitStateMachine<FryState>
    {
        public FryStateMachine()
        {
            Event(() => FriesOrdered, x => x.CorrelateById(context => context.Message.OrderLineId));
            Event(() => FriesCompleted, x => x.CorrelateById(context => context.Message.OrderLineId));
            Event(() => FriesFaulted, x => x.CorrelateById(context => context.Message.Message.OrderLineId));

            InstanceState(x => x.CurrentState, WaitingForCompletion, Completed, Faulted);

            Initially(
                When(FriesOrdered)
                    .InitializeFuture()
                    .Then(context =>
                    {
                        context.Instance.OrderId = context.Data.OrderId;
                        context.Instance.Size = context.Data.Size;
                    })
                    .Activity(x => x.OfInstanceType<CookFryActivity>())
                    .TransitionTo(WaitingForCompletion)
            );

            During(WaitingForCompletion,
                When(FriesOrdered)
                    .PendingRequestStarted()
            );

            During(Completed,
                When(FriesOrdered)
                    .RespondAsync(x => x.CreateFriesCompleted())
            );

            During(Faulted,
                When(FriesOrdered)
                    .RespondAsync(x => x.CreateFriesFaulted())
            );

            DuringAny(
                When(FriesCompleted)
                    .SetCompleted(x => x.CreateFriesCompleted())
                    .TransitionTo(Completed),
                When(FriesFaulted)
                    .FaultFries()
                    .SetFaulted(x => x.CreateFriesFaulted())
                    .TransitionTo(Faulted)
            );
        }

        public State WaitingForCompletion { get; }
        public State Completed { get; }
        public State Faulted { get; }

        public Event<OrderFry> FriesOrdered { get; }
        public Event<FryReady> FriesCompleted { get; }
        public Event<Fault<CookFry>> FriesFaulted { get; }
    }


    public static class FriesStateMachineExtensions
    {
        public static EventActivityBinder<FryState, Fault<CookFry>> FaultFries(this EventActivityBinder<FryState,
            Fault<CookFry>> binder)
        {
            return binder.Then(context => context.Instance.ExceptionInfo = context.Data.Exceptions.FirstOrDefault());
        }

        public static Task<FryCompleted> CreateFriesCompleted<T>(this ConsumeEventContext<FryState, T> context)
            where T : class
        {
            return context.Init<FryCompleted>(new
            {
                context.Instance.Created,
                context.Instance.Completed,
                context.Instance.OrderId,
                OrderLineId = context.Instance.CorrelationId,
                context.Instance.Size,
                Description = $"{context.Instance.Size} Fries"
            });
        }

        public static Task<FryFaulted> CreateFriesFaulted<T>(this ConsumeEventContext<FryState, T> context)
            where T : class
        {
            return context.Init<FryFaulted>(new
            {
                context.Instance.Created,
                context.Instance.Faulted,
                context.Instance.OrderId,
                OrderLineId = context.Instance.CorrelationId,
                Description = $"{context.Instance.Size} Fries",
                context.Instance.ExceptionInfo
            });
        }
    }
}