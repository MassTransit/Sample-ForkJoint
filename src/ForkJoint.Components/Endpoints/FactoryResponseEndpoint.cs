namespace ForkJoint.Components.Endpoints
{
    using System.Threading.Tasks;
    using Internals;


    public class FactoryResponseEndpoint<TResult, TResponse> :
        IResponseEndpoint<TResult>
        where TResult : class
        where TResponse : class
    {
        readonly AsyncFutureMessageFactory<TResult, TResponse> _factory;

        public FactoryResponseEndpoint(AsyncFutureMessageFactory<TResult, TResponse> factory)
        {
            _factory = factory;
        }

        public async Task SendResponse(FutureConsumeContext<TResult> context, params FutureSubscription[] subscriptions)
        {
            var response = await context.SetResult(context.Instance.CorrelationId, _factory);

            await context.SendMessageToSubscriptions(subscriptions, response).ConfigureAwait(false);
        }
    }


    public class FactoryResponseEndpoint<TResponse> :
        IResponseEndpoint
        where TResponse : class
    {
        readonly AsyncFutureMessageFactory<TResponse> _factory;

        public FactoryResponseEndpoint(AsyncFutureMessageFactory<TResponse> factory)
        {
            _factory = factory;
        }

        public async Task SendResponse(FutureConsumeContext context, params FutureSubscription[] subscriptions)
        {
            var response = await context.SetResult(context.Instance.CorrelationId, _factory).ConfigureAwait(false);

            await context.SendMessageToSubscriptions(subscriptions, response).ConfigureAwait(false);
        }
    }
}