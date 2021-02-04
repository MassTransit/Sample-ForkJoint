namespace ForkJoint.Components.Activities
{
    using System;
    using System.Threading.Tasks;
    using Automatonymous;
    using GreenPipes;
    using Internals;
    using MassTransit;
    using MassTransit.Courier;
    using MassTransit.Courier.Contracts;


    public class ExecuteRoutingSlipFutureActivity<T> :
        Activity<FutureState, T>
        where T : class
    {
        readonly IItineraryPlanner<T> _planner;

        public ExecuteRoutingSlipFutureActivity(IItineraryPlanner<T> planner)
        {
            _planner = planner;
        }

        public void Probe(ProbeContext context)
        {
            context.CreateScope(nameof(ExecuteRoutingSlipFutureActivity<T>));
        }

        public void Accept(StateMachineVisitor visitor)
        {
            visitor.Visit(this);
        }

        public Task Execute(BehaviorContext<FutureState, T> context, Behavior<FutureState, T> next)
        {
            return Execute(context.CreateFutureConsumeContext());
        }

        public Task Faulted<TException>(BehaviorExceptionContext<FutureState, T, TException> context, Behavior<FutureState, T> next)
            where TException : Exception
        {
            return next.Faulted(context);
        }

        async Task Execute(FutureConsumeContext<T> consumeContext)
        {
            // this will need to be done by a consumer at some point, to handle retry/fault handling

            var trackingNumber = NewId.NextGuid();

            var builder = new RoutingSlipBuilder(trackingNumber);

            builder.AddVariable(nameof(consumeContext.FutureId), consumeContext.FutureId);

            builder.AddSubscription(consumeContext.ReceiveContext.InputAddress, RoutingSlipEvents.Completed | RoutingSlipEvents.Faulted);

            await _planner.PlanItinerary(consumeContext.Message, builder).ConfigureAwait(false);

            var routingSlip = builder.Build();

            await consumeContext.Execute(routingSlip).ConfigureAwait(false);
        }
    }
}