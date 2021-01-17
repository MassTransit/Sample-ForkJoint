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


    public class OrderBurgersActivity :
        Activity<FutureState, SubmitOrder>
    {
        public void Probe(ProbeContext context)
        {
            context.CreateScope(nameof(OrderBurgersActivity));
        }

        public void Accept(StateMachineVisitor visitor)
        {
            visitor.Visit(this);
        }

        public async Task Execute(BehaviorContext<FutureState, SubmitOrder> context, Behavior<FutureState, SubmitOrder> next)
        {
            Burger[] burgers = context.Data.Burgers;
            if (burgers != null)
            {
                ConsumeEventContext<FutureState, SubmitOrder> consumeContext = context.CreateConsumeContext();

                await Task.WhenAll(burgers.Select(burger => consumeContext.Publish<OrderBurger>(new
                {
                    context.Data.OrderId,
                    OrderLineId = burger.BurgerId,
                    Burger = burger,
                    __RequestId = InVar.Id,
                    __ResponseAddress = consumeContext.ReceiveContext.InputAddress
                }, context.CancellationToken)));

                context.Instance.Pending.UnionWith(burgers.Select(x => x.BurgerId));
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