namespace ForkJoint.Api.Components.StateMachines
{
    using System;
    using System.Threading.Tasks;
    using Automatonymous;
    using Contracts;
    using GreenPipes;
    using MassTransit;


    public class PourShakeActivity :
        Activity<ShakeState>
    {
        public void Probe(ProbeContext context)
        {
            context.CreateScope(nameof(PourShakeActivity));
        }

        public void Accept(StateMachineVisitor visitor)
        {
            visitor.Visit(this);
        }

        public async Task Execute(BehaviorContext<ShakeState> context, Behavior<ShakeState> next)
        {
            await Execute(context);

            await next.Execute(context);
        }

        public async Task Execute<T>(BehaviorContext<ShakeState, T> context, Behavior<ShakeState, T> next)
        {
            await Execute(context);

            await next.Execute(context);
        }

        public Task Faulted<TException>(BehaviorExceptionContext<ShakeState, TException> context, Behavior<ShakeState> next)
            where TException : Exception
        {
            return next.Faulted(context);
        }

        public Task Faulted<T, TException>(BehaviorExceptionContext<ShakeState, T, TException> context,
            Behavior<ShakeState, T> next)
            where TException : Exception
        {
            return next.Faulted(context);
        }

        static async Task Execute(BehaviorContext<ShakeState> context)
        {
            ConsumeEventContext<ShakeState> consumeContext = context.CreateConsumeContext();

            await consumeContext.Publish<PourShake>(new
            {
                context.Instance.OrderId,
                OrderLineId = context.Instance.CorrelationId,
                context.Instance.Flavor,
                context.Instance.Size
            }, x =>
            {
                x.RequestId = NewId.NextGuid();
                x.ResponseAddress = consumeContext.ReceiveContext.InputAddress;
            });
        }
    }
}