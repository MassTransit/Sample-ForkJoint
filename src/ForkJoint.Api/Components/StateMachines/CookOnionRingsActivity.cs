namespace ForkJoint.Api.Components.StateMachines
{
    using System;
    using System.Threading.Tasks;
    using Automatonymous;
    using Contracts;
    using GreenPipes;
    using MassTransit;


    public class CookOnionRingsActivity :
        Activity<OnionRingsState>
    {
        public void Probe(ProbeContext context)
        {
            context.CreateScope(nameof(CookOnionRingsActivity));
        }

        public void Accept(StateMachineVisitor visitor)
        {
            visitor.Visit(this);
        }

        public async Task Execute(BehaviorContext<OnionRingsState> context, Behavior<OnionRingsState> next)
        {
            await Execute(context);

            await next.Execute(context);
        }

        public async Task Execute<T>(BehaviorContext<OnionRingsState, T> context, Behavior<OnionRingsState, T> next)
        {
            await Execute(context);

            await next.Execute(context);
        }

        public Task Faulted<TException>(BehaviorExceptionContext<OnionRingsState, TException> context, Behavior<OnionRingsState> next)
            where TException : Exception
        {
            return next.Faulted(context);
        }

        public Task Faulted<T, TException>(BehaviorExceptionContext<OnionRingsState, T, TException> context,
            Behavior<OnionRingsState, T> next)
            where TException : Exception
        {
            return next.Faulted(context);
        }

        static async Task Execute(BehaviorContext<OnionRingsState> context)
        {
            ConsumeEventContext<OnionRingsState> consumeContext = context.CreateConsumeContext();

            await consumeContext.Publish<CookOnionRings>(new
            {
                context.Instance.OrderId,
                OrderLineId = context.Instance.CorrelationId,
                context.Instance.Quantity
            }, x =>
            {
                x.RequestId = NewId.NextGuid();
                x.ResponseAddress = consumeContext.ReceiveContext.InputAddress;
            });
        }
    }
}