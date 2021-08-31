namespace ForkJoint.Application.Components.Futures
{
    using Contracts;
    using GreenPipes;
    using MassTransit;
    using MassTransit.Courier;
    using MassTransit.Futures;
    using MassTransit.Registration;
    using System;

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
            //ConcurrentMessageLimit = ConcurrentMessageLimits.GlobalValue;

            ConcurrentMessageLimit = Environment.ProcessorCount * 4;
        }

        protected override void ConfigureSaga(IReceiveEndpointConfigurator endpointConfigurator, ISagaConfigurator<FutureState> sagaConfigurator)
        {
            endpointConfigurator.UseMessageRetry(cfg => cfg.Intervals(500, 15000, 60000));

            endpointConfigurator.UseInMemoryOutbox();
        }
    }
}