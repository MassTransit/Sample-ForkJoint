namespace ForkJoint.Components.Endpoints
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using GreenPipes;


    public class FutureResponse<TResult, TResponse> :
        ISpecification
        where TResult : class
        where TResponse : class
    {
        IResponseEndpoint<TResult> _endpoint;

        public FutureMessageFactory<TResult, TResponse> Factory
        {
            set => _endpoint = new FactoryResponseEndpoint<TResult, TResponse>(value);
        }

        public InitializerValueProvider<TResult> Initializer
        {
            set => _endpoint = new InitializerResponseEndpoint<TResult, TResponse>(value);
        }

        public IEnumerable<ValidationResult> Validate()
        {
            if (_endpoint == null)
                yield return this.Failure("Response", "Factory", "Init or Create must be configured");
        }

        public Task SendResponse(FutureConsumeContext<TResult> context, params FutureSubscription[] subscriptions)
        {
            return context.Instance.HasSubscriptions()
                ? _endpoint.SendResponse(context, subscriptions)
                : _endpoint.SendResponse(context);
        }
    }
}