namespace ForkJoint.Api.Components.StateMachines
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using Automatonymous;
    using Contracts;
    using GreenPipes;
    using MassTransit;


    public class RequestBurgerActivity :
        Activity<OrderState, SubmitOrder>
    {
        public void Probe(ProbeContext context)
        {
            context.CreateScope("requestBurger");
        }

        public void Accept(StateMachineVisitor visitor)
        {
            visitor.Visit(this);
        }

        public async Task Execute(BehaviorContext<OrderState, SubmitOrder> context, Behavior<OrderState, SubmitOrder> next)
        {
            ConsumeEventContext<OrderState, SubmitOrder> consumeContext = context.CreateConsumeContext();

            await Task.WhenAll(context.Data.Burgers.Select(burger => consumeContext.Publish<OrderBurger>(new
            {
                context.Data.OrderId,
                Burger = burger,
                __RequestId = InVar.Id,
                __ResponseAddress = consumeContext.ReceiveContext.InputAddress
            })));

            // THIS IS SLOW
            // foreach (var burger in context.Data.Burgers)
            // {
            //     await consumeContext.Publish<OrderBurger>(new
            //     {
            //         context.Data.OrderId,
            //         Burger = burger,
            //         __RequestId = InVar.Id,
            //         __ResponseAddress = consumeContext.ReceiveContext.InputAddress
            //     });
            // }

            await next.Execute(context);
        }

        public Task Faulted<TException>(BehaviorExceptionContext<OrderState, SubmitOrder, TException> context, Behavior<OrderState, SubmitOrder> next)
            where TException : Exception
        {
            return next.Faulted(context);
        }
    }
}