namespace ForkJoint.Components
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Activities;
    using Automatonymous;
    using Configurators;
    using Endpoints;
    using Internals;
    using MassTransit;
    using MassTransit.Courier.Contracts;
    using MassTransit.Metadata;


    /// <summary>
    /// RoutingSlipFuture is a durable future that sends a request to a consumer and completes or faults
    /// depending upon the response.
    /// The initiating event correlation must be declared
    /// </summary>
    /// <typeparam name="TRequest"></typeparam>
    /// <typeparam name="TResponse"></typeparam>
    // ReSharper disable MemberCanBePrivate.Global
    // ReSharper disable UnusedAutoPropertyAccessor.Global
    public abstract class RoutingSlipFuture<TRequest, TResponse> :
        Future<TRequest, TResponse>
        where TRequest : class
        where TResponse : class
    {
        readonly FutureFault<TRequest, Fault<TRequest>, RoutingSlipFaulted> _fault = new();
        readonly FutureResponse<TRequest, RoutingSlipCompleted, TResponse> _response = new();

        protected RoutingSlipFuture()
        {
            Event(() => RoutingSlipCompleted, x =>
            {
                x.CorrelateById(context => FutureIdOrFault(context.Message.Variables));
                x.OnMissingInstance(m =>
                    m.Execute(context => throw new FutureNotFoundException(typeof(TRequest), FutureIdOrDefault(context.Message.Variables))));
                x.OnMissingInstance(m => m.Fault());
                x.ConfigureConsumeTopology = false;
            });

            Event(() => RoutingSlipFaulted, x =>
            {
                x.CorrelateById(context => FutureIdOrFault(context.Message.Variables));
                x.OnMissingInstance(m =>
                    m.Execute(context => throw new FutureNotFoundException(typeof(TRequest), FutureIdOrDefault(context.Message.Variables))));
                x.ConfigureConsumeTopology = false;
            });

            Fault(fault => fault.Init(context =>
            {
                var message = context.Instance.GetRequest<TRequest>();

                return new
                {
                    FaultId = context.MessageId ?? NewId.NextGuid(),
                    FaultedMessageId = context.Message.TrackingNumber,
                    FaultMessageTypes = TypeMetadataCache<TRequest>.MessageTypeNames,
                    Host = context.Message.ActivityExceptions.Select(x => x.Host).FirstOrDefault() ?? context.Host,
                    context.Message.Timestamp,
                    Exceptions = context.Message.ActivityExceptions.Select(x => x.ExceptionInfo).ToArray(),
                    Message = message
                };
            }));

            Initially(
                When(FutureRequested)
                    .Activity(x => x.OfType<ExecuteRoutingSlipFutureActivity<TRequest>>())
            );

            DuringAny(
                When(RoutingSlipCompleted)
                    .ThenAsync(context => SetCompleted(context))
                    .TransitionTo(Completed),
                When(RoutingSlipFaulted)
                    .ThenAsync(context => SetFaulted(context))
                    .TransitionTo(Faulted)
            );
        }

        public Event<RoutingSlipCompleted> RoutingSlipCompleted { get; protected set; }
        public Event<RoutingSlipFaulted> RoutingSlipFaulted { get; protected set; }

        protected void Response(Action<IFutureResponseConfigurator<RoutingSlipCompleted, TResponse>> configure)
        {
            var configurator = new FutureResponseConfigurator<TRequest, RoutingSlipCompleted, TResponse>(_response);

            configure?.Invoke(configurator);
        }

        protected void Fault(Action<IFutureFaultConfigurator<Fault<TRequest>, RoutingSlipFaulted>> configure)
        {
            var configurator = new FutureFaultConfigurator<TRequest, Fault<TRequest>, RoutingSlipFaulted>(_fault);

            configure?.Invoke(configurator);
        }

        Task SetCompleted(BehaviorContext<FutureState, RoutingSlipCompleted> context)
        {
            FutureConsumeContext<RoutingSlipCompleted> consumeContext = context.CreateFutureConsumeContext();

            // TODO add initializer to initialize the response from variables dictionary

            return _response.SendResponse(consumeContext, consumeContext.Instance.Subscriptions.ToArray());
        }

        Task SetFaulted(BehaviorContext<FutureState, RoutingSlipFaulted> context)
        {
            FutureConsumeContext<RoutingSlipFaulted> consumeContext = context.CreateFutureConsumeContext();

            // TODO add initializer to initialize the response from variables dictionary

            return _fault.SendFault(consumeContext, consumeContext.Instance.Subscriptions.ToArray());
        }

        protected static Guid FutureIdOrFault(IDictionary<string, object> variables)
        {
            if (variables.TryGetValue(nameof(FutureConsumeContext.FutureId), out Guid correlationId))
                return correlationId;

            throw new RequestException("CorrelationId not present, define the routing slip using Event");
        }

        protected static Guid FutureIdOrDefault(IDictionary<string, object> variables)
        {
            return variables.TryGetValue(nameof(FutureConsumeContext.FutureId), out Guid correlationId) ? correlationId : default;
        }
    }
}