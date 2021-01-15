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
    public class OrderStateMachine :
        MassTransitStateMachine<OrderState>
    {
        public OrderStateMachine()
        {
            Event(() => OrderSubmitted, x => x.CorrelateById(context => context.Message.OrderId));
            Event(() => LineCompleted, x => x.CorrelateById(context => context.Message.OrderId));
            Event(() => LineFaulted, x => x.CorrelateById(context => context.Message.OrderId));

            InstanceState(x => x.CurrentState, WaitingForCompletion, Completed, Faulted);

            Initially(
                When(OrderSubmitted)
                    .InitializeFuture()
                    .Then(context =>
                    {
                        context.Instance.LineCount = 0;

                        if (context.Data.Burgers != null)
                        {
                            context.Instance.LineCount += context.Data.Burgers.Length;
                            context.Instance.LinesPending.UnionWith(context.Data.Burgers.Select(x => x.BurgerId));
                        }

                        if (context.Data.Fries != null)
                        {
                            context.Instance.LineCount += context.Data.Fries.Length;
                            context.Instance.LinesPending.UnionWith(context.Data.Fries.Select(x => x.FryId));
                        }

                        if (context.Data.Shakes != null)
                        {
                            context.Instance.LineCount += context.Data.Shakes.Length;
                            context.Instance.LinesPending.UnionWith(context.Data.Shakes.Select(x => x.ShakeId));
                        }

                        if (context.Data.FryShakes != null)
                        {
                            context.Instance.LineCount += context.Data.FryShakes.Length;
                            context.Instance.LinesPending.UnionWith(context.Data.FryShakes.Select(x => x.FryShakeId));
                        }
                    })
                    .Activity(x => x.OfType<OrderBurgersActivity>())
                    .Activity(x => x.OfType<OrderFriesActivity>())
                    .Activity(x => x.OfType<OrderShakesActivity>())
                    .Activity(x => x.OfType<OrderFryShakesActivity>())
                    .TransitionTo(WaitingForCompletion)
            );

            During(WaitingForCompletion,
                When(OrderSubmitted)
                    .If(context => context.Instance.RequestId != context.CreateConsumeContext().RequestId, x => x.RequestStarted())
            );

            During(Completed,
                When(OrderSubmitted)
                    .RespondAsync(x => x.CreateOrderCompleted())
            );

            During(Faulted,
                When(OrderSubmitted)
                    .RespondAsync(x => x.CreateOrderFaulted())
            );

            DuringAny(
                When(LineCompleted)
                    .CompleteLine()
                    .CompleteOrderIfReady(this),
                When(LineFaulted)
                    .FaultLine()
                    .CompleteOrderIfReady(this)
            );
        }

        public State WaitingForCompletion { get; }
        public State Completed { get; }
        public State Faulted { get; }

        public Event<SubmitOrder> OrderSubmitted { get; }
        public Event<OrderLineCompleted> LineCompleted { get; }
        public Event<OrderLineFaulted> LineFaulted { get; }
    }


    public static class OrderStateMachineExtensions
    {
        public static EventActivityBinder<OrderState, OrderLineCompleted> CompleteLine(this EventActivityBinder<OrderState, OrderLineCompleted> binder)
        {
            return binder.Then(context =>
            {
                context.Instance.LinesPending.Remove(context.Data.OrderLineId);
                context.Instance.LinesFaulted.Remove(context.Data.OrderLineId);

                context.Instance.LinesCompleted.Add(context.Data.OrderLineId, context.Data);
            });
        }

        public static EventActivityBinder<OrderState, OrderLineFaulted> FaultLine(this EventActivityBinder<OrderState, OrderLineFaulted> binder)
        {
            return binder.Then(context =>
            {
                context.Instance.LinesPending.Remove(context.Data.OrderLineId);

                context.Instance.LinesFaulted.Add(context.Data.OrderLineId, context.Data);
            });
        }

        public static EventActivityBinder<OrderState, T> CompleteOrderIfReady<T>(this EventActivityBinder<OrderState, T> binder, OrderStateMachine machine)
            where T : class
        {
            return binder
                .If(context => context.Instance.LinesPending.Count == 0, ready => ready
                    .IfElse(context => context.Instance.LinesFaulted.Count == 0,
                        completed => completed
                            .SetCompleted(x => x.CreateOrderCompleted())
                            .TransitionTo(machine.Completed),
                        notCompleted => notCompleted
                            .SetFaulted(x => x.CreateOrderFaulted())
                            .TransitionTo(machine.Faulted)
                    )
                );
        }

        public static Task<OrderCompleted> CreateOrderCompleted<T>(this ConsumeEventContext<OrderState, T> context)
            where T : class
        {
            return context.Init<OrderCompleted>(new
            {
                context.Instance.Created,
                context.Instance.Completed,
                OrderId = context.Instance.CorrelationId,
                context.Instance.LinesCompleted
            });
        }

        public static Task<OrderFaulted> CreateOrderFaulted<T>(this ConsumeEventContext<OrderState, T> context)
            where T : class
        {
            return context.Init<OrderFaulted>(new
            {
                context.Instance.Created,
                context.Instance.Faulted,
                OrderId = context.Instance.CorrelationId,
                ExceptionInfo = default(ExceptionInfo),
                context.Instance.LinesCompleted,
                context.Instance.LinesFaulted
            });
        }
    }
}