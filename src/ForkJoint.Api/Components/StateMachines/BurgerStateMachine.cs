namespace ForkJoint.Api.Components.StateMachines
{
    using System.Linq;
    using System.Threading.Tasks;
    using Automatonymous;
    using Automatonymous.Binders;
    using Contracts;
    using MassTransit;
    using MassTransit.Courier;
    using MassTransit.Courier.Contracts;


    // ReSharper disable UnassignedGetOnlyAutoProperty
    // ReSharper disable MemberCanBePrivate.Global
    public class BurgerStateMachine :
        MassTransitStateMachine<BurgerState>
    {
        public BurgerStateMachine()
        {
            Event(() => BurgerRequested, x => x.CorrelateById(context => context.Message.Burger.BurgerId));
            Event(() => BurgerCompleted, x => x.CorrelateById(instance => instance.TrackingNumber, context => context.Message.TrackingNumber));
            Event(() => BurgerFaulted, x => x.CorrelateById(instance => instance.TrackingNumber, context => context.Message.TrackingNumber));

            InstanceState(x => x.CurrentState, WaitingForCompletion, Completed, Faulted);

            Initially(
                When(BurgerRequested)
                    .InitializeFuture()
                    .Then(context =>
                    {
                        context.Instance.OrderId = context.Data.OrderId;
                        context.Instance.Burger = context.Data.Burger;
                    })
                    .Activity(x => x.OfInstanceType<PrepareBurgerActivity>())
                    .TransitionTo(WaitingForCompletion)
            );

            During(WaitingForCompletion,
                When(BurgerRequested)
                    .PendingRequestStarted()
            );

            During(Completed,
                When(BurgerRequested)
                    .RespondAsync(x => x.CreateBurgerCompleted())
            );

            During(Faulted,
                When(BurgerRequested)
                    .RespondAsync(x => x.CreateBurgerFaulted())
            );

            DuringAny(
                When(BurgerCompleted)
                    .CompleteBurger()
                    .SetCompleted(x => x.CreateBurgerCompleted())
                    .TransitionTo(Completed),
                When(BurgerFaulted)
                    .FaultBurger()
                    .SetFaulted(x => x.CreateBurgerFaulted())
                    .TransitionTo(Faulted)
            );
        }

        public State WaitingForCompletion { get; }
        public State Completed { get; }
        public State Faulted { get; }

        public Event<OrderBurger> BurgerRequested { get; }
        public Event<RoutingSlipCompleted> BurgerCompleted { get; }
        public Event<RoutingSlipFaulted> BurgerFaulted { get; }
    }


    public static class BurgerStateMachineExtensions
    {
        public static EventActivityBinder<BurgerState, RoutingSlipCompleted> CompleteBurger(this EventActivityBinder<BurgerState, RoutingSlipCompleted> binder)
        {
            return binder.Then(context => context.Instance.Burger = context.Data.GetVariable<Burger>("Burger"));
        }

        public static EventActivityBinder<BurgerState, RoutingSlipFaulted> FaultBurger(this EventActivityBinder<BurgerState, RoutingSlipFaulted> binder)
        {
            return binder.Then(context => context.Instance.ExceptionInfo = context.Data.ActivityExceptions.Select(x => x.ExceptionInfo).FirstOrDefault());
        }

        public static Task<BurgerCompleted> CreateBurgerCompleted<T>(this ConsumeEventContext<BurgerState, T> context)
            where T : class
        {
            return context.Init<BurgerCompleted>(new
            {
                context.Instance.Created,
                context.Instance.Completed,
                context.Instance.OrderId,
                OrderLineId = context.Instance.CorrelationId,
                Description = context.Instance.Burger.ToString(),
                context.Instance.Burger
            });
        }

        public static Task<BurgerFaulted> CreateBurgerFaulted<T>(this ConsumeEventContext<BurgerState, T> context)
            where T : class
        {
            return context.Init<BurgerFaulted>(new
            {
                context.Instance.Created,
                context.Instance.Faulted,
                context.Instance.OrderId,
                OrderLineId = context.Instance.CorrelationId,
                Description = context.Instance.Burger.ToString(),
                context.Instance.ExceptionInfo
            });
        }
    }
}