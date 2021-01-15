namespace ForkJoint.Api.Components.StateMachines
{
    using System;
    using System.Threading.Tasks;
    using Automatonymous;
    using Contracts;
    using GreenPipes;
    using MassTransit;


    public class OrderFryShakeActivity :
        Activity<FryShakeState, OrderFryShake>
    {
        public void Probe(ProbeContext context)
        {
            context.CreateScope(nameof(OrderFriesActivity));
        }

        public void Accept(StateMachineVisitor visitor)
        {
            visitor.Visit(this);
        }

        public async Task Execute(BehaviorContext<FryShakeState, OrderFryShake> context, Behavior<FryShakeState, OrderFryShake> next)
        {
            ConsumeEventContext<FryShakeState, OrderFryShake> consumeContext = context.CreateConsumeContext();

            await consumeContext.Publish<OrderFry>(new
            {
                context.Data.OrderId,
                context.Data.OrderLineId,
                context.Data.Size,
                __RequestId = InVar.Id,
                __ResponseAddress = consumeContext.ReceiveContext.InputAddress
            }, context.CancellationToken);

            await consumeContext.Publish<OrderShake>(new
            {
                context.Data.OrderId,
                context.Data.OrderLineId,
                context.Data.Flavor,
                context.Data.Size,
                __RequestId = InVar.Id,
                __ResponseAddress = consumeContext.ReceiveContext.InputAddress
            }, context.CancellationToken);

            await next.Execute(context);
        }

        public Task Faulted<TException>(BehaviorExceptionContext<FryShakeState, OrderFryShake, TException> context, Behavior<FryShakeState, OrderFryShake> next)
            where TException : Exception
        {
            return next.Faulted(context);
        }
    }
}