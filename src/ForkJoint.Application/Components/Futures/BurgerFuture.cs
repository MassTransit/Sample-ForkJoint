namespace ForkJoint.Application.Components.Futures
{
    using Contracts;
    using GreenPipes;
    using MassTransit;
    using MassTransit.Courier;
    using MassTransit.Futures;
    using MassTransit.Registration;

    public class BurgerFuture :
        Future<OrderBurger, BurgerCompleted>
    {
        public BurgerFuture()
        {
            ConfigureCommand(x => x.CorrelateById(context => context.Message.OrderLineId));

            ExecuteRoutingSlip(x => x
                .OnRoutingSlipCompleted(r => r
                    .SetCompletedUsingInitializer(context =>
                    {
                        var burger = context.Message.GetVariable<Burger>(nameof(BurgerCompleted.Burger));

                        return new
                        {
                            Burger = burger,
                            Description = burger.ToString()
                        };
                    })));
        }
    }

    public class BurgerFutureDefinition : FutureDefinition<BurgerFuture>
    {
        public BurgerFutureDefinition()
        {
            ConcurrentMessageLimit = ConcurrentMessageLimits.GlobalValue;
        }

        protected override void ConfigureSaga(IReceiveEndpointConfigurator endpointConfigurator, ISagaConfigurator<FutureState> sagaConfigurator)
        {
            endpointConfigurator.UseMessageRetry(cfg => cfg.Immediate(5));
            endpointConfigurator.UseInMemoryOutbox();
        }
    }
}