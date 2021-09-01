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
            if (endpointConfigurator is IRabbitMqReceiveEndpointConfigurator && GlobalValues.UseLazyQueues)
            {
                ((IRabbitMqReceiveEndpointConfigurator)endpointConfigurator).Lazy = GlobalValues.UseLazyQueues;
            }

            if (endpointConfigurator is IRabbitMqReceiveEndpointConfigurator && GlobalValues.PrefetchCount != null)
            {
                ((IRabbitMqReceiveEndpointConfigurator)endpointConfigurator).PrefetchCount = (int)GlobalValues.PrefetchCount;
            }

            endpointConfigurator.UseMessageRetry(cfg => cfg.Intervals(500, 15000, 60000));

            endpointConfigurator.UseInMemoryOutbox();

            //var partitionCount = ConcurrentMessageLimit ?? Environment.ProcessorCount * 4;

            //IPartitioner partitioner = new Partitioner(partitionCount, new Murmur3UnsafeHashGenerator());

            //endpointConfigurator.UsePartitioner<RoutingSlip>(partitioner, x => x.Message.TrackingNumber);
        }
    }
}