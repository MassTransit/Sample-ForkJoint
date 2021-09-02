namespace ForkJoint.Application.Components.Activities
{
    using ForkJoint.Application.Services;
    using GreenPipes;
    using GreenPipes.Partitioning;
    using MassTransit;
    using MassTransit.Courier;
    using MassTransit.Courier.Contracts;
    using MassTransit.Definition;
using MassTransit.RabbitMqTransport;
    using Microsoft.Extensions.Logging;
    using System;
    using System.Threading.Tasks;

    public class GrillBurgerActivity :
        IActivity<GrillBurgerArguments, GrillBurgerLog>
    {
        readonly IGrill _grill;
        readonly ILogger<GrillBurgerActivity> _logger;

        public GrillBurgerActivity(ILogger<GrillBurgerActivity> logger, IGrill grill)
        {
            _logger = logger;
            _grill = grill;
        }

        public async Task<ExecutionResult> Execute(ExecuteContext<GrillBurgerArguments> context)
        {
            var patty = await _grill.CookOrUseExistingPatty(context.Arguments.Weight, context.Arguments.Cheese);

            return context.CompletedWithVariables<GrillBurgerLog>(new { patty }, new { patty });
        }

        public Task<CompensationResult> Compensate(CompensateContext<GrillBurgerLog> context)
        {
            var patty = context.Log.Patty;

            _logger.LogDebug("Putting Burger back in inventory: {Weight} {Cheese}", patty.Weight, patty.Cheese);

            _grill.Add(patty);

            return Task.FromResult(context.Compensated());
        }
    }

    public class GrillBurgerActivityefinition : ActivityDefinition<GrillBurgerActivity, GrillBurgerArguments, GrillBurgerLog>
    {
        public GrillBurgerActivityefinition()
        {
            ConcurrentMessageLimit = GlobalValues.ConcurrentMessageLimit ?? Environment.ProcessorCount * 4;
        }

        protected override void ConfigureExecuteActivity(IReceiveEndpointConfigurator endpointConfigurator, IExecuteActivityConfigurator<GrillBurgerActivity, GrillBurgerArguments> executeActivityConfigurator)
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

            endpointConfigurator.UseMessageRetry(cfg => cfg.Intervals(500, 15000, 60000, 120000));

            endpointConfigurator.UseInMemoryOutbox();

            //var partitionCount = ConcurrentMessageLimit ?? Environment.ProcessorCount * 4;

            //IPartitioner partitioner = new Partitioner(partitionCount, new Murmur3UnsafeHashGenerator());

            //endpointConfigurator.UsePartitioner<RoutingSlip>(partitioner, x => x.Message.TrackingNumber);
        }

        protected override void ConfigureCompensateActivity(IReceiveEndpointConfigurator endpointConfigurator, ICompensateActivityConfigurator<GrillBurgerActivity, GrillBurgerLog> compensateActivityConfigurator)
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

            endpointConfigurator.UseMessageRetry(cfg => cfg.Intervals(500, 15000, 60000, 120000));

            endpointConfigurator.UseInMemoryOutbox();

            //var partitionCount = ConcurrentMessageLimit ?? Environment.ProcessorCount * 4;

            //IPartitioner partitioner = new Partitioner(partitionCount, new Murmur3UnsafeHashGenerator());

            //endpointConfigurator.UsePartitioner<RoutingSlip>(partitioner, x => x.Message.TrackingNumber);
        }
    }
}