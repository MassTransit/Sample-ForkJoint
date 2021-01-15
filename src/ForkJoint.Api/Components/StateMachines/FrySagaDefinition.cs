namespace ForkJoint.Api.Components.StateMachines
{
    using System;
    using Contracts;
    using GreenPipes;
    using GreenPipes.Partitioning;
    using MassTransit;
    using MassTransit.Definition;
    using MassTransit.RabbitMqTransport;


    public class FrySagaDefinition :
        SagaDefinition<FryState>
    {
        public FrySagaDefinition()
        {
            var partitionCount = 32;

            ConcurrentMessageLimit = partitionCount;
        }

        protected override void ConfigureSaga(IReceiveEndpointConfigurator endpointConfigurator, ISagaConfigurator<FryState> sagaConfigurator)
        {
            if (endpointConfigurator is IRabbitMqReceiveEndpointConfigurator rabbit)
            {
                endpointConfigurator.ConfigureConsumeTopology = false;
                rabbit.Bind<OrderFry>();
            }

            var partitionCount = ConcurrentMessageLimit ?? Environment.ProcessorCount * 4;

            IPartitioner partitioner = new Partitioner(partitionCount, new Murmur3UnsafeHashGenerator());

            endpointConfigurator.UsePartitioner<OrderFry>(partitioner, x => x.Message.OrderLineId);
            endpointConfigurator.UsePartitioner<FryReady>(partitioner, x => x.Message.OrderLineId);
            endpointConfigurator.UsePartitioner<Fault<CookFry>>(partitioner, x => x.Message.Message.OrderLineId);

            endpointConfigurator.UseScheduledRedelivery(r => r.Intervals(1000));
            endpointConfigurator.UseMessageRetry(r => r.Intervals(100));
            endpointConfigurator.UseInMemoryOutbox();
        }
    }
}