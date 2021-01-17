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


    public class OrderFriesActivity :
        Activity<FutureState, SubmitOrder>
    {
        public void Probe(ProbeContext context)
        {
            context.CreateScope(nameof(OrderFriesActivity));
        }

        public void Accept(StateMachineVisitor visitor)
        {
            visitor.Visit(this);
        }

        public async Task Execute(BehaviorContext<FutureState, SubmitOrder> context, Behavior<FutureState, SubmitOrder> next)
        {
            if (context.Data.Fries != null)
            {
                ConsumeEventContext<FutureState, SubmitOrder> consumeContext = context.CreateConsumeContext();

                await Task.WhenAll(context.Data.Fries.Select(fry => consumeContext.Publish<OrderFry>(new
                {
                    context.Data.OrderId,
                    OrderLineId = fry.FryId,
                    fry.Size,
                    __RequestId = InVar.Id,
                    __ResponseAddress = consumeContext.ReceiveContext.InputAddress
                }, context.CancellationToken)));

                context.Instance.Pending.UnionWith(context.Data.Fries.Select(x => x.FryId));
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