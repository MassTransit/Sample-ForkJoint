namespace ForkJoint.Api.Components.StateMachines
{
    using System;
    using System.Threading.Tasks;
    using Automatonymous;
    using Contracts;
    using GreenPipes;
    using ItineraryPlanners;
    using MassTransit;
    using MassTransit.Courier;
    using MassTransit.Courier.Contracts;


    public class PrepareBurgerActivity :
        Activity<BurgerState>
    {
        readonly IItineraryPlanner<Burger> _planner;

        public PrepareBurgerActivity(IItineraryPlanner<Burger> planner)
        {
            _planner = planner;
        }

        public void Probe(ProbeContext context)
        {
            context.CreateScope("prepareBurger");
        }

        public void Accept(StateMachineVisitor visitor)
        {
            visitor.Visit(this);
        }

        public async Task Execute(BehaviorContext<BurgerState> context, Behavior<BurgerState> next)
        {
            await Execute(context);

            await next.Execute(context);
        }

        public async Task Execute<T>(BehaviorContext<BurgerState, T> context, Behavior<BurgerState, T> next)
        {
            await Execute(context);

            await next.Execute(context);
        }

        public Task Faulted<TException>(BehaviorExceptionContext<BurgerState, TException> context, Behavior<BurgerState> next)
            where TException : Exception
        {
            return next.Faulted(context);
        }

        public Task Faulted<T, TException>(BehaviorExceptionContext<BurgerState, T, TException> context,
            Behavior<BurgerState, T> next)
            where TException : Exception
        {
            return next.Faulted(context);
        }

        async Task Execute(BehaviorContext<BurgerState> context)
        {
            ConsumeEventContext<BurgerState> consumeContext = context.CreateConsumeContext();

            var trackingNumber = NewId.NextGuid();

            var builder = new RoutingSlipBuilder(trackingNumber);

            builder.AddSubscription(consumeContext.ReceiveContext.InputAddress, RoutingSlipEvents.Completed | RoutingSlipEvents.Faulted);

            if (consumeContext.ExpirationTime.HasValue)
                builder.AddVariable("Deadline", consumeContext.ExpirationTime.Value);

            builder.AddVariable("OrderId", context.Instance.OrderId);
            builder.AddVariable("BurgerId", context.Instance.CorrelationId);

            _planner.PlanItinerary(context.Instance.Burger, builder);

            if (context.Instance.Burger.OnionRing)
            {
                await consumeContext.Publish<OrderOnionRings>(new
                {
                    context.Instance.OrderId,
                    OrderLineId = context.Instance.Burger.BurgerId,
                    Quantity = 1
                });
            }

            var routingSlip = builder.Build();

            await consumeContext.Execute(routingSlip).ConfigureAwait(false);

            context.Instance.TrackingNumber = trackingNumber;
        }
    }
}