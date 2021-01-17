namespace ForkJoint.Api.Components.Futures
{
    using System.Threading.Tasks;
    using Automatonymous;
    using Contracts;
    using ForkJoint.Components;
    using MassTransit.Courier;
    using MassTransit.Courier.Contracts;


    public class BurgerFuture :
        RoutingSlipFuture<OrderBurger, BurgerCompleted>
    {
        public BurgerFuture()
        {
            Event(() => FutureRequested, x => x.CorrelateById(context => context.Message.Burger.BurgerId));
        }

        protected override Task<BurgerCompleted> CreateCompleted(ConsumeEventContext<FutureState, RoutingSlipCompleted> context)
        {
            var burger = context.Data.GetVariable<Burger>(nameof(BurgerCompleted.Burger));

            return Init<RoutingSlipCompleted, BurgerCompleted>(context, new
            {
                Burger = burger,
                Description = burger.ToString()
            });
        }
    }
}