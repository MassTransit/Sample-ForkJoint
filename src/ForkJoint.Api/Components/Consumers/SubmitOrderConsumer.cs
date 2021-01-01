namespace ForkJoint.Api.Components.Consumers
{
    using System;
    using System.Threading.Tasks;
    using Activities.DressBurger;
    using Activities.GrillBurger;
    using Contracts;
    using MassTransit;
    using MassTransit.Courier;
    using MassTransit.Courier.Contracts;
    using Microsoft.Extensions.Logging;


    public class SubmitOrderConsumer :
        IConsumer<SubmitOrder>
    {
        readonly IEndpointNameFormatter _formatter;
        readonly ILogger<SubmitOrderConsumer> _logger;

        public SubmitOrderConsumer(ILogger<SubmitOrderConsumer> logger, IEndpointNameFormatter formatter)
        {
            _logger = logger;
            _formatter = formatter;
        }

        public async Task Consume(ConsumeContext<SubmitOrder> context)
        {
            _logger.LogDebug("Order Submission Received: {OrderId} {CorrelationId}", context.Message.OrderId, context.CorrelationId);

            var routingSlip = CreateRoutingSlip(context.Message);

            await context.Execute(routingSlip);

            if (context.ResponseAddress != null)
                await context.RespondAsync<OrderSubmissionAccepted>(new {context.Message.OrderId});
        }

        RoutingSlip CreateRoutingSlip(SubmitOrder submitOrder)
        {
            var builder = new RoutingSlipBuilder(NewId.NextGuid());

            builder.AddVariable("OrderId", submitOrder.OrderId);


            var grillQueueName = _formatter.ExecuteActivity<GrillBurgerActivity, GrillBurgerArguments>();
            builder.AddActivity("grill-burger", new Uri($"queue:{grillQueueName}"), new
            {
                Weight = 0.5m,
                Temperature = 165.0m
            });

            var dressQueueName = _formatter.ExecuteActivity<DressBurgerActivity, DressBurgerArguments>();
            builder.AddActivity("dress-burger", new Uri($"queue:{dressQueueName}"), new
            {
                Ketchup = true,
                Lettuce = true,
            });

            return builder.Build();
        }
    }
}