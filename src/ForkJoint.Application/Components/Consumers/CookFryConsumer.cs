namespace ForkJoint.Application.Components.Consumers
{
    using Contracts;
    using ForkJoint.Application.Services;
    using GreenPipes;
    using MassTransit;
    using MassTransit.ConsumeConfigurators;
    using MassTransit.Definition;
using MassTransit.RabbitMqTransport;
    using System;
    using System.Threading.Tasks;

    public class CookFryConsumer :
        IConsumer<CookFry>
    {
        readonly IFryer _fryer;

        public CookFryConsumer(IFryer fryer)
        {
            _fryer = fryer;
        }

        public async Task Consume(ConsumeContext<CookFry> context)
        {
            await _fryer.CookFry(context.Message.Size);

            await context.RespondAsync<FryReady>(new
            {
                context.Message.OrderId,
                context.Message.OrderLineId,
                context.Message.Size
            });
        }
    }

    public class CookFryConsumerDefinition : ConsumerDefinition<CookFryConsumer>
    {
        public CookFryConsumerDefinition()
        {
            ConcurrentMessageLimit = GlobalValues.ConcurrentMessageLimit ?? Environment.ProcessorCount * 4;
        }

        protected override void ConfigureConsumer(IReceiveEndpointConfigurator endpointConfigurator, IConsumerConfigurator<CookFryConsumer> consumerConfigurator)
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
        }
    }
}