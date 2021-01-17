namespace ForkJoint.Api.Components.FutureActivities
{
    using System;
    using System.Threading.Tasks;
    using Automatonymous;
    using Contracts;
    using ForkJoint.Components;
    using GreenPipes;
    using MassTransit;


    public class OrderFryActivity :
        Activity<FutureState, OrderFryShake>
    {
        public void Probe(ProbeContext context)
        {
            context.CreateScope(nameof(OrderFriesActivity));
        }

        public void Accept(StateMachineVisitor visitor)
        {
            visitor.Visit(this);
        }

        public async Task Execute(BehaviorContext<FutureState, OrderFryShake> context, Behavior<FutureState, OrderFryShake> next)
        {
            ConsumeEventContext<FutureState, OrderFryShake> consumeContext = context.CreateConsumeContext();

            var orderLineId = NewId.NextGuid();

            await consumeContext.Publish<OrderFry>(new
            {
                OrderId = context.Instance.CorrelationId,
                OrderLineId = orderLineId,
                context.Data.Size,
                __RequestId = InVar.Id,
                __ResponseAddress = consumeContext.ReceiveContext.InputAddress
            }, context.CancellationToken);

            context.Instance.Pending.Add(orderLineId);

            await next.Execute(context);
        }

        public Task Faulted<TException>(BehaviorExceptionContext<FutureState, OrderFryShake, TException> context, Behavior<FutureState, OrderFryShake> next)
            where TException : Exception
        {
            return next.Faulted(context);
        }
    }
}