namespace ForkJoint.Api.Components.Consumers
{
    using System.Threading.Tasks;
    using Contracts;
    using MassTransit;
    using Microsoft.Extensions.Logging;


    public class SubmitOrderConsumer :
        IConsumer<SubmitOrder>
    {
        readonly ILogger<SubmitOrderConsumer> _logger;

        public SubmitOrderConsumer(ILogger<SubmitOrderConsumer> logger)
        {
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<SubmitOrder> context)
        {
            _logger.LogDebug("Order Submission Received: {OrderId} {CorrelationId}", context.Message.OrderId, context.CorrelationId);

            await Task.Delay(100);

            await context.RespondAsync<OrderSubmissionAccepted>(new {context.Message.OrderId});
        }
    }
}