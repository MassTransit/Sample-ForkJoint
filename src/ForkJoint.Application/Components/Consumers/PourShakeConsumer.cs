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

    public class PourShakeConsumer :
        IConsumer<PourShake>
    {
        readonly IShakeMachine _shakeMachine;

        public PourShakeConsumer(IShakeMachine shakeMachine)
        {
            _shakeMachine = shakeMachine;
        }

        public async Task Consume(ConsumeContext<PourShake> context)
        {
            await _shakeMachine.PourShake(context.Message.Flavor,
                context.Message.Size);

            await context.RespondAsync<ShakeReady>(new
            {
                context.Message.OrderId,
                context.Message.OrderLineId,
                context.Message.Flavor,
                context.Message.Size
            });
        }
    }

    public class PourShakeConsumerDefinition : ConsumerDefinition<PourShakeConsumer>
    {
        public PourShakeConsumerDefinition()
        {
            //ConcurrentMessageLimit = ConcurrentMessageLimits.GlobalValue;

            ConcurrentMessageLimit = Environment.ProcessorCount * 4;
        }

        protected override void ConfigureConsumer(IReceiveEndpointConfigurator endpointConfigurator, IConsumerConfigurator<PourShakeConsumer> consumerConfigurator)
        {
            endpointConfigurator.UseMessageRetry(cfg => cfg.Intervals(500, 15000, 60000));

            endpointConfigurator.UseInMemoryOutbox();
        }
    }
}