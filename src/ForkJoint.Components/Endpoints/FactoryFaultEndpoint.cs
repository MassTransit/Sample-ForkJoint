namespace ForkJoint.Components.Endpoints
{
    using System.Threading.Tasks;
    using Internals;


    public class FactoryFaultEndpoint<TResult, TFault> :
        IFaultEndpoint<TResult>
        where TResult : class
        where TFault : class
    {
        readonly FutureMessageFactory<TResult, TFault> _factory;

        public FactoryFaultEndpoint(FutureMessageFactory<TResult, TFault> factory)
        {
            _factory = factory;
        }

        public async Task SendFault(FutureConsumeContext<TResult> context, params FutureSubscription[] subscriptions)
        {
            var fault = await _factory(context).ConfigureAwait(false);

            context.SetFault(context.Instance.CorrelationId, fault);

            await context.SendMessageToSubscriptions(subscriptions, fault).ConfigureAwait(false);
        }
    }
}