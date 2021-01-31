namespace ForkJoint.Components.Configurators
{
    using System;
    using Automatonymous;
    using Endpoints;


    public class FutureCommandConfigurator<TRequest, TCommand> :
        IFutureCommandConfigurator<TRequest, TCommand>
        where TRequest : class
        where TCommand : class
    {
        readonly FutureCommand<TRequest, TCommand> _command;

        public FutureCommandConfigurator(FutureCommand<TRequest, TCommand> command)
        {
            _command = command;
        }

        public Uri DestinationAddress
        {
            set => _command.DestinationAddressProvider = _ => value;
        }

        public DestinationAddressProvider<FutureState> DestinationAddressProvider
        {
            set => _command.DestinationAddressProvider = value;
        }

        /// <summary>
        /// Adds an object initializer to the command, on top of the <see cref="FutureState" /> and <typeparamref name="TRequest" />
        /// which are added automatically.
        /// </summary>
        /// <param name="provider"></param>
        public void Init(InitializerValueProvider<TRequest> provider)
        {
            if (provider == null)
                throw new ArgumentNullException(nameof(provider));

            _command.Initializer = provider;
        }

        public void Create(AsyncFutureMessageFactory<TRequest, TCommand> factory)
        {
            if (factory == null)
                throw new ArgumentNullException(nameof(factory));

            _command.Factory = factory;
        }
    }
}