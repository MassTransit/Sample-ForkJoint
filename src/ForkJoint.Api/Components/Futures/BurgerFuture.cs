namespace ForkJoint.Api.Components.Futures
{
    using Contracts;
    using MassTransit.Courier;
    using MassTransit.Futures;


    public class BurgerFuture :
        Future<OrderBurger, BurgerCompleted>
    {
        public BurgerFuture()
        {
            Event(() => FutureRequested, x => x.CorrelateById(context => context.Message.Burger.BurgerId));

            ExecuteRoutingSlip(x =>
            {
                x.Response(r => r.Init(context =>
                {
                    var burger = context.Message.GetVariable<Burger>(nameof(BurgerCompleted.Burger));

                    return new
                    {
                        Burger = burger,
                        Description = burger.ToString()
                    };
                }));
            });
        }
    }
}
