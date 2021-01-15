namespace ForkJoint.Api.Components.StateMachines
{
    using System;
    using System.Threading.Tasks;
    using Automatonymous;
    using Contracts;
    using GreenPipes;
    using MassTransit;


    public class CookFryActivity :
        Activity<FryState>
    {
        public void Probe(ProbeContext context)
        {
            context.CreateScope(nameof(CookFryActivity));
        }

        public void Accept(StateMachineVisitor visitor)
        {
            visitor.Visit(this);
        }

        public async Task Execute(BehaviorContext<FryState> context, Behavior<FryState> next)
        {
            await Execute(context);

            await next.Execute(context);
        }

        public async Task Execute<T>(BehaviorContext<FryState, T> context, Behavior<FryState, T> next)
        {
            await Execute(context);

            await next.Execute(context);
        }

        public Task Faulted<TException>(BehaviorExceptionContext<FryState, TException> context, Behavior<FryState> next)
            where TException : Exception
        {
            return next.Faulted(context);
        }

        public Task Faulted<T, TException>(BehaviorExceptionContext<FryState, T, TException> context,
            Behavior<FryState, T> next)
            where TException : Exception
        {
            return next.Faulted(context);
        }

        static async Task Execute(BehaviorContext<FryState> context)
        {
            ConsumeEventContext<FryState> consumeContext = context.CreateConsumeContext();

            await consumeContext.Publish<CookFry>(new
            {
                context.Instance.OrderId,
                OrderLineId = context.Instance.CorrelationId,
                context.Instance.Size
            }, x =>
            {
                x.RequestId = NewId.NextGuid();
                x.ResponseAddress = consumeContext.ReceiveContext.InputAddress;
            });
        }
    }
}