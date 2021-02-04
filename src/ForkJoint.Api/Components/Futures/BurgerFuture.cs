namespace ForkJoint.Api.Components.Futures
{
    using Contracts;
    using ForkJoint.Components;
    using MassTransit.Courier;


    public class BurgerFuture :
        RoutingSlipFuture<OrderBurger, BurgerCompleted>
    {
        public BurgerFuture()
        {
            Event(() => FutureRequested, x => x.CorrelateById(context => context.Message.Burger.BurgerId));

            Response(x => x.Init(context =>
            {
                var burger = context.Message.GetVariable<Burger>(nameof(BurgerCompleted.Burger));

                return new
                {
                    Burger = burger,
                    Description = burger.ToString()
                };
            }));
        }
    }
}