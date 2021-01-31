namespace ForkJoint.Components.Endpoints
{
    using System.Threading.Tasks;
    using Automatonymous;
    using MassTransit.Context;
    using MassTransit.Initializers;
    using Pipeline;


    public class InitializerCommandEndpoint<TRequest, TCommand> :
        ICommandEndpoint<TRequest, TCommand>
        where TRequest : class
        where TCommand : class
    {
        readonly InitializerValueProvider<TRequest> _provider;
        DestinationAddressProvider<FutureState> _destinationAddressProvider;

        public InitializerCommandEndpoint(DestinationAddressProvider<FutureState> destinationAddressProvider, InitializerValueProvider<TRequest> provider)
        {
            DestinationAddressProvider = destinationAddressProvider;
            _provider = provider;
        }

        public DestinationAddressProvider<FutureState> DestinationAddressProvider
        {
            set => _destinationAddressProvider = value;
        }

        public async Task SendCommand(FutureConsumeContext<TRequest> context)
        {
            InitializeContext<TCommand> initializeContext = await MessageInitializerCache<TCommand>.Initialize(new
            {
                context.Instance.Canceled,
                context.Instance.Completed,
                context.Instance.Created,
                context.Instance.Deadline,
                context.Instance.Faulted,
                context.Instance.Location,
            }, context.CancellationToken);

            if (context.Message != null)
                initializeContext = await MessageInitializerCache<TCommand>.Initialize(initializeContext, context.Message);

            var destinationAddress = _destinationAddressProvider(context);

            var endpoint = destinationAddress != null
                ? await context.GetSendEndpoint(destinationAddress).ConfigureAwait(false)
                : new ConsumeSendEndpoint(await context.ReceiveContext.PublishEndpointProvider.GetPublishSendEndpoint<TCommand>().ConfigureAwait(false),
                    context, default);

            var pipe = new FutureCommandPipe<TCommand>(context.ReceiveContext.InputAddress, context.Instance.CorrelationId);

            var values = _provider(context);

            IMessageInitializer<TCommand> messageInitializer = MessageInitializerCache<TCommand>.GetInitializer(values.GetType());

            await messageInitializer.Send(endpoint, initializeContext, values, pipe).ConfigureAwait(false);
        }
    }
}