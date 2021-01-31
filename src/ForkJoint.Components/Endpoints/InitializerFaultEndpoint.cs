namespace ForkJoint.Components.Endpoints
{
    using System.Threading.Tasks;
    using Internals;
    using MassTransit;
    using MassTransit.Initializers;


    public class InitializerFaultEndpoint<TRequest, TFault, TInput> :
        IFaultEndpoint<TInput>
        where TRequest : class
        where TFault : class
        where TInput : class
    {
        readonly InitializerValueProvider<TInput> _provider;

        public InitializerFaultEndpoint(InitializerValueProvider<TInput> provider)
        {
            _provider = provider;
        }

        public async Task SendFault(FutureConsumeContext<TInput> context, params FutureSubscription[] subscriptions)
        {
            InitializeContext<TFault> initializeContext;
            if (context.Message is Fault fault)
            {
                var request = context.Instance.GetRequest<TRequest>();

                context.SetFaulted(context.Instance.CorrelationId, fault.Timestamp);

                initializeContext = await MessageInitializerCache<TFault>.Initialize(new
                {
                    fault.FaultId,
                    fault.FaultedMessageId,
                    fault.Timestamp,
                    fault.Exceptions,
                    fault.Host,
                    fault.FaultMessageTypes,
                    Message = request
                }, context.CancellationToken);

                initializeContext = await MessageInitializerCache<TFault>.Initialize(initializeContext, context.Message);
            }
            else
            {
                context.SetFaulted(context.Instance.CorrelationId);

                initializeContext = await MessageInitializerCache<TFault>.Initialize(context.Message, context.CancellationToken);
            }

            var values = _provider(context);

            IMessageInitializer<TFault> initializer = MessageInitializerCache<TFault>.GetInitializer(values.GetType());

            InitializeContext<TFault> messageContext = await initializer.Initialize(initializeContext, values).ConfigureAwait(false);

            context.SetFault(context.Instance.CorrelationId, messageContext.Message);

            await context.SendMessageToSubscriptions(subscriptions, initializer, initializeContext, values).ConfigureAwait(false);
        }
    }
}