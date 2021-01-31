namespace ForkJoint.Components.Endpoints
{
    using System.Threading.Tasks;
    using Internals;
    using MassTransit.Initializers;


    public class InitializerResponseEndpoint<TResult, TResponse> :
        IResponseEndpoint<TResult>
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
}