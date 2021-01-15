namespace ForkJoint.Api.Components.StateMachines
{
    using System;
    using Contracts;
    using GreenPipes;
    using GreenPipes.Partitioning;
    using MassTransit;
    using MassTransit.Definition;
    using MassTransit.RabbitMqTransport;


    public class ShakeSagaDefinition :
        SagaDefinition<ShakeState>
    {
        public ShakeSagaDefinition()
        {
            var partitionCount = 32;

            ConcurrentMessageLimit = partitionCount;
        }

        protected override void ConfigureSaga(IReceiveEndpointConfigurator endpointConfigurator, ISagaConfigurator<ShakeState> sagaConfigurator)
        {
            if (endpointConfigurator is IRabbitMqReceiveEndpointConfigurator rabbit)
            {
                endpointConfigurator.ConfigureConsumeTopology = false;
                rabbit.Bind<OrderShake>();
            }

            var partitionCount = ConcurrentMessageLimit ?? Environment.ProcessorCount * 4;

            IPartitioner partitioner = new Partitioner(partitionCount, new Murmur3UnsafeHashGenerator());

            endpointConfigurator.UsePartitioner<OrderShake>(partitioner, x => x.Message.OrderLineId);
            endpointConfigurator.UsePartitioner<ShakeReady>(partitioner, x => x.Message.OrderLineId);
            endpointConfigurator.UsePartitioner<Fault<PourShake>>(partitioner, x => x.Message.Message.OrderLineId);

            endpointConfigurator.UseScheduledRedelivery(r => r.Intervals(1000));
            endpointConfigurator.UseMessageRetry(r => r.Intervals(100));
            endpointConfigurator.UseInMemoryOutbox();
        }
    }
}