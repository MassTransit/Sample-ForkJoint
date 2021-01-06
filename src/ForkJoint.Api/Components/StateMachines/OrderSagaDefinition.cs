namespace ForkJoint.Api.Components.StateMachines
{
    using System;
    using Contracts;
    using GreenPipes;
    using GreenPipes.Partitioning;
    using MassTransit;
    using MassTransit.Definition;
    using MassTransit.RabbitMqTransport;


    public class OrderSagaDefinition :
        SagaDefinition<OrderState>
    {
        public OrderSagaDefinition()
        {
            var partitionCount = 32;

            ConcurrentMessageLimit = partitionCount;
        }

        protected override void ConfigureSaga(IReceiveEndpointConfigurator endpointConfigurator, ISagaConfigurator<OrderState> sagaConfigurator)
        {
            if (endpointConfigurator is IRabbitMqReceiveEndpointConfigurator rabbit)
            {
                endpointConfigurator.ConfigureConsumeTopology = false;
                rabbit.Bind<SubmitOrder>();
            }

            var partitionCount = ConcurrentMessageLimit ?? Environment.ProcessorCount * 4;

            IPartitioner partitioner = new Partitioner(partitionCount, new Murmur3UnsafeHashGenerator());

            endpointConfigurator.UsePartitioner<SubmitOrder>(partitioner, x => x.Message.OrderId);
            endpointConfigurator.UsePartitioner<BurgerCompleted>(partitioner, x => x.Message.OrderId);
            endpointConfigurator.UsePartitioner<BurgerFaulted>(partitioner, x => x.Message.OrderId);

            endpointConfigurator.UseScheduledRedelivery(r => r.Intervals(1000));
            endpointConfigurator.UseMessageRetry(r => r.Intervals(100));
            endpointConfigurator.UseInMemoryOutbox();
        }
    }
}