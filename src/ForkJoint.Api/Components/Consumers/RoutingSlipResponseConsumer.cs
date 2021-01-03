namespace ForkJoint.Api.Components.Consumers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using MassTransit;
    using MassTransit.Courier.Contracts;
    using MassTransit.Events;
    using MassTransit.Metadata;
    using MassTransit.Util;


    public abstract class RoutingSlipResponseConsumer<TRequest, TResponse> :
        RoutingSlipResponseConsumer<TRequest, TResponse, Fault<TRequest>>
        where TRequest : class
        where TResponse : class
    {
        protected override Task<Fault<TRequest>> CreateFaultedResponseMessage(ConsumeContext<RoutingSlipFaulted> context, TRequest request, Guid requestId)
        {
            IEnumerable<ExceptionInfo> exceptions = context.Message.ActivityExceptions.Select(x => x.ExceptionInfo);

            Fault<TRequest> response = new FaultEvent<TRequest>(request, requestId, context.Host, exceptions, TypeMetadataCache<TRequest>.MessageTypeNames);

            return Task.FromResult(response);
        }
    }


    public abstract class RoutingSlipResponseConsumer<TRequest, TResponse, TFaultResponse> :
        IConsumer<RoutingSlipCompleted>,
        IConsumer<RoutingSlipFaulted>
        where TRequest : class
        where TResponse : class
        where TFaultResponse : class
    {
        public async Task Consume(ConsumeContext<RoutingSlipCompleted> context)
        {
            if (IsResponseExpected(context.Message.Variables, out var requestId, out var destinationAddress, out var request))
            {
                if (IsBeforeDeadline(context.Message.Variables))
                {
                    var endpoint = await context.GetResponseEndpoint<TResponse>(destinationAddress, requestId).ConfigureAwait(false);

                    var response = await CreateResponseMessage(context, request);

                    await endpoint.Send(response).ConfigureAwait(false);
                }
            }
        }

        public async Task Consume(ConsumeContext<RoutingSlipFaulted> context)
        {
            if (IsResponseExpected(context.Message.Variables, out var requestId, out var destinationAddress, out var request))
            {
                if (IsBeforeDeadline(context.Message.Variables))
                {
                    if (HasVariable(context.Message.Variables, "FaultAddress", out Uri faultAddress))
                        destinationAddress = faultAddress;

                    var endpoint = await context.GetFaultEndpoint<TResponse>(destinationAddress, requestId).ConfigureAwait(false);

                    var response = await CreateFaultedResponseMessage(context, request, requestId);

                    await endpoint.Send(response).ConfigureAwait(false);
                }
            }
        }

        static bool IsBeforeDeadline(IDictionary<string, object> variables)
        {
            return !HasVariable(variables, "Deadline", out DateTime deadline) || deadline > DateTime.UtcNow;
        }

        static bool IsResponseExpected(IDictionary<string, object> variables, out Guid requestId, out Uri destinationAddress, out TRequest request)
        {
            if (HasVariable(variables, "RequestId", out requestId)
                && HasVariable(variables, "ResponseAddress", out destinationAddress)
                && HasVariable(variables, "Request", out request))
                return true;

            destinationAddress = null;
            request = null;
            return false;
        }

        protected static bool HasVariable<T>(IDictionary<string, object> variables, string key, out T value)
        {
            if (variables.TryGetValue(key, out var obj))
            {
                value = ObjectTypeDeserializer.Deserialize<T>(obj, default);
                return true;
            }

            value = default;
            return false;
        }

        protected abstract Task<TResponse> CreateResponseMessage(ConsumeContext<RoutingSlipCompleted> context, TRequest request);

        protected abstract Task<TFaultResponse> CreateFaultedResponseMessage(ConsumeContext<RoutingSlipFaulted> context, TRequest request, Guid requestId);
    }
}