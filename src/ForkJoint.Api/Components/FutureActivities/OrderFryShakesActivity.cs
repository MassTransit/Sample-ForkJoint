namespace ForkJoint.Api.Components.FutureActivities
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using Automatonymous;
    using Contracts;
    using ForkJoint.Components;
    using GreenPipes;
    using MassTransit;


    public class OrderFryShakesActivity :
        Activity<FutureState, SubmitOrder>
    {
        public void Probe(ProbeContext context)
        {
            context.CreateScope(nameof(OrderFryShakesActivity));
        }

        public void Accept(StateMachineVisitor visitor)
        {
            visitor.Visit(this);
        }

        public async Task Execute(BehaviorContext<FutureState, SubmitOrder> context, Behavior<FutureState, SubmitOrder> next)
        {
            ConsumeEventContext<FutureState, SubmitOrder> consumeContext = context.CreateConsumeContext();

            if (context.Data.FryShakes != null)
            {
                await Task.WhenAll(context.Data.FryShakes.Select(shake => consumeContext.Publish<OrderFryShake>(new
                {
                    context.Data.OrderId,
                    OrderLineId = shake.FryShakeId,
                    shake.Flavor,
                    shake.Size,
                    __RequestId = InVar.Id,
                    __ResponseAddress = consumeContext.ReceiveContext.InputAddress
                }, context.CancellationToken)));

                context.Instance.Pending.UnionWith(context.Data.FryShakes.Select(x => x.FryShakeId));
            }

            await next.Execute(context);
        }

        public Task Faulted<TException>(BehaviorExceptionContext<FutureState, SubmitOrder, TException> context, Behavior<FutureState, SubmitOrder> next)
            where TException : Exception
        {
            return next.Faulted(context);
        }
    }
}