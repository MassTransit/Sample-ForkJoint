namespace ForkJoint.Components.Endpoints
{
    using System.Threading.Tasks;
    using Internals;


    public class FactoryResponseEndpoint<TResult, TResponse> :
        IResponseEndpoint<TResult>
        where TResult : class
        where TResponse : class
    {
        readonly FutureMessageFactory<TResult, TResponse> _factory;

        public FactoryResponseEndpoint(FutureMessageFactory<TResult, TResponse> factory)
        {
            _factory = factory;
        }

        public async Task SendResponse(FutureConsumeContext<TResult> context, params FutureSubscription[] subscriptions)
        {
            var response = await _factory(context).ConfigureAwait(false);

            context.SetResult(context.Instance.CorrelationId, response);

            await context.SendMessageToSubscriptions(subscriptions, response).ConfigureAwait(false);
        }
    }
}