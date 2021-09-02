namespace ForkJoint.Application.Components.Activities
{
    using Contracts;
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

    public class DressBurgerActivity :
        IExecuteActivity<DressBurgerArguments>
    {
        readonly ILogger<DressBurgerActivity> _logger;
        readonly IRequestClient<OrderOnionRings> _onionRingClient;

        public DressBurgerActivity(ILogger<DressBurgerActivity> logger, IRequestClient<OrderOnionRings> onionRingClient)
        {
            _logger = logger;
            _onionRingClient = onionRingClient;
        }

        public async Task<ExecutionResult> Execute(ExecuteContext<DressBurgerArguments> context)
        {
            var arguments = context.Arguments;

            var patty = arguments.Patty;
            if (patty == null)
                throw new ArgumentNullException(nameof(arguments.Patty));

            _logger.LogDebug("Dressing Burger: {OrderId} {Ketchup} {Lettuce}", arguments.OrderId, arguments.Ketchup,
                arguments.Lettuce);

            if (arguments.Lettuce)
                throw new InvalidOperationException("No lettuce available");

            if (arguments.OnionRing)
            {
                Guid? onionRingId = arguments.OnionRingId ?? NewId.NextGuid();

                _logger.LogDebug("Ordering Onion Ring: {OrderId}", onionRingId);

                Response<OnionRingsCompleted> response = await _onionRingClient.GetResponse<OnionRingsCompleted>(new
                {
                    arguments.OrderId,
                    OrderLineId = onionRingId,
                    Quantity = 1
                }, context.CancellationToken);
            }

            var burger = new Burger
            {
                BurgerId = arguments.BurgerId,
                Weight = patty.Weight,
                Cheese = patty.Cheese,
                Lettuce = arguments.Lettuce,
                Onion = arguments.Onion,
                Pickle = arguments.Pickle,
                Ketchup = arguments.Ketchup,
                Mustard = arguments.Mustard,
                BarbecueSauce = arguments.BarbecueSauce,
                OnionRing = arguments.OnionRing
            };

            return context.CompletedWithVariables(new { burger });
        }
    }

    public class DressBurgerActivityDefinition : ExecuteActivityDefinition<DressBurgerActivity, DressBurgerArguments>
    {
        public DressBurgerActivityDefinition()
        {
            ConcurrentMessageLimit = GlobalValues.ConcurrentMessageLimit ?? Environment.ProcessorCount * 4;
        }

        protected override void ConfigureExecuteActivity(IReceiveEndpointConfigurator endpointConfigurator, IExecuteActivityConfigurator<DressBurgerActivity, DressBurgerArguments> executeActivityConfigurator)
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