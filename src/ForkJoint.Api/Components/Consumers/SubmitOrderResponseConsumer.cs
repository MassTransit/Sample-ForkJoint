namespace ForkJoint.Api.Components.Consumers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Contracts;
    using MassTransit;
    using MassTransit.Courier;
    using MassTransit.Courier.Contracts;
    using MassTransit.Initializers;


    public class SubmitOrderResponseConsumer :
        RoutingSlipResponseConsumer<SubmitOrder, OrderCompleted, OrderNotCompleted>
    {
        protected override Task<OrderCompleted> CreateResponseMessage(ConsumeContext<RoutingSlipCompleted> context, SubmitOrder request)
        {
            var orderId = context.Message.GetVariable<Guid>("OrderId");

            HasVariable(context.Message.Variables, "Burger", out Burger burger);

            return MessageInitializerCache<OrderCompleted>.InitializeMessage(context, new
            {
                orderId,
                burger
            });
        }

        protected override Task<OrderNotCompleted> CreateFaultedResponseMessage(ConsumeContext<RoutingSlipFaulted> context, SubmitOrder request,
            Guid requestId)
        {
            var orderId = context.Message.GetVariable<Guid>("OrderId");

            IEnumerable<ExceptionInfo> exceptions = context.Message.ActivityExceptions.Select(x => x.ExceptionInfo);

            var reason = exceptions.FirstOrDefault()?.Message ?? "Unknown";

            return MessageInitializerCache<OrderNotCompleted>.InitializeMessage(context, new
            {
                orderId,
                Reason = reason,
                request.Burgers
            });
        }
    }
}