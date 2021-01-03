namespace ForkJoint.Api.Components.Consumers
{
    using System;
    using System.Threading.Tasks;
    using MassTransit;
    using MassTransit.Courier;
    using MassTransit.Courier.Contracts;


    public abstract class RoutingSlipRequestConsumer<TRequest> :
        IConsumer<TRequest>
        where TRequest : class
    {
        readonly Uri _subscriptionAddress;

        protected RoutingSlipRequestConsumer(string subscriptionEndpointName)
        {
            _subscriptionAddress = new Uri($"exchange:{subscriptionEndpointName}");
        }

        public async Task Consume(ConsumeContext<TRequest> context)
        {
            var builder = new RoutingSlipBuilder(NewId.NextGuid());

            builder.AddSubscription(_subscriptionAddress, RoutingSlipEvents.Completed | RoutingSlipEvents.Faulted);

            if (context.ExpirationTime.HasValue)
                builder.AddVariable("Deadline", context.ExpirationTime.Value);

            builder.AddVariable("RequestId", context.RequestId);
            builder.AddVariable("ResponseAddress", context.ResponseAddress);
            builder.AddVariable("FaultAddress", context.FaultAddress);
            builder.AddVariable("Request", context.Message);

            await BuildItinerary(builder, context);

            var routingSlip = builder.Build();

            await context.Execute(routingSlip).ConfigureAwait(false);
        }

        protected abstract Task BuildItinerary(RoutingSlipBuilder builder, ConsumeContext<TRequest> context);
    }
}