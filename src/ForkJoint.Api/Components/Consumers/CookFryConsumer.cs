namespace ForkJoint.Api.Components.Consumers
{
    using System.Threading.Tasks;
    using Contracts;
    using MassTransit;
    using MassTransit.Definition;
    using Services;


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

    public class CookFryConsumerDefinition: ConsumerDefinition<CookFryConsumer>
    {
        public CookFryConsumerDefinition()
        {
            ConcurrentMessageLimit = 32;
        }
    }
}