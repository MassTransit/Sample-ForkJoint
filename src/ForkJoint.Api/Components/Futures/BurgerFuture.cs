namespace ForkJoint.Api.Components.Futures
{
    using System;
    using Contracts;
    using ForkJoint.Components;
    using MassTransit.Courier;


    public class BurgerFuture :
        RoutingSlipFuture<OrderBurger, BurgerCompleted>
    {
        public BurgerFuture()
        {
            Event(() => FutureRequested, x => x.CorrelateById(context => context.Message.Burger.BurgerId));
            Event(() => RequestFutureRequested, x => x.CorrelateById(context => context.Message.Request.Burger.BurgerId));

            Response(x => x.Init(context =>
            {
                var burger = context.Message.GetVariable<Burger>(nameof(BurgerCompleted.Burger));

                return new
                {
                    OrderId = context.Message.GetVariable<Guid>(nameof(OrderBurger.OrderId)),
                    OrderLineId = context.Message.GetVariable<Guid>(nameof(OrderBurger.OrderLineId)),
                    Burger = burger,
                    Description = burger.ToString()
                };
            }));
        }
    }
}