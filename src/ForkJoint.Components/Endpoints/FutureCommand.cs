namespace ForkJoint.Components.Endpoints
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Automatonymous;
    using GreenPipes;


    public class FutureCommand<TRequest, TCommand> :
        ISpecification
        where TRequest : class
        where TCommand : class
    {
        static readonly Default _default = new Default();
        DestinationAddressProvider<FutureState> _destinationAddressProvider;
        ICommandEndpoint<TRequest, TCommand> _endpoint;

        public FutureCommand()
        {
            _destinationAddressProvider = PublishAddressProvider;
            _endpoint = new InitializerCommandEndpoint<TRequest, TCommand>(_destinationAddressProvider, DefaultProvider);
        }

        public FutureMessageFactory<TRequest, TCommand> Factory
        {
            set => _endpoint = new FactoryCommandEndpoint<TRequest, TCommand>(_destinationAddressProvider, value);
        }

        public InitializerValueProvider<TRequest> Initializer
        {
            set => _endpoint = new InitializerCommandEndpoint<TRequest, TCommand>(_destinationAddressProvider, value);
        }

        public DestinationAddressProvider<FutureState> DestinationAddressProvider
        {
            set
            {
                _destinationAddressProvider = value;
                _endpoint.DestinationAddressProvider = value;
            }
        }

        public IEnumerable<ValidationResult> Validate()
        {
            if (_destinationAddressProvider == null)
                yield return this.Failure("DestinationAddressProvider", "must not be null");
            if (_endpoint == null)
                yield return this.Failure("Command", "Factory", "Init or Create must be configured");
        }

        static object DefaultProvider(FutureConsumeContext<TRequest> context)
        {
            return _default;
        }

        public Task SendCommand(FutureConsumeContext<TRequest> context)
        {
            return _endpoint.SendCommand(context);
        }

        static Uri PublishAddressProvider(ConsumeEventContext<FutureState> context)
        {
            return null;
        }


        class Default
        {
        }
    }
}