namespace ForkJoint.Components.Endpoints
{
    using System.Threading.Tasks;
    using Internals;
    using MassTransit.Initializers;


    public class InitializerResponseEndpoint<TRequest, TResult, TResponse> :
        IResponseEndpoint<TResult>
        where TRequest : class
        where TResult : class
        where TResponse : class
    {
        readonly InitializerValueProvider<TResult> _provider;

        public InitializerResponseEndpoint(InitializerValueProvider<TResult> provider)
        {
            _provider = provider;
        }

        public async Task SendResponse(FutureConsumeContext<TResult> context, params FutureSubscription[] subscriptions)
        {
            context.SetCompleted(context.Instance.CorrelationId);

            InitializeContext<TResponse> initializeContext = await MessageInitializerCache<TResponse>.Initialize(new
            {
                context.Instance.Canceled,
                context.Instance.Completed,
                context.Instance.Created,
                context.Instance.Deadline,
                context.Instance.Faulted,
                context.Instance.Location,
            }, context.CancellationToken);

            var request = context.Instance.GetRequest<TRequest>();
            if (request != null)
                initializeContext = await MessageInitializerCache<TResponse>.Initialize(initializeContext, request);

            if (context.Message != null)
                initializeContext = await MessageInitializerCache<TResponse>.Initialize(initializeContext, context.Message);

            // this is due to the way headers are propagated via the initializer
            var values = _provider(context);

            IMessageInitializer<TResponse> initializer = MessageInitializerCache<TResponse>.GetInitializer(values.GetType());

            // initialize the message and save it as the response
            InitializeContext<TResponse> messageContext = await initializer.Initialize(initializeContext, values).ConfigureAwait(false);

            context.SetResult(context.Instance.CorrelationId, messageContext.Message);

            await context.SendMessageToSubscriptions(subscriptions, initializer, initializeContext, values).ConfigureAwait(false);
        }
    }


    public class InitializerResponseEndpoint<TRequest, TResponse> :
        IResponseEndpoint
        where TRequest : class
        where TResponse : class
    {
        readonly InitializerValueProvider _provider;

        public InitializerResponseEndpoint(InitializerValueProvider provider)
        {
            _provider = provider;
        }

        public async Task SendResponse(FutureConsumeContext context, params FutureSubscription[] subscriptions)
        {
            context.SetCompleted(context.Instance.CorrelationId);

            InitializeContext<TResponse> initializeContext = await MessageInitializerCache<TResponse>.Initialize(new
            {
                context.Instance.Canceled,
                context.Instance.Completed,
                context.Instance.Created,
                context.Instance.Deadline,
                context.Instance.Faulted,
                context.Instance.Location,
            }, context.CancellationToken);

            var request = context.Instance.GetRequest<TRequest>();
            if (request != null)
                initializeContext = await MessageInitializerCache<TResponse>.Initialize(initializeContext, request);

            // this is due to the way headers are propagated via the initializer
            var values = _provider(context);

            IMessageInitializer<TResponse> initializer = MessageInitializerCache<TResponse>.GetInitializer(values.GetType());

            // initialize the message and save it as the response
            InitializeContext<TResponse> messageContext = await initializer.Initialize(initializeContext, values).ConfigureAwait(false);

            context.SetResult(context.Instance.CorrelationId, messageContext.Message);

            await context.SendMessageToSubscriptions(subscriptions, initializer, initializeContext, values).ConfigureAwait(false);
        }
    }
}