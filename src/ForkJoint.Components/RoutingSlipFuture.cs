namespace ForkJoint.Components
{
    using System.Linq;
    using System.Threading.Tasks;
    using Automatonymous;
    using MassTransit;
    using MassTransit.Courier.Contracts;
    using MassTransit.Metadata;


    /// <summary>
    /// RoutingSlipFuture is a durable future that sends a request to a consumer and completes or faults
    /// depending upon the response.
    /// The initiating event correlation must be declared
    /// </summary>
    /// <typeparam name="TRequest"></typeparam>
    /// <typeparam name="TCompleted"></typeparam>
    // ReSharper disable MemberCanBePrivate.Global
    // ReSharper disable UnusedAutoPropertyAccessor.Global
    public abstract class RoutingSlipFuture<TRequest, TCompleted> :
        Future<TRequest, TCompleted>
        where TRequest : class
        where TCompleted : class
    {
        protected RoutingSlipFuture()
        {
            Event(() => RoutingSlipCompleted, x =>
            {
                x.CorrelateById(context => context.Message.TrackingNumber);
                x.OnMissingInstance(m => m.Fault());
            });
            Event(() => RoutingSlipFaulted, x =>
            {
                x.CorrelateById(context => context.Message.TrackingNumber);
                x.OnMissingInstance(m => m.Fault());
            });

            Initially(
                When(FutureRequested)
                    .Activity(x => x.OfType<ExecuteRoutingSlipFutureActivity<TRequest>>())
            );

            DuringAny(
                When(RoutingSlipCompleted)
                    .SetFutureCompleted(CreateCompleted)
                    .NotifySubscribers(x => GetCompleted(x))
                    .TransitionTo(Completed),
                When(RoutingSlipFaulted)
                    .SetFutureFaulted(CreateFaulted)
                    .NotifySubscribers(x => GetFaulted(x))
                    .TransitionTo(Faulted)
            );
        }

        public Event<RoutingSlipCompleted> RoutingSlipCompleted { get; protected set; }
        public Event<RoutingSlipFaulted> RoutingSlipFaulted { get; protected set; }

        protected virtual Task<TCompleted> CreateCompleted(ConsumeEventContext<FutureState, RoutingSlipCompleted> context)
        {
            return Init<RoutingSlipCompleted, TCompleted>(context);
        }

        protected virtual Task<Fault<TRequest>> CreateFaulted(ConsumeEventContext<FutureState, RoutingSlipFaulted> context)
        {
            var message = context.Instance.GetRequest<TRequest>();

            return context.Init<Fault<TRequest>>(new
            {
                FaultId = NewId.NextGuid(),
                FaultedMessageId = context.Data.TrackingNumber,
                FaultMessageTypes = TypeMetadataCache<TRequest>.MessageTypeNames,
                context.Host,
                context.Data.Timestamp,
                Exceptions = context.Data.ActivityExceptions.Select(x => x.ExceptionInfo).ToArray(),
                Message = message
            });
        }
    }
}