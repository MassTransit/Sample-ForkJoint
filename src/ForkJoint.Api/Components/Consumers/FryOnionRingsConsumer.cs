namespace ForkJoint.Api.Components.Consumers
{
    using System.Threading.Tasks;
    using Contracts;
    using MassTransit;
    using Services;


    public class FryOnionRingsConsumer :
        IConsumer<FryOnionRings>
    {
        readonly IFryer _fryer;

        public FryOnionRingsConsumer(IFryer fryer)
        {
            _fryer = fryer;
        }

        public async Task Consume(ConsumeContext<FryOnionRings> context)
        {
            await _fryer.FryOnionRings(context.Message.Quantity);

            await context.RespondAsync<OnionRingsFried>(new
            {
                context.Message.OrderId,
                context.Message.OrderLineId,
                context.Message.Quantity
            });
        }
    }
}