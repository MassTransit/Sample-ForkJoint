namespace ForkJoint.Api.Components.StateMachines
{
    using System;
    using Contracts;
    using GreenPipes;
    using GreenPipes.Partitioning;
    using MassTransit;
    using MassTransit.Definition;
    using MassTransit.RabbitMqTransport;


    public class FryShakeSagaDefinition :
        SagaDefinition<FryShakeState>
    {
        public FryShakeSagaDefinition()
        {
            var partitionCount = 32;

            ConcurrentMessageLimit = partitionCount;
        }

        protected override void ConfigureSaga(IReceiveEndpointConfigurator endpointConfigurator, ISagaConfigurator<FryShakeState> sagaConfigurator)
        {
            if (endpointConfigurator is IRabbitMqReceiveEndpointConfigurator rabbit)
            {
                endpointConfigurator.ConfigureConsumeTopology = false;
                rabbit.Bind<OrderFryShake>();
            }

            var partitionCount = ConcurrentMessageLimit ?? Environment.ProcessorCount * 4;

            IPartitioner partitioner = new Partitioner(partitionCount, new Murmur3UnsafeHashGenerator());

            endpointConfigurator.UsePartitioner<OrderFryShake>(partitioner, x => x.Message.OrderLineId);
            endpointConfigurator.UsePartitioner<FryCompleted>(partitioner, x => x.Message.OrderLineId);
            endpointConfigurator.UsePartitioner<FryFaulted>(partitioner, x => x.Message.OrderLineId);
            endpointConfigurator.UsePartitioner<ShakeCompleted>(partitioner, x => x.Message.OrderLineId);
            endpointConfigurator.UsePartitioner<ShakeFaulted>(partitioner, x => x.Message.OrderLineId);

            endpointConfigurator.UseScheduledRedelivery(r => r.Intervals(1000));
            endpointConfigurator.UseMessageRetry(r => r.Intervals(100));
            endpointConfigurator.UseInMemoryOutbox();
        }
    }
}