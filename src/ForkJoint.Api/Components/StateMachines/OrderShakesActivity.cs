namespace ForkJoint.Api.Components.StateMachines
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using Automatonymous;
    using Contracts;
    using GreenPipes;
    using MassTransit;


    public class OrderShakesActivity :
        Activity<OrderState, SubmitOrder>
    {
        public void Probe(ProbeContext context)
        {
            context.CreateScope(nameof(OrderShakesActivity));
        }

        public void Accept(StateMachineVisitor visitor)
        {
            visitor.Visit(this);
        }

        public async Task Execute(BehaviorContext<OrderState, SubmitOrder> context, Behavior<OrderState, SubmitOrder> next)
        {
            ConsumeEventContext<OrderState, SubmitOrder> consumeContext = context.CreateConsumeContext();

            if (context.Data.Shakes != null)
                await Task.WhenAll(context.Data.Shakes.Select(shake => consumeContext.Publish<OrderShake>(new
                {
                    context.Data.OrderId,
                    OrderLineId = shake.ShakeId,
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