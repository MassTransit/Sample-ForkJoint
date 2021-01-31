namespace ForkJoint.Components
{
    using System;
    using System.Threading.Tasks;
    using Automatonymous;
    using Contracts;
    using Internals;
    using MassTransit;
    using MassTransit.Initializers;

    // ReSharper disable MemberCanBePrivate.Global
    // ReSharper disable MemberCanBeProtected.Global
    // ReSharper disable UnusedAutoPropertyAccessor.Global

    public abstract class Future<TRequest, TResponse, TFault> :
        MassTransitStateMachine<FutureState>
        where TRequest : class
        where TResponse : class
        where TFault : class
    {
        protected Future()
        {
            InstanceState(x => x.CurrentState, WaitingForCompletion, Completed, Faulted);

            Event(() => FutureRequested, x => x.CorrelateById(context => CorrelationIdOrFault(context)));
            Event(() => RequestFutureRequested, x => x.CorrelateById(context => CorrelationIdOrFault(context)));

            Event(() => ResponseRequested, e =>
            {
                e.CorrelateById(x => x.Message.CorrelationId);

                e.OnMissingInstance(x => x.Execute(context => throw new FutureNotFoundException(typeof(TRequest), context.Message.CorrelationId)));
            });


            Initially(
                When(FutureRequested)
                    .InitializeFuture()
                    .TransitionTo(WaitingForCompletion)
            );

            During(WaitingForCompletion,
                When(FutureRequested)
                    .AddSubscription(),
                When(ResponseRequested)
                    .AddSubscription()
            );

            During(Completed,
                When(FutureRequested)
                    .RespondAsync(x => GetCompleted(x)),
                When(ResponseRequested)
                    .RespondAsync(x => GetCompleted(x))
            );

            During(Faulted,
                When(FutureRequested)
                    .RespondAsync(x => GetFaulted(x)),
                When(ResponseRequested)
                    .RespondAsync(x => GetFaulted(x))
            );
        }

        public State WaitingForCompletion { get; protected set; }
        public State Completed { get; protected set; }
        public State Faulted { get; protected set; }

        public Event<TRequest> FutureRequested { get; protected set; }
        public Event<Contracts.Request<TRequest>> RequestFutureRequested { get; protected set; }
        public Event<Get<TRequest>> ResponseRequested { get; protected set; }

        protected static Guid RequestIdOrFault(MessageContext context)
        {
            return context.RequestId ?? throw new RequestException("RequestId not present, but required");
        }

        protected static Guid CorrelationIdOrFault(MessageContext context)
        {
            return context.CorrelationId ?? throw new RequestException("CorrelationId not present, define the request correlation using Event");
        }

        protected virtual Task<TResponse> GetCompleted(EventContext<FutureState> context)
        {
            if (context.Instance.TryGetResult(context.Instance.CorrelationId, out TResponse completed))
                return Task.FromResult(completed);

            throw new InvalidOperationException("Completed result not available");
        }

        protected virtual Task<TFault> GetFaulted(EventContext<FutureState> context)
        {
            if (context.Instance.TryGetFault(context.Instance.CorrelationId, out TFault faulted))
                return Task.FromResult(faulted);

            throw new InvalidOperationException("Faulted result not available");
        }

        protected async Task<T> Init<TData, T>(ConsumeEventContext<FutureState, TData> context, object values = default)
            where TData : class
            where T : class
        {
            InitializeContext<T> initializeContext = await MessageInitializerCache<T>.Initialize(context.Data, context.CancellationToken);

            initializeContext = await MessageInitializerCache<T>.Initialize(initializeContext, new
            {
                context.Instance.Canceled,
                context.Instance.Completed,
                context.Instance.Created,
                context.Instance.Deadline,
                context.Instance.Faulted,
                context.Instance.Location,
            });

            var request = context.Instance.GetRequest<TRequest>();
            if (request != null)
                initializeContext = await MessageInitializerCache<T>.Initialize(initializeContext, request);

            if (values != null)
                initializeContext = await MessageInitializerCache<T>.Initialize(initializeContext, values);

            return initializeContext.Message;
        }
    }


    /// <summary>
    /// Used to reference a future
    /// </summary>
    public interface Future
    {
        Guid Id { get; }

        /// <summary>
        /// The endpoint address of the future
        /// </summary>
        Uri Location { get; }
    }


    /// <summary>
    /// Used to reference a future
    /// </summary>
    /// <typeparam name="TResult"></typeparam>
    public interface Future<TResult> :
        Future
        where TResult : class
    {
    }


    /*
     * *- CorrelationId - identifies the future
    - Location - where to communicate with the future
    - Type - the completed future type
    - FaultType - the faulted future type
     */


    public abstract class Future<TRequest, TResponse> :
        Future<TRequest, TResponse, Fault<TRequest>>
        where TRequest : class
        where TResponse : class
    {
    }
}