namespace ForkJoint.Api.Components.StateMachines
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using Automatonymous;
    using Contracts;
    using GreenPipes;
    using MassTransit;


    public class OrderFryShakesActivity :
        Activity<OrderState, SubmitOrder>
    {
        public void Probe(ProbeContext context)
        {
            context.CreateScope(nameof(OrderFryShakesActivity));
        }

        public void Accept(StateMachineVisitor visitor)
        {
            visitor.Visit(this);
        }

        public async Task Execute(BehaviorContext<OrderState, SubmitOrder> context, Behavior<OrderState, SubmitOrder> next)
        {
            ConsumeEventContext<OrderState, SubmitOrder> consumeContext = context.CreateConsumeContext();

            if (context.Data.FryShakes != null)
                await Task.WhenAll(context.Data.FryShakes.Select(shake => consumeContext.Publish<OrderFryShake>(new
                {
                    context.Data.OrderId,
                    OrderLineId = shake.FryShakeId,
                    shake.Flavor,
                    shake.Size,
                    __RequestId = InVar.Id,
                    __ResponseAddress = consumeContext.ReceiveContext.InputAddress
                }, context.CancellationToken)));

            await next.Execute(context);
        }

        public Task Faulted<TException>(BehaviorExceptionContext<OrderState, SubmitOrder, TException> context, Behavior<OrderState, SubmitOrder> next)
            where TException : Exception
        {
            return next.Faulted(context);
        }
    }
}