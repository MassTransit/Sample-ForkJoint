namespace ForkJoint.Api.Components.Consumers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Activities.DressBurger;
    using Activities.GrillBurger;
    using Contracts;
    using MassTransit;
    using MassTransit.Courier;
    using MassTransit.Courier.Contracts;
    using MassTransit.Events;
    using MassTransit.Metadata;
    using Microsoft.Extensions.Logging;


    public class SubmitOrderConsumer :
        IConsumer<SubmitOrder>,
        IConsumer<RoutingSlipCompleted>,
        IConsumer<RoutingSlipFaulted>
    {
        readonly IEndpointNameFormatter _formatter;
        readonly ILogger<SubmitOrderConsumer> _logger;

        public SubmitOrderConsumer(ILogger<SubmitOrderConsumer> logger, IEndpointNameFormatter formatter)
        {
            _logger = logger;
            _formatter = formatter;
        }

        public async Task Consume(ConsumeContext<RoutingSlipCompleted> context)
        {
            var orderId = context.Message.GetVariable<Guid>("OrderId");
            var requestId = context.Message.GetVariable<Guid?>(nameof(ConsumeContext.RequestId));
            var responseAddress = context.Message.GetVariable<Uri>(nameof(ConsumeContext.ResponseAddress));

            if (requestId.HasValue && responseAddress != null)
            {
                if (context.Message.Variables.ContainsKey("Deadline"))
                {
                    var deadLine = context.Message.GetVariable<DateTime>("Deadline");
                    if (deadLine <= DateTime.UtcNow)
                        return;
                }

                var responseEndpoint = await context.GetResponseEndpoint<OrderSubmissionAccepted>(responseAddress);

                await responseEndpoint.Send<OrderSubmissionAccepted>(new {OrderId = orderId});
            }
        }

        public async Task Consume(ConsumeContext<RoutingSlipFaulted> context)
        {
            var requestId = context.Message.GetVariable<Guid?>(nameof(ConsumeContext.RequestId));
            var responseAddress = context.Message.GetVariable<Uri>(nameof(ConsumeContext.ResponseAddress));
            var request = context.Message.GetVariable<SubmitOrder>("Request");

            if (requestId.HasValue && responseAddress != null)
            {
                var responseEndpoint = await context.GetResponseEndpoint<OrderSubmissionAccepted>(responseAddress);

                IEnumerable<ExceptionInfo> exceptions = context.Message.ActivityExceptions.Select(x => x.ExceptionInfo);

                Fault<SubmitOrder> response =
                    new FaultEvent<SubmitOrder>(request, requestId, context.Host, exceptions, TypeMetadataCache<SubmitOrder>.MessageTypeNames);

                await responseEndpoint.Send(response);
            }
        }

        public async Task Consume(ConsumeContext<SubmitOrder> context)
        {
            _logger.LogDebug("Order Submission Received: {OrderId} {CorrelationId}", context.Message.OrderId, context.CorrelationId);

            var routingSlip = CreateRoutingSlip(context);

            await context.Execute(routingSlip);
        }

        RoutingSlip CreateRoutingSlip(ConsumeContext<SubmitOrder> context)
        {
            var builder = new RoutingSlipBuilder(NewId.NextGuid());

            builder.AddSubscription(context.ReceiveContext.InputAddress, RoutingSlipEvents.Completed | RoutingSlipEvents.Faulted);

            builder.AddVariable("OrderId", context.Message.OrderId);

            if (context.ExpirationTime.HasValue)
                builder.AddVariable("Deadline", context.ExpirationTime.Value);

            builder.AddVariable("Request", context.Message);
            builder.AddVariable(nameof(ConsumeContext.RequestId), context.RequestId);
            builder.AddVariable(nameof(ConsumeContext.ResponseAddress), context.ResponseAddress);
            builder.AddVariable(nameof(ConsumeContext.FaultAddress), context.FaultAddress);

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