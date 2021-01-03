namespace ForkJoint.Api.Components.StateMachines
{
    using System.Linq;
    using System.Threading.Tasks;
    using Automatonymous;
    using Contracts;
    using MassTransit;
    using MassTransit.Courier;
    using MassTransit.Courier.Contracts;
    using Microsoft.Extensions.Logging;


    // ReSharper disable UnassignedGetOnlyAutoProperty
    // ReSharper disable MemberCanBePrivate.Global
    public class BurgerStateMachine :
        MassTransitStateMachine<BurgerState>
    {
        public BurgerStateMachine(ILogger<BurgerStateMachine> logger)
        {
            Event(() => BurgerRequested, x => x.CorrelateById(context => context.Message.Burger.BurgerId));
            Event(() => BurgerCompleted, x => x.CorrelateById(instance => instance.TrackingNumber, context => context.Message.TrackingNumber));
            Event(() => BurgerFaulted, x => x.CorrelateById(instance => instance.TrackingNumber, context => context.Message.TrackingNumber));

            InstanceState(x => x.CurrentState, WaitingForCompletion, Completed, Faulted);

            Initially(
                When(BurgerRequested)
                    .Then(context =>
                    {
                        logger.LogInformation("BurgerStateMachine RequestId: {RequestId}", context.CreateConsumeContext().RequestId);

                        context.Instance.OrderId = context.Data.OrderId;
                        context.Instance.Burger = context.Data.Burger;
                    })
                    .Activity(x => x.OfInstanceType<PrepareBurgerActivity>())
                    .RequestStarted()
                    .TransitionTo(WaitingForCompletion));

            During(WaitingForCompletion,
                When(BurgerRequested)
                    .Then(context =>
                    {
                        logger.LogInformation("BurgerStateMachine RequestId: {RequestId} (duplicate request)", context.CreateConsumeContext().RequestId);
                    })
                    .RequestStarted(),
                When(BurgerCompleted)
                    .Then(context => context.Instance.Burger = context.Data.GetVariable<Burger>("Burger"))
                    .RequestCompleted(x => CreateBurgerCompleted(x))
                    .TransitionTo(Completed),
                When(BurgerFaulted)
                    .Then(context =>
                    {
                        context.Instance.Reason = context.Data.ActivityExceptions.Select(x => x.ExceptionInfo).FirstOrDefault()?.Message ?? "Unknown";
                    })
                    .RequestCompleted(x => CreateBurgerNotCompleted(x))
                    .TransitionTo(Faulted));

            During(Completed,
                When(BurgerRequested)
                    .RespondAsync(x => CreateBurgerCompleted(x)));
            During(Faulted,
                When(BurgerRequested)
                    .RespondAsync(x => CreateBurgerNotCompleted(x)));
        }

        public State WaitingForCompletion { get; }
        public State Completed { get; }
        public State Faulted { get; }

        public Event<RequestBurger> BurgerRequested { get; }
        public Event<RoutingSlipCompleted> BurgerCompleted { get; }
        public Event<RoutingSlipFaulted> BurgerFaulted { get; }

        static Task<BurgerCompleted> CreateBurgerCompleted<T>(ConsumeEventContext<BurgerState, T> context)
            where T : class
        {
            return context.Init<BurgerCompleted>(new
            {
                context.Instance.OrderId,
                context.Instance.Burger
            });
        }

        static Task<BurgerNotCompleted> CreateBurgerNotCompleted<T>(ConsumeEventContext<BurgerState, T> context)
            where T : class
        {
            return context.Init<BurgerNotCompleted>(new
            {
                context.Instance.OrderId,
                context.Instance.Burger,
                context.Instance.Reason
            });
        }
    }
}