namespace ForkJoint.Components
{
    using System;
    using System.Threading.Tasks;
    using Automatonymous;
    using MassTransit;
    using MassTransit.Initializers;


    // ReSharper disable MemberCanBePrivate.Global
    // ReSharper disable UnusedAutoPropertyAccessor.Global
    public abstract class Future<TRequest, TCompleted, TFaulted> :
        MassTransitStateMachine<FutureState>
        where TRequest : class
        where TCompleted : class
        where TFaulted : class
    {
        protected Future()
        {
            InstanceState(x => x.CurrentState, WaitingForCompletion, Completed, Faulted);

            Initially(
                When(FutureRequested)
                    .InitializeFuture()
                    .TransitionTo(WaitingForCompletion)
            );

            During(WaitingForCompletion,
                When(FutureRequested)
                    .AddSubscription()
            );

            During(Completed,
                When(FutureRequested)
                    .RespondAsync(x => GetCompleted(x))
            );

            During(Faulted,
                When(FutureRequested)
                    .RespondAsync(x => GetFaulted(x))
            );
        }

        public State WaitingForCompletion { get; protected set; }
        public State Completed { get; protected set; }
        public State Faulted { get; protected set; }

        public Event<TRequest> FutureRequested { get; protected set; }

        protected virtual Task<TCompleted> GetCompleted(EventContext<FutureState> context)
        {
            if (context.Instance.TryGetResult(context.Instance.CorrelationId, out TCompleted completed))
                return Task.FromResult(completed);

            throw new InvalidOperationException("Completed result not available");
        }

        protected virtual Task<TFaulted> GetFaulted(EventContext<FutureState> context)
        {
            if (context.Instance.TryGetFault(context.Instance.CorrelationId, out TFaulted faulted))
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


    public abstract class Future<TRequest, TCompleted> :
        Future<TRequest, TCompleted, Fault<TRequest>>
        where TRequest : class
        where TCompleted : class
    {
    }
}