namespace ForkJoint.Application.Components.Futures
{
    using Contracts;
    using GreenPipes;
    using GreenPipes.Partitioning;
    using MassTransit;
    using MassTransit.Courier;
    using MassTransit.Courier.Contracts;
    using MassTransit.Futures;
    using MassTransit.RabbitMqTransport;
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
            ConcurrentMessageLimit = GlobalValues.ConcurrentMessageLimit ?? Environment.ProcessorCount * 4;
        }

        protected override void ConfigureSaga(IReceiveEndpointConfigurator endpointConfigurator, ISagaConfigurator<FutureState> sagaConfigurator)
        {
            if (endpointConfigurator is IRabbitMqReceiveEndpointConfigurator)
            {
                var rabbitMqReceiveEndpointConfigurator = (IRabbitMqReceiveEndpointConfigurator)endpointConfigurator;

                if (GlobalValues.PrefetchCount != null)
                    rabbitMqReceiveEndpointConfigurator.PrefetchCount = (int)GlobalValues.PrefetchCount;

                if (GlobalValues.UseQuorumQueues)
                    rabbitMqReceiveEndpointConfigurator.SetQuorumQueue();

                if (GlobalValues.UseLazyQueues && !GlobalValues.UseQuorumQueues)
                    rabbitMqReceiveEndpointConfigurator.Lazy = GlobalValues.UseLazyQueues;
            }

            endpointConfigurator.UseMessageRetry(GlobalValues.RetryPolicy);

            endpointConfigurator.UseInMemoryOutbox();

            //var partitionCount = ConcurrentMessageLimit ?? Environment.ProcessorCount * 4;

            //IPartitioner partitioner = new Partitioner(partitionCount, new Murmur3UnsafeHashGenerator());

            //endpointConfigurator.UsePartitioner<OrderBurger>(partitioner, x => x.Message.OrderLineId);
            //endpointConfigurator.UsePartitioner<BurgerCompleted>(partitioner, x => x.Message.OrderLineId);
            //endpointConfigurator.UsePartitioner<RoutingSlipCompleted>(partitioner, x => x.Message.TrackingNumber);
        }
    }
}