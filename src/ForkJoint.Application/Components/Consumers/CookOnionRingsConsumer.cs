namespace ForkJoint.Application.Components.Consumers
{
    using Contracts;
    using ForkJoint.Application.Services;
    using GreenPipes;
    using MassTransit;
    using MassTransit.ConsumeConfigurators;
    using MassTransit.Definition;
    using System;
    using System.Threading.Tasks;

    public class CookOnionRingsConsumer :
        IConsumer<CookOnionRings>
    {
        readonly IFryer _fryer;

        public CookOnionRingsConsumer(IFryer fryer)
        {
            _fryer = fryer;
        }

        public async Task Consume(ConsumeContext<CookOnionRings> context)
        {
            await _fryer.CookOnionRings(context.Message.Quantity);

            await context.RespondAsync<OnionRingsReady>(new
            {
                context.Message.OrderId,
                context.Message.OrderLineId,
                context.Message.Quantity
            });
        }
    }

    public class CookOnionRingsConsumerDefinition : ConsumerDefinition<CookOnionRingsConsumer>
    {
        public CookOnionRingsConsumerDefinition()
        {
            //ConcurrentMessageLimit = ConcurrentMessageLimits.GlobalValue;

            ConcurrentMessageLimit = Environment.ProcessorCount * 4;
        }

        protected override void ConfigureConsumer(IReceiveEndpointConfigurator endpointConfigurator, IConsumerConfigurator<CookOnionRingsConsumer> consumerConfigurator)
        {
            endpointConfigurator.UseMessageRetry(cfg => cfg.Intervals(500, 15000, 60000));

            endpointConfigurator.UseInMemoryOutbox();
        }
    }
}