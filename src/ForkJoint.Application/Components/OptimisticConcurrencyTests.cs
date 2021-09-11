namespace ForkJoint.Application.Components
{
    using Automatonymous;
    using MassTransit;
    using MassTransit.Definition;
    using MassTransit.EntityFrameworkCoreIntegration.Mappings;
    using MassTransit.RabbitMqTransport;
    using MassTransit.Saga;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;
    using System;

    public class OptimisticConcurrencyTestsState :
        SagaStateMachineInstance,
        ISagaVersion
    {
        public int CurrentState { get; set; }

        public DateTime Created { get; set; }
        public DateTime? Completed { get; set; }
        public DateTime? Faulted { get; set; }

        public int Version { get; set; }

        public byte[] RowVersion { get; set; }

        public Guid CorrelationId { get; set; }
    }

    public class OptimisticConcurrencyTestsStateMap :
        SagaClassMap<OptimisticConcurrencyTestsState>
    {
        protected override void Configure(EntityTypeBuilder<OptimisticConcurrencyTestsState> entity, ModelBuilder model)
        {
            entity.Property(x => x.CurrentState);

            entity.Property(x => x.RowVersion).IsRowVersion();

            entity.Property(x => x.Created);
            entity.Property(x => x.Completed);
            entity.Property(x => x.Faulted);
        }
    }

    public class OptimisticConcurrencyTestsStateMachine : MassTransitStateMachine<OptimisticConcurrencyTestsState>
    {
        public Event<Create> Create { get; }
        public Event<Update> Update { get; }

        public State Created { get; }

        public OptimisticConcurrencyTestsStateMachine()
        {
            InstanceState(x => x.CurrentState, Created);

            Event(() => Create, x => x.CorrelateById(m => m.Message.Id));
            Event(() => Update, x => x.CorrelateById(m => m.Message.Id));

            Initially(
                When(Create)
                    .TransitionTo(Created)
                    );

            During(Created,
                Ignore(Create),
                When(Update)
                    .Then(ctx => ctx.Instance.Completed = DateTime.UtcNow)
                );
        }
    }

    public class OptimisticConcurrencyTestsSagaDefinition : SagaDefinition<OptimisticConcurrencyTestsState>
    {
        public OptimisticConcurrencyTestsSagaDefinition()
        {
            ConcurrentMessageLimit = GlobalValues.ConcurrentMessageLimit ?? Environment.ProcessorCount * 4;
        }

        protected override void ConfigureSaga(IReceiveEndpointConfigurator endpointConfigurator, ISagaConfigurator<OptimisticConcurrencyTestsState> sagaConfigurator)
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
        }
    }

    public interface Create
    {
        public Guid Id { get; }
    }

    public interface Update
    {
        public Guid Id { get; }
    }
}