namespace ForkJoint.Application.Components.Consumers
{
    using Contracts;
    using ForkJoint.Application.Services;
    using GreenPipes;
    using MassTransit;
    using MassTransit.ConsumeConfigurators;
    using MassTransit.Definition;
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
            ConcurrentMessageLimit = ConcurrentMessageLimits.GlobalValue;
        }

        protected override void ConfigureConsumer(IReceiveEndpointConfigurator endpointConfigurator, IConsumerConfigurator<CookFryConsumer> consumerConfigurator)
        {
            endpointConfigurator.UseMessageRetry(cfg => cfg.Immediate(5));
            endpointConfigurator.UseInMemoryOutbox();
        }
    }
}